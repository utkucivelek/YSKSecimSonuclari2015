using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YSKScraper
{
    public class YandexGeocoder
    {
        public static void GeocodeAndCombine(string addressFile,string outputFile)
        {
            // geocode them all
            YandexGeocoder.GeocodePass1(addressFile, "geocodedOutputPass1.txt");

            //try to fix unknown addresses
            YandexGeocoder.GeocodePass2(addressFile, "geocodedOutputPass1.txt", "geocodedOutputPass2.txt");

            //try to fix duplicate addresses
            YandexGeocoder.GeocodePass3(addressFile, "geocodedOutputPass2.txt", "geocodedOutputPass3.txt");

            //try to fix again
            YandexGeocoder.GeocodePass2(addressFile, "geocodedOutputPass3.txt", "geocodedOutputPassFinal.txt");

            // create the output as csv
            YandexGeocoder.CombineFiles(addressFile, "geocodedOutputPassFinal.txt", outputFile);

            // cleanup
            File.Delete("geocodedOutputPass1.txt");
            File.Delete("geocodedOutputPass2.txt");
            File.Delete("geocodedOutputPass3.txt");
            File.Delete("geocodedOutputPassFinal.txt");

        }

        /// <summary>
        /// geocodes all addresses in given file.
        /// </summary>
        /// <param name="addressFile">input address file (one address per line)</param>
        /// <param name="outputFile">output coordinates will be written here.</param>
        public static void GeocodePass1(string addressFile, string outputFile)
        {
            // Resume from last session
            int startIndex = 0;
            if (File.Exists(outputFile))
            {
                // get the last id
                var tmpLines = File.ReadAllLines(outputFile);
                var lastline = tmpLines.Last();
                var tokens = lastline.Split(' ');
                startIndex = Convert.ToInt32(tokens[0]);
            }

            // read the contents, and loop through distinct values 
            string[] lines = File.ReadAllLines(addressFile, Encoding.UTF8).Distinct().ToArray();
            //       File.WriteAllLines("distinctMahalle.txt", lines);
            using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
            {
                for (int i = startIndex; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    Console.Write("{0}/{1} {2} ", i + 1, lines.Length, line);
                    try
                    {
                        string address = String.Format("http://geocode-maps.yandex.ru/1.x/?geocode={0}&lang=en-US&results=1", line);
                        string result = wc.DownloadString(address);

                        // extract the values between <pos>...</val>
                        string latlonStr = string.Empty;
                        int pos1 = result.IndexOf("<pos>");
                        if (pos1 == -1) // means no results found.
                        {
                            latlonStr = "---";
                        }
                        else
                        {
                            int pos2 = result.IndexOf("</pos>", pos1 + 1);
                            latlonStr = result.Substring(pos1 + 5, pos2 - (pos1 + 5));
                        }
                        Console.WriteLine(latlonStr);
                        File.AppendAllText(outputFile, String.Format("{0} {1} {2}", i + 1, latlonStr, Environment.NewLine), Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(Environment.NewLine + ex.Message);
                        break;
                    }
                }
            }
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }

        /// <summary>
        /// tries to fix "---" values in the inputCoordsFile 
        /// </summary>
        /// <param name="addressFile">input address file (one address per line)</param>
        /// <param name="inputCoordsFile">input coordinates file to be reprocessed by trying different address styles.</param>
        /// <param name="outputFile">output coordinates will be written here.</param>
        public static void GeocodePass2(string addressFile, string inputCoordsFile, string outputFile)
        {
            string[] addressLines = File.ReadAllLines(addressFile, Encoding.UTF8).Distinct().ToArray();
            string[] coordLines = File.ReadAllLines(inputCoordsFile, Encoding.UTF8);

            // find the lines with ---
            var missingCoordLines = coordLines.Where(line => line.Contains("---"));
            if (missingCoordLines.Count() == 0) return; // nothing to do

            using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
            {
                // loop through all missing entries
                foreach (string line in missingCoordLines)
                {
                    var tokens = line.Split(' ');
                    int index = Convert.ToInt32(tokens[0]) - 1;
                    string address = addressLines[index];

                    var addressTokens = address.Split(',');
                    string il = addressTokens[0];
                    string ilce = addressTokens[1];
                    string mah = addressTokens[2];

                    // replace -1 -2 in il & ilce
                    il = il.Replace("-1", "");
                    il = il.Replace("-2", "");
                    il = il.Replace("-3", "");
                    il = il.Replace("-4", "");

                    // ignore MERKEZ ilce
                    if (ilce.Contains("MERKEZ")) ilce = "";
                    else
                    {
                        ilce = ilce.Replace("-1", "");
                        ilce = ilce.Replace("-2", "");
                        ilce = ilce.Replace("-3", "");
                        ilce = ilce.Replace("-4", "");
                    }

                    // try to get a valid response by modifying addresses
                    //   mah = mah.Replace("-", " ");
                    //   if (mah.Contains("MERKEZ")) il = "";

                    address = string.Format("{0} {1} {2}", il, ilce, mah);
                    Console.Write("{0} {1} ", index + 1, address);

                    // send query
                    try
                    {
                        string url = String.Format("http://geocode-maps.yandex.ru/1.x/?geocode={0}&lang=en-US&results=1", address);
                        string result = wc.DownloadString(url);

                        // extract the values between <pos>...</val>
                        string latlonStr = string.Empty;
                        int pos1 = result.IndexOf("<pos>");
                        if (pos1 == -1) // means no results found.
                        {
                            latlonStr = "---";
                        }
                        else
                        {
                            int pos2 = result.IndexOf("</pos>", pos1 + 1);
                            latlonStr = result.Substring(pos1 + 5, pos2 - (pos1 + 5));
                        }
                        Console.WriteLine(latlonStr);
                        coordLines[index] = String.Format("{0} {1}", index + 1, latlonStr);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(Environment.NewLine + ex.Message);
                        break;
                    }

                }
            }
            File.WriteAllLines(outputFile, coordLines, Encoding.UTF8);

            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }

        /// <summary>
        /// replaces duplicate coords with "---"
        /// </summary>
        /// <param name="addressFile">input address file (one address per line)</param>
        /// <param name="inputCoordsFile">input coordinates file to be checked for duplicates</param>
        /// <param name="outputFile">output coordinates will be written here.</param>
        public static void GeocodePass3(string addressFile, string inputCoordsFile, string outputFile)
        {
            string[] addressLines = File.ReadAllLines(addressFile, Encoding.UTF8).Distinct().ToArray();
            string[] coordLines = File.ReadAllLines(inputCoordsFile, Encoding.UTF8);

            // find the duplicate coords 
            Dictionary<string, int> dupeTestDict = new Dictionary<string, int>();
            HashSet<int> dupeIndices = new HashSet<int>();

            foreach (string line in coordLines)
            {
                var tokens = line.Split(' ');
                if (tokens[1].Equals("---")) continue;

                int index = Convert.ToInt32(tokens[0]) - 1;
                string coord = string.Format("{0} {1}", tokens[1], tokens[2]);

                if (dupeTestDict.ContainsKey(coord))
                {
                    Console.WriteLine("Dupe found: {0}", addressLines[index]);

                    dupeIndices.Add(dupeTestDict[coord]);
                    dupeIndices.Add(index);
                    // dupeTestDict.Remove(coord);
                }
                else
                {
                    dupeTestDict.Add(coord, index);
                }

                string address = addressLines[index];
            }

            foreach (int index in dupeIndices)
            {
                coordLines[index] = string.Format("{0} {1}", index + 1, "---");
            }

            File.WriteAllLines(outputFile, coordLines, Encoding.UTF8);

            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }

        /// <summary>
        /// combines address and coordinate files as a csv file
        /// </summary>
        /// <param name="addressFile">input address file (one address per line)</param>
        /// <param name="inputCoordsFile">input coordinates file</param>
        /// <param name="outputFile">output csv file</param>
        public static void CombineFiles(string addressFile, string inputCoordsFile, string outputFile)
        {
            string[] addressLines = File.ReadAllLines(addressFile, Encoding.UTF8).Distinct().ToArray();
            string[] coordLines = File.ReadAllLines(inputCoordsFile, Encoding.UTF8);

            StreamWriter outFile = File.CreateText(outputFile);
            outFile.WriteLine("[il],[ilce],[mahalle/koy],[lat],[lon]");
            for (int i = 0; i < addressLines.Length; i++)
            {
                string address = addressLines[i];
                if (string.IsNullOrWhiteSpace(address)) continue;
                var coordTokens = coordLines[i].Split(' ');
                if (coordTokens.Length == 2)
                {
                    outFile.WriteLine("{0}, ---, ---", address);
                }
                else
                {
                    outFile.WriteLine("{0}, {1}, {2}", address, coordTokens[2], coordTokens[1]);
                }
            }
            outFile.Close();

        }

        /// <summary>
        /// for filtering certain addresses to help debugging
        /// </summary>
        private static void Filter()
        {
            string[] addressLines = File.ReadAllLines("distinctMahalle.txt", Encoding.UTF8);
            string[] coordLines = File.ReadAllLines("geocodedOutputPass3.txt", Encoding.UTF8);

            // find the lines with ---
            var missingCoordLines = coordLines.Where(line => line.Contains("---"));
            if (missingCoordLines.Count() == 0) return; // nothing to do

            List<int> filteredIndices = new List<int>();
            foreach (string line in missingCoordLines)
            {
                var tokens = line.Split(' ');
                int index = Convert.ToInt32(tokens[0]) - 1;
                string address = addressLines[index];

                var addressTokens = address.Split(',');
                string il = addressTokens[0];
                string ilce = addressTokens[1];
                string mah = addressTokens[2];

                // replace -1 -2 in il & ilce
                il = il.Replace("-1", "");
                il = il.Replace("-2", "");
                il = il.Replace("-3", "");
                il = il.Replace("-4", "");

                if (ilce.Contains("MERKEZ")) ilce = "";
                else
                {
                    ilce = ilce.Replace("-1", "");
                    ilce = ilce.Replace("-2", "");
                    ilce = ilce.Replace("-3", "");
                    ilce = ilce.Replace("-4", "");
                }

                if (il.Trim().Equals("İSTANBUL"))
                {
                    address = string.Format("{0} {1} {2}", il, ilce, mah);
                    filteredIndices.Add(index);
                    Console.WriteLine("{0}", address);
                }

            }
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }
    }
}
