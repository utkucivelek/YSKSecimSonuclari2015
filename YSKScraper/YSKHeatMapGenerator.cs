using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSKScraper
{
    public class YSKHeatMapGenerator
    {
        protected string[] coordLines=null;
        protected string[] electionDataLines=null;

        public void Init(string addressFile,string electionDataFile)
        {
            coordLines = File.ReadAllLines(addressFile, Encoding.UTF8);
            electionDataLines = File.ReadAllLines(electionDataFile, Encoding.UTF8);
        }

        public void Generate(string partyCodename,string outFilename,double multiplier=1.0)
        {
            if(coordLines==null || electionDataLines==null)
            {
                throw new NullReferenceException("Init not called.");
            }
            
            // create a dictionary with unique address keys and location data
            Dictionary<string, string[]> addressDict = new Dictionary<string, string[]>();
            for (int i = 1; i < coordLines.Length; i++)
            {
                var tokens = coordLines[i].Split(',');
                if (tokens[3].Trim() == "---") continue;
                string addressKey = string.Format("{0},{1},{2}", tokens[0].Trim(), tokens[1].Trim(), tokens[2].Trim());
                addressDict.Add(addressKey, new string[] { tokens[3].Trim(), tokens[4].Trim() });
            }

            // create dictionaries to count the party and overall vote for a given address
            Dictionary<string, int> partyVotesDict = new Dictionary<string, int>();
            Dictionary<string, int> allVotesDict = new Dictionary<string, int>();
            Array.ForEach(addressDict.Keys.ToArray(),a=>partyVotesDict.Add(a,0));
            Array.ForEach(addressDict.Keys.ToArray(),a=>allVotesDict.Add(a,0));
            int partyIndex = -1;
            int allVotesIndex = -1;
            for (int i = 0; i < electionDataLines.Length; i++)
            {
                string line = electionDataLines[i];
                var tokens = line.Split(',').Select(a=>a.Trim()).ToArray();

                if (line.StartsWith("IL_ID"))
                {
                    // party index  
                    partyIndex = Array.FindIndex(tokens, (a) => a.Equals(partyCodename));

                    // all votes
                     allVotesIndex = Array.FindIndex(tokens, (a) => a.Equals("GECERLI_OY_TOPLAMI"));
                }

                // some parties do not enter ballots at every city
                if (partyIndex == -1) continue;
                
                // create addresskey
                string addressKey = string.Format("{0},{1},{2}", tokens[3], tokens[4], tokens[5]);

                // no address, no go
                if (!addressDict.ContainsKey(addressKey)) continue;

                 partyVotesDict[addressKey] += Convert.ToInt32(tokens[partyIndex]);
                 allVotesDict[addressKey] += Convert.ToInt32(tokens[allVotesIndex]);
            }

            StringBuilder sb = new StringBuilder();
            foreach(string key in partyVotesDict.Keys)
            {
                if (allVotesDict[key] == 0) continue; // there are 0 values in the data !!
                double percent=multiplier*100.0*(partyVotesDict[key]/(double)allVotesDict[key]);
                percent = Math.Min(percent, 100.0); // can go up 100% because of the multiplier

                sb.AppendFormat("[{0},{1},{2:#.##}],", addressDict[key][0], addressDict[key][1], percent);
            }

            StreamWriter jsFile = File.CreateText(outFilename);
            jsFile.Write("values=[");
            jsFile.Write(sb.Remove(sb.Length - 1, 1).ToString());
            jsFile.WriteLine("];");
            jsFile.Close();
        }

        public static void RandomHeatMap(string adressFile,string outputJSFile)
        {
            var lines = File.ReadAllLines(adressFile, Encoding.UTF8);
            Random rnd = new Random(12315);
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < lines.Length; i++)
            {
                var tokens = lines[i].Split(',');
                if (tokens[3].Trim() == "---") continue;

                sb.AppendFormat("[{0},{1},{2}],", tokens[3].Trim(), tokens[4].Trim(), rnd.Next(100));
            }
            StreamWriter jsFile = File.CreateText(outputJSFile);
            jsFile.Write("values=[");
            jsFile.Write(sb.Remove(sb.Length - 1, 1).ToString());
            jsFile.WriteLine("];");
            jsFile.Close();
        }
    }
    
}
