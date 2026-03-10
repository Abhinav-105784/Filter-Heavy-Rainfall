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

        /// <summary>
        /// Writing new files after warping and deleting uneccessary files
        /// </summary>
        /// <param name="TifFiles"></param>
        /// <param name="Polygonfile"></param>
        /// <param name="number"></param>
        public static void Process(List<string> TifFiles, string Polygonfile, int number)
        {
            //Checking if inputs are correct
            if (TifFiles.Count == 0 || TifFiles == null)
            {
                MessageBox.Show("Tif files list is empty or null");
                return;
            }

            if (!File.Exists(Polygonfile))
            {
                MessageBox.Show("Provide the polygon shapefile and try again");
                return;
            }

            try
            {
                GdalConfiguration.ConfigureGdal();
                Gdal.AllRegister();
                Ogr.RegisterAll();
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error in Gdal Package : {ex.Message}");
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine($"Processing {TifFiles.Count} Tif Files against Polygon :{Path.GetFileName(Polygonfile)} \n");
            SortedDictionary<string, double> outValues = new SortedDictionary<string, double>();
            List<double> allValues = new List<double>();
            List<string> tifList = new List<string>();
           

            int success = 0, skipped = 0, failed = 0;

            // processing each tif file for warping and computing rainfall values
            foreach (var tifFile in TifFiles)
            {
                if (!File.Exists(tifFile))
                {
                    Console.WriteLine($"Skip file not found: {tifFile}");
                    skipped++;
                    continue;
                }

                string outputDir = Path.GetDirectoryName(tifFile);
                string outputPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(tifFile) + "_clipped.tif");
                Console.WriteLine($"Warping : {Path.GetFileNameWithoutExtension(tifFile)} -> {Path.GetFileNameWithoutExtension(outputPath)}");

                try
                {
                    WarpingCutline(tifFile, outputPath, Polygonfile);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("OK");
                    Console.ResetColor();

                    double avg = ComputeAverageRainfall(outputPath);
                    allValues.Add(avg);
                    tifList.Add(outputPath);
                    success++;
                }

                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed for {Path.GetFileNameWithoutExtension(tifFile)} -> {ex.Message}\n");
                    Console.ResetColor();
                    failed++;
                }
            }

            if (allValues.Count > 0 && allValues.Count>=number)
            {
                var pairs = new List<(string file, double average)>();
                for (int i = 0; i < allValues.Count; i++)
                {
                    pairs.Add((tifList[i], allValues[i]));
                }

                pairs.Sort((a, b) => b.average.CompareTo(a.average));

                for(int i=0; i < number; i++)
                {
                    string fileName = Path.GetFileName(pairs[i].file);
                    outValues[fileName] = pairs[i].average;
                }
            }
            else if( number > allValues.Count || number ==0)
            {
                Console.WriteLine("Give proper value for the numbers to gathered\n");
            }
            else
            {
                Console.WriteLine("No successful TIFFs to rank.\n");
            }
           
                Console.WriteLine($"\n=== Summary ===\n");
            Console.WriteLine($"  Success : {success}\n");
            Console.WriteLine($"  Skipped : {skipped}\n");
            Console.WriteLine($"  Failed  : {failed}\n");

            Console.WriteLine($"All top {number} files and values\n");
            
            foreach (var kvp in outValues)
            {
                Console.WriteLine($"{kvp.Key} -- {kvp.Value}");
            }

            string outputFolder = Path.GetDirectoryName(tifList.FirstOrDefault() ?? TifFiles.FirstOrDefault());
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Directory.GetCurrentDirectory(); 
            }

            // Writing csv for files that have required data
            string csvFileName = $"Top_{number}_Rainfall_Averages_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string csvPath = Path.Combine(outputFolder, csvFileName);

            try
            {
                using (var writer = new StreamWriter(csvPath))
                {
                    writer.WriteLine("Filename,Average_Rainfall");

                    foreach (var kvp in outValues)
                    {
                        writer.WriteLine($"\"{kvp.Key}\",{kvp.Value:F4}");
                    }
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nCSV file created successfully:");
                Console.WriteLine($"→ {csvPath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError while creating CSV: {ex.Message}");
                Console.ResetColor();
            }

            //Garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            System.Threading.Thread.Sleep(100);

            //Deletion of unwanted files
            HashSet<string> filesToKeep = new HashSet<string>(outValues.Keys, StringComparer.OrdinalIgnoreCase);
            foreach (string file in tifList)
            {
                string fileName = Path.GetFileName(file);
                if (!filesToKeep.Contains(fileName))
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted {fileName} as it wasn't matching the required criteria");
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Could not delete {fileName}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Simple Warping on the Raster file Individually using polygon shapefile
        /// </summary>
        /// <param name="inputTif"></param>
        /// <param name="outputTif"></param>
        /// <param name="polygonFile"></param>
        private static void WarpingCutline(string inputTif, string outputTif, string polygonFile)
        {

            inputTif = Path.GetFullPath(inputTif);
            outputTif = Path.GetFullPath(outputTif);
            polygonFile = Path.GetFullPath(polygonFile);

            using (var srcDs = Gdal.Open(inputTif, Access.GA_ReadOnly))
            {
                if (srcDs == null)
                {
                    Console.WriteLine($"Could not read/open : {inputTif}");
                    return;
                }

                string[] warpOptions = new string[]
                {
                "-crop_to_cutline",
                "-cutline",polygonFile,
                "-of","GTiff",
                "-co","TILED=YES",
                "-multi"

                };

                GDALWarpAppOptions warpOpts = null;

                try
                {
                    warpOpts = new GDALWarpAppOptions(warpOptions);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Warp options Error for {inputTif} : {ex.Message}\n");

                    return;
                }

                Dataset clippedDS = null;

                try
                {
                    clippedDS = Gdal.Warp(outputTif, new[] { srcDs }, warpOpts, null, null);
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Warping Error for {inputTif} : {ex.Message}\n");

                    return;
                }


                if (clippedDS != null)
                {
                    Console.WriteLine($"Clip successful -> {outputTif} \n");
                }

                else
                {
                    Console.WriteLine($"Gdal.Warp returned null for : {inputTif}\n");
                }

                clippedDS.FlushCache();
                clippedDS.Dispose();
            }
        }

        /// <summary>
        /// Computing the rainfall value from here for each warped raster
        /// </summary>
        /// <param name="clippedTif"></param>
        /// <returns></returns>
        public static double ComputeAverageRainfall(string clippedTif)
        {
            clippedTif = Path.GetFullPath(clippedTif);
            double average = 0;
            double max = 0;
            int cellNo = 0;

            using (var ds = Gdal.Open(clippedTif, Access.GA_ReadOnly))
            {

                if (ds == null)
                {
                    Console.WriteLine($"Could not open : {clippedTif}\n");
                    return 0;
                }

                using (var band = ds.GetRasterBand(1))
                {
                    int width = ds.RasterXSize;
                    int height = ds.RasterYSize;

                    band.GetNoDataValue(out double noDataVal, out int hasNoData);

                    float[] data = new float[width * height];
                    band.ReadRaster(0, 0, width, height, data, width, height, 0, 0);

                    double sum = 0;
                    double count = 0;

                    foreach (var value in data)
                    {
                        if (hasNoData == 1 && Math.Abs(value - noDataVal) < 1e-6)
                            continue;

                        sum += value;
                        if(value>max)
                        {
                            max = value;
                            cellNo++;
                        }
                        
                        count++;
                    }

                    if (count == 0)
                    {
                        Console.WriteLine($"No valid cells found in : {Path.GetFileName(clippedTif)}");
                    }

                    average = sum / count;

                    Console.WriteLine($"Total Cells : {count}\n");
                    Console.WriteLine($"Sum         : {sum:F4}\n");
                    Console.WriteLine($"Average     : {average:F4}\n");
                    Console.WriteLine($"Maximum Rain: {max:F4}\n");
                    Console.WriteLine($"At Cell     : {cellNo}\n");
                }

                ds.FlushCache();
            }

            return average;
        }
    }
}