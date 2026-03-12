using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using System.Threading.Tasks;

namespace Filtering_Rainfall_Asc
{
    internal class ReadingRaster
    {
        /// <summary>
        /// Getting out the List of cell number and value of rainfall in that cell from raster
        /// </summary>
        /// <param name="rasterFile"></param>
        /// <returns></returns>
        public static List<(int, double)> Values(string rasterFile)
        {
            rasterFile = Path.GetFullPath(rasterFile);

            List<(int, double)> output = new List<(int, double)>();
            using (var ds = Gdal.Open(rasterFile, Access.GA_ReadOnly))
            {
                if (ds == null)
                {
                    Console.WriteLine($"Could not open : {rasterFile}.\n");
                    return null;
                }

                using (var band = ds.GetRasterBand(1))
                {
                    int width = ds.RasterXSize;
                    int height = ds.RasterYSize;

                    band.GetNoDataValue(out double noDataVal, out int hasNoVal);

                    float[] data = new float[width * height];
                    band.ReadRaster(0, 0, width, height, data, width, height, 0, 0);

                    int cellNo = 0;
                    int count = 0;

                    foreach (var cell in data)
                    {
                        if (!(hasNoVal == 1 && Math.Abs(cell - noDataVal) < 1e-6))
                        {
                            output.Add((cellNo, cell));
                            count++;

                        }
                        cellNo++;
                    }

                    if (count == 0)
                    {
                        Console.WriteLine($"No Valid cells found in : {Path.GetFileName(rasterFile)}\n");
                        return null;
                    }
                }
                ds.FlushCache();
            }
            return output;
        }

        public static void WriteRainfallMatrix(List<string> files, string csvFile)
        {
            List<List<(int cell, double value)>> allData = new List<List<(int cell, double value)>>();

            List<string> times = new List<string>();

            int maxCells = 0;

            foreach(string file in files)
            {
                var values = Values(file);

                if (values == null) continue;

                allData.Add(values);

                maxCells = Math.Max(maxCells, values.Count);

                string time = Path.GetFileNameWithoutExtension(file).Split('_').Last();

                times.Insert(0,time);
            }

            using (var writer = new StreamWriter(csvFile))
            {
                //Header Row
                writer.Write("Time,FileName");

                for(int i=0;i< maxCells;i++)
                {
                    writer.Write($",cell{i}");
                }

                writer.WriteLine();

                //Data rows

                for (int i = 0; i < allData.Count; i++)
                {
                    writer.Write($"{times[i]},{Path.GetFileName(files[i])}");

                    var row = allData[i];

                    foreach(var cell in row)
                    {
                        writer.Write($",{cell.value}");
                    }
                    writer.WriteLine();
                }

            }

            Console.WriteLine($"Excel matrix saved to : {csvFile}");
        }
    }
}
