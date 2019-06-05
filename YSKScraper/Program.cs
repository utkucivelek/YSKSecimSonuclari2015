using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSKScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            // 0. The following steps should not be executed all at once. 
            // Comment/uncomment as needed.

            // 1. scrape ysk site for 2015 results : this takes > 3 hours and can fail at any point
           // YSKScraper.Scrape2015();

            // 2. combine downloaded files
           // ExcelUtil.MergeFilesYSK(@"D:\Downloads\YSK2015", @"D:\Work\YSK\2015Combined.xlsx");

            // 3. open the combined xlsx file and extract il,ilce,mahalle/koy columns into a new csv file
            // save it as mahalleler2015.csv for next step
            // save the whole file as yskCombined2015.csv for step 5

            // 4. Geocode the addresses : this takes > 6 hours and can fail 
            //YandexGeocoder.GeocodeAndCombine(@"D:\Work\YSK\mahalleler2015.csv", @"D:\Work\YSK\geocodedAddresses2015.csv");

            // 5. Process the combined xlsx file together with geocoded adress file to generate .js files for leaflet.js.
            YSKHeatMapGenerator mapgen = new YSKHeatMapGenerator();
            mapgen.Init(@"Data\geocodedAddresses2015.csv", @"Data\yskCombined2015.csv");
            Console.WriteLine("Generating .js files for heatmap...");
            mapgen.Generate("AK PARTİ", @"HeatMap\js\votesAKP.js");
            mapgen.Generate("CHP", @"HeatMap\js\votesCHP.js");
            mapgen.Generate("MHP", @"HeatMap\js\votesMHP.js");
            mapgen.Generate("HDP", @"HeatMap\js\votesHDP.js");
            mapgen.Generate("VATAN PARTİSİ", @"HeatMap\js\votesVATANx100.js", 100.0);
            mapgen.Generate("SAADET", @"HeatMap\js\votesSAADETx10.js", 10.0);
            mapgen.Generate("LDP", @"HeatMap\js\votesLDPx100.js", 100.0);
            Console.WriteLine("done.");
        }
    }
}
