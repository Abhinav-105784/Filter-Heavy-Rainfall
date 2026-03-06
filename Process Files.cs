using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;

namespace Filtering_Rainfall_Asc
{
    internal class Process_Files
    {
        public Process_Files() { }
        private const string RADOLAN_PROJ4 = "+proj=stere +lat_0=90 +lat_ts=60 +lon_0=10 +k=1 +x_0=0 +y_0=0 +a=6370040 +b=6370040 +units=m +no_defs";
        public static void Process(List<string> ascFiles, string file, int number)
        {
            try
            {
                GdalConfiguration.ConfigureGdal();
                Gdal.AllRegister();
                Ogr.RegisterAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"GDAL initialization Failed : {ex.Message}");
                Console.WriteLine($"GDAL initialization Failed : {ex.Message}");
                return;
            }
            // Build RADOLAN SRS and fix WKT axes manually (no OSRAxisMappingStrategy)
            var radolanSrs = new SpatialReference("");
            radolanSrs.ImportFromProj4(RADOLAN_PROJ4);
            radolanSrs.ExportToWkt(out string radolanWkt, null);
            // Fixed: Manually replace axes to EAST/NORTH (avoids inversion error)
            radolanWkt = radolanWkt
                .Replace("AXIS[\"Easting\",SOUTH]", "AXIS[\"Easting\",EAST]")
                .Replace("AXIS[\"Northing\",SOUTH]", "AXIS[\"Northing\",NORTH]");
            Console.WriteLine($"Fixed RADOLAN WKT: {radolanWkt}\n");
            string sourceSrs = null;
            SpatialReference spatialReference = null;
            try
            {
                var shpDs = Ogr.Open(file, 0);
                if (shpDs == null)
                {
                    MessageBox.Show("Unable to open the Shapefile");
                    return;
                }
                var layerCount = shpDs.GetLayerByIndex(0);
                var srs = layerCount.GetSpatialRef();
                Console.WriteLine(srs.GetName() + "\n");
                if (srs == null || srs.IsProjected() == 0 && srs.IsGeographic() == 0)
                {
                    Console.WriteLine("No SRS/PRJ found in Shapefile!");
                    MessageBox.Show("Either SRS or Proj file missing with Shapefile");
                }
                else
                {
                    string wkt;
                    srs.ExportToWkt(out wkt, null);
                    sourceSrs = wkt;
                    Console.WriteLine("Shapefile CRS: " + wkt);
                    spatialReference = srs;
                }
                shpDs.Dispose();
                if (layerCount == null)
                {
                    MessageBox.Show("There are no layers in the Shapefile");
                    return;
                }
                Console.WriteLine($"Polygon union is Ready features Processed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while Processing Shapefile: {ex.Message}");
                return;
            }
            foreach (var ascPath in ascFiles)
            {
                if (!File.Exists(ascPath))
                {
                    Console.WriteLine($"Didn't found the file, Skipping {ascPath}");
                    continue;
                }
                string outputPath = Path.ChangeExtension(ascPath, null) + "_clipped.tif";
                try
                {
                    var srcDs = Gdal.Open(ascPath, Access.GA_ReadOnly);
                    if (srcDs == null)
                    {
                        Console.WriteLine($"ASC file not opening: {ascPath}");
                        continue;
                    }
                    Console.WriteLine($"Processing: {Path.GetFileName(ascPath)} -> {Path.GetFileName(outputPath)}");
                    srcDs.SetProjection(radolanWkt);  // Fixed: Use fixed WKT
                    Console.WriteLine("RADOLAN projection set");
                    string currentSRS1 = srcDs.GetProjection();
                    Console.WriteLine($"ASC File Projection : {currentSRS1}");
                    if (IsRecentRADOLAN(ascPath))
                    {
                        var divideDS = DivideRasterBy10(srcDs);
                        srcDs.Dispose();
                        srcDs = divideDS;
                        Console.WriteLine("Values divided by 10 (recent data)");
                    }
                    else
                    {
                        Console.WriteLine("Historical data (no divide needed)");
                    }
                    string[] warpOptionsArray = new string[]
                    {
                        "-of","GTiff",
                        "-r","near",
                        "-cutline", file,
                        "-crop_to_cutline",
                        "-s_srs", radolanWkt,  // Source SRS (fixed WKT)
                        "-t_srs", sourceSrs,  // Target SRS (shapefile)
                        "TILED=YES",
                        "COMPRESS=DEFLATE"
                    };
                    GDALWarpAppOptions warpOpts = null;
                    try
                    {
                        warpOpts = new GDALWarpAppOptions(warpOptionsArray);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception : {ex.Message}\n");
                    }
                    Dataset clippedDS = null;
                    try
                    {
                        clippedDS = Gdal.Warp(outputPath, new[] { srcDs }, warpOpts, null, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception1 :{ex.Message}\n");
                    }
                    if (clippedDS != null)
                    {
                        Console.WriteLine($"Clip successful ->{outputPath}");
                        clippedDS.Dispose();
                    }
                    else
                    {
                        Console.WriteLine($"Warp failed! Error: {Gdal.GetLastErrorMsg()}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in {ascPath} : {ex.Message}");
                }
            }
            Console.WriteLine("All ASC files successfully clipped");
        }
        public static bool IsRecentRADOLAN(string ascPath)
        {
            var lines = File.ReadLines(ascPath).ToArray();
            return lines.Length > 1 && lines[1].StartsWith("nrows 900");
        }
        public static Dataset DivideRasterBy10(Dataset ds)
        {
            int width = ds.RasterXSize, height = ds.RasterYSize, bands = ds.RasterCount;
            var driver = Gdal.GetDriverByName("MEM");
            Dataset newDs = driver.Create("", width, height, bands, DataType.GDT_Float32, null);
            double[] geoTransform = new double[6];
            ds.GetGeoTransform(geoTransform);
            newDs.SetGeoTransform(geoTransform);
            string projection = ds.GetProjection();
            newDs.SetProjection(projection);
            for (int i = 1; i <= bands; i++)
            {
                Band srcBand = ds.GetRasterBand(i);
                Band dstBand = newDs.GetRasterBand(i);
                float[] buffer = new float[width * height];
                CPLErr readResult = srcBand.ReadRaster(0, 0, width, height, buffer, width, height, 0, 0);
                if (readResult != CPLErr.CE_None)
                {
                    Console.WriteLine($"Read Error on band {i}: {Gdal.GetLastErrorMsg()}");
                    continue;
                }
                for (int j = 0; j < buffer.Length; j++)
                {
                    if (buffer[j] != -1)
                    {
                        buffer[j] /= 10.0f;
                    }
                }
                CPLErr writeResult = dstBand.WriteRaster(0, 0, width, height, buffer, width, height, 0, 0);
                if (writeResult != CPLErr.CE_None)
                {
                    Console.WriteLine($"Write Error on band {i} : {Gdal.GetLastErrorMsg()}");
                }
            }
            return newDs;
        }
    }
}