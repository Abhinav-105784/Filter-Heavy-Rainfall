using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;

namespace Filtering_Rainfall_Asc
{
    internal class Process_Files
    {

        public Process_Files() { }
        private const string RADOLAN_PRJ = "PROJCS[\"Stereographic_North_Pole\",GEOGCS[\"GCS_unnamed ellipse\",DATUM[\"D_unknown\",SPHEROID[\"Unknown\",6370040,0]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Stereographic_North_Pole\"],PARAMETER[\"standard_parallel_1\",60],PARAMETER[\"central_meridian\",10],PARAMETER[\"false_easting\",0],PARAMETER[\"false_northing\",0],UNIT[\"Meter\",1]]";
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
            string radolanWktTest = RADOLAN_PRJ;
            var testSrs = new SpatialReference("");
            testSrs.ImportFromWkt(ref radolanWktTest);
            Console.WriteLine($"GDAL interprets axes as: {testSrs.GetAxisName(null, 0)}, {testSrs.GetAxisName(null, 1)}");

            // ── Build a clean RADOLAN WKT from PROJ4 that GDAL understands ─────
            var radolanSrs = new SpatialReference("");
            radolanSrs.ImportFromProj4(RADOLAN_PROJ4);
            radolanSrs.ExportToWkt(out string radolanWkt, null);
            Console.WriteLine($"RADOLAN WKT from PROJ4: {radolanWkt}");
            string sourceSrs = null;
            //Geometry cutLine = null;
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
                //spatialReference = srs;
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
                    var srcDsReadOnly = Gdal.Open(ascPath, Access.GA_ReadOnly);
                    if (srcDsReadOnly == null)
                    {
                        Console.WriteLine($"ASC file not opening: {ascPath}");
                        continue;
                    }

                    var memDriver = Gdal.GetDriverByName("MEM");
                    Dataset srcDs = memDriver.CreateCopy("", srcDsReadOnly, 0, null, null, null);
                    srcDsReadOnly.Dispose();

                    if (srcDs == null)
                    {
                        Console.WriteLine($"Failed to copy the MEM dataset: {ascPath}");
                        continue;
                    }
                    //string currentSRS = srcDs.GetProjectionRef();
                    //Console.WriteLine($"ASC File Projection : {currentSRS}");
                    //srcDs.SetSpatialRef(spatialReference);
                    //srcDs.SetProjection(sourceSrs);
                    //string currentSRS1 = srcDs.GetProjection();
                    //Console.WriteLine($"ASC File Projection : {currentSRS1}");
                    Console.WriteLine($"Processing: {Path.GetFileName(ascPath)} -> {Path.GetFileName(outputPath)}");

                    //CPLErr projResult = srcDs.SetProjection(RADOLAN_PRJ);
                    //if (projResult != CPLErr.CE_None)
                    //    Console.WriteLine($"WARNING: SetProjection failed: {Gdal.GetLastErrorMsg()}");
                    //else
                    //    Console.WriteLine("RADOLAN projection set successfully.");
                    //Console.WriteLine("RADOLAN projection set");
                    //string currentSRS1 = srcDs.GetProjection();
                    //Console.WriteLine($"ASC File Projection : {currentSRS1}");

                    CPLErr projResult = srcDs.SetProjection(radolanWkt);
                    if (projResult != CPLErr.CE_None)
                        Console.WriteLine($"WARNING: SetProjection failed: {Gdal.GetLastErrorMsg()}");
                    else
                        Console.WriteLine("RADOLAN projection set successfully.");

                    Console.WriteLine($"Projection on dataset: {srcDs.GetProjection()}");

                    double[] geoTransform = new double[6];
                    srcDs.GetGeoTransform(geoTransform);
                    Console.WriteLine(string.Join(",", geoTransform));

                    string tempPath = Path.ChangeExtension(ascPath, null) + "_TEMP.tif";

                    var gtiffDriver = Gdal.GetDriverByName("GTiff");

                    Dataset tempDs = gtiffDriver.CreateCopy(tempPath, srcDs, 0, null, null, null);

                    tempDs.SetProjection(RADOLAN_PRJ);
                    tempDs.FlushCache();
                    tempDs.Dispose();
                    srcDs.Dispose();

