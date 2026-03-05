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
                return;
            }

            // Build fixed RADOLAN WKT with correct axes
            var radolanSrs = new SpatialReference("");
            radolanSrs.ImportFromProj4(RADOLAN_PROJ4);
            radolanSrs.ExportToWkt(out string radolanWkt, null);
            radolanWkt = radolanWkt
                .Replace("AXIS[\"Easting\",SOUTH]", "AXIS[\"Easting\",EAST]")
                .Replace("AXIS[\"Northing\",SOUTH]", "AXIS[\"Northing\",NORTH]");
            Console.WriteLine($"Fixed RADOLAN WKT: {radolanWkt}\n");

            foreach (var ascPath in ascFiles)
            {
                if (!File.Exists(ascPath))
                {
                    Console.WriteLine($"File not found, skipping: {ascPath}");
                    continue;
                }

                string tempPath = Path.ChangeExtension(ascPath, null) + "_TEMP.tif";

                try
                {
                    Console.WriteLine($"Processing: {Path.GetFileName(ascPath)}\n");

                    // ── Step 1: Open ASC and copy to MEM ────────────────────────────
                    var srcDsReadOnly = Gdal.Open(ascPath, Access.GA_ReadOnly);
                    if (srcDsReadOnly == null)
                    {
                        Console.WriteLine($"Cannot open: {ascPath}");
                        continue;
                    }

                    var memDriver = Gdal.GetDriverByName("MEM");
                    Dataset memDs = memDriver.CreateCopy("", srcDsReadOnly, 0, null, null, null);
                    srcDsReadOnly.Dispose();

                    if (memDs == null)
                    {
                        Console.WriteLine($"MEM copy failed: {ascPath}");
                        continue;
                    }

                    // ── Step 2: Set fixed projection on MEM ─────────────────────────
                    CPLErr projResult = memDs.SetProjection(radolanWkt);
                    Console.WriteLine(projResult == CPLErr.CE_None
                        ? "Projection set successfully."
                        : $"WARNING: SetProjection failed: {Gdal.GetLastErrorMsg()}");

                    double[] gt = new double[6];
                    memDs.GetGeoTransform(gt);
                    Console.WriteLine($"GeoTransform: {string.Join(", ", gt)}");
                    Console.WriteLine($"Determinant: {gt[1] * gt[5] - gt[2] * gt[4]}\n");

                    // ── Step 3: Write MEM to TEMP.tif ───────────────────────────────
                    var gtiffDriver = Gdal.GetDriverByName("GTiff");
                    Dataset tempDs = gtiffDriver.CreateCopy(tempPath, memDs, 0, null, null, null);
                    memDs.Dispose();

                    if (tempDs == null)
                    {
                        Console.WriteLine($"Failed to create TEMP.tif: {tempPath}");
                        continue;
                    }

                    tempDs.FlushCache();
                    tempDs.Dispose();
                    Console.WriteLine($"TEMP.tif created: {tempPath}\n");

                    // ── Step 4: Divide by 10 if recent RADOLAN ───────────────────────
                    if (IsRecentRADOLAN(ascPath))
                    {
                        Dataset tempSrc = Gdal.Open(tempPath, Access.GA_ReadOnly);
                        if (tempSrc == null)
                        {
                            Console.WriteLine($"Cannot open TEMP.tif for divide: {tempPath}");
                            continue;
                        }

                        Dataset dividedDs = DivideRasterBy10(tempSrc);
                        tempSrc.Dispose();

                        // Overwrite TEMP.tif with divided values
                        string tempPath2 = Path.ChangeExtension(ascPath, null) + "_TEMP2.tif";
                        Dataset dividedTempDs = gtiffDriver.CreateCopy(tempPath2, dividedDs, 0, null, null, null);
                        dividedDs.Dispose();
                        dividedTempDs.FlushCache();
                        dividedTempDs.Dispose();

                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        File.Move(tempPath2, tempPath);

                        Console.WriteLine("Values divided by 10, TEMP.tif updated.\n");
                    }
                    else
                    {
                        Console.WriteLine("Not recent RADOLAN, skipping divide by 10.\n");
                    }

                    Console.WriteLine($"Done -> {tempPath}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {ascPath}: {ex.Message}");
                }
            }

            Console.WriteLine("All files processed.");
        }

        public static bool IsRecentRADOLAN(string ascPath)
        {
            var lines = File.ReadLines(ascPath).ToArray();
            return lines.Take(10).Any(l => l.Trim().StartsWith("nrows 900", StringComparison.OrdinalIgnoreCase));
        }

        public static Dataset DivideRasterBy10(Dataset ds)
        {
            int width = ds.RasterXSize, height = ds.RasterYSize, bands = ds.RasterCount;
            var memDriver = Gdal.GetDriverByName("MEM");
            Dataset newDs = memDriver.Create("", width, height, bands, DataType.GDT_Float32, null);

            double[] gt = new double[6];
            ds.GetGeoTransform(gt);
            newDs.SetGeoTransform(gt);
            newDs.SetProjection(ds.GetProjection());

            for (int i = 1; i <= bands; i++)
            {
                Band srcBand = ds.GetRasterBand(i);
                Band dstBand = newDs.GetRasterBand(i);
                float[] buffer = new float[width * height];

                if (srcBand.ReadRaster(0, 0, width, height, buffer, width, height, 0, 0) != CPLErr.CE_None)
                {
                    Console.WriteLine($"Read error band {i}: {Gdal.GetLastErrorMsg()}");
                    continue;
                }

                for (int j = 0; j < buffer.Length; j++)
                    if (buffer[j] != -1f) buffer[j] /= 10.0f;

                if (dstBand.WriteRaster(0, 0, width, height, buffer, width, height, 0, 0) != CPLErr.CE_None)
                    Console.WriteLine($"Write error band {i}: {Gdal.GetLastErrorMsg()}");
            }

            return newDs;
        }
    }
}