                    Dataset warpSrc = Gdal.Open(tempPath, Access.GA_ReadOnly);
                    Console.WriteLine($"Temp File projection : {warpSrc.GetProjection()}");
                    if (IsRecentRADOLAN(ascPath))
                    {
                        var divideDS = DivideRasterBy10(srcDs);
                        srcDs.Dispose();
                        srcDs = divideDS;
                        Console.WriteLine("Values divided by 10 (recent data)");
                    }
                    else
                    {
                        Console.WriteLine("Unable to process ASCII to raster properly");
                    }

                    Console.WriteLine($"RasterX: {srcDs.RasterXSize}, RasterY: {srcDs.RasterYSize}, Bands: {srcDs.RasterCount}");
                    Console.WriteLine($"Projection: {srcDs.GetProjection()}");

                    //var testSrs = new SpatialReference("");
                    //string RADOLAN_PRJ_var = RADOLAN_PRJ;
                    //testSrs.ImportFromWkt(ref RADOLAN_PRJ_var); // your official WKT
                    //Console.WriteLine($"GDAL interprets axes as: {testSrs.GetAxisName(null, 0)}, {testSrs.GetAxisName(null, 1)}");

                    double[] gt = new double[6];
                    srcDs.GetGeoTransform(gt);
                    Console.WriteLine($"GeoTransform: {string.Join(", ", gt)}");

                    double det = gt[1] * gt[5] - gt[2] * gt[4];
                    Console.WriteLine($"GeoTransform determinant: {det}");

                    //Gdal.SetConfigOption("CPL_DEBUG", "ON");
                    //Gdal.SetConfigOption("GDAL_DATA", "ON");

                    //string[] warpOptionsArray = new string[]
                    //{
                    //    "-of","GTiff",
                    //    "-r","near",
                    //    "-cutline", file,
                    //   // "-cutline_srs", sourceSrs,
                    //    "-crop_to_cutline",
                    //    //"-s_srs",RADOLAN_PRJ,
                    //     "-co","TILED=YES"
                    //};



                    // Use the same WKT in warp options
                    string[] warpOptionsArray = new string[]
                    {
                      "-of", "GTiff",
                       "-r", "near",
                       "-cutline", file,
                       "-cutline_srs", sourceSrs,   // tell GDAL cutline is in UTM32N
                       "-crop_to_cutline",
                      // "-s_srs", radolanWkt,        // explicitly pass the clean WKT
                       "-co", "TILED=YES"
                    };
                    Console.WriteLine("Warp options: " + string.Join(" ", warpOptionsArray) + "\n");

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
                        clippedDS = Gdal.Warp(outputPath, new[] { warpSrc }, warpOpts, null, null);
                        warpSrc.Dispose();
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
                        Console.WriteLine($"GDAL Last Error: {Gdal.GetLastErrorMsg()}");
                        Console.WriteLine($"GDAL Last Error No: {Gdal.GetLastErrorNo()}");
                    }
                    if(File.Exists(tempPath)) File.Delete(tempPath);

                    //File.Delete(tempWktFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in {ascPath} : {ex.Message}");
                    return;
                }
            }

            //cutLine?.Dispose();
            Console.WriteLine("All ASC files successfully clipped");
        }
        public static bool IsRecentRADOLAN(string ascPath)
        {
            var lines = File.ReadLines(ascPath).ToArray();
            return lines.Take(10).Any(l => l.Trim().StartsWith("nrows 900", StringComparison.OrdinalIgnoreCase));
        }
        public static Dataset DivideRasterBy10(Dataset ds)
        {
            int width = ds.RasterXSize, height = ds.RasterYSize, bands = ds.RasterCount;
            var driver = Gdal.GetDriverByName("MEM");
            Dataset newDs = driver.Create("", width, height, bands, DataType.GDT_Float32, null);
            double[] geoTransform = new double[6];
            ds.GetGeoTransform(geoTransform);
            newDs.SetGeoTransform(geoTransform);
            newDs.SetProjection(ds.GetProjection());

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
                    Console.WriteLine($"Read Error onb band {i}: {Gdal.GetLastErrorMsg()}");
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
                    Console.WriteLine($"Write Error on band : {i} : {Gdal.GetLastErrorMsg()}");
                }
            }
            return newDs;
        }
    }
}