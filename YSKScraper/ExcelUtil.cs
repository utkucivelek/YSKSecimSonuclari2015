using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace YSKScraper
{
    class ExcelUtil
    {
        /// <summary>
        /// processes all xls files in the datadir and combines them
        /// </summary>
        /// <param name="dataDir">data file directory</param>
        /// <param name="outputFilename">output file name (.xlsx)</param>
        public static void MergeFilesYSK(string dataDir,string outputFilename)
        {
            var xlApp1 = new Microsoft.Office.Interop.Excel.Application();
            var xlApp2 = new Microsoft.Office.Interop.Excel.Application();

            if (xlApp1 == null || xlApp2 == null)
            {
                Console.WriteLine("EXCEL could not be started. Check that your office installation and project references are correct.");
                return;
            }

            xlApp1.Visible = true;
            xlApp1.DisplayAlerts = false;

            xlApp2.Visible = true;

            var outWorkbook = xlApp2.Workbooks.Add();
            var outSheet = outWorkbook.ActiveSheet;

            var xlsFiles = Directory.EnumerateFiles(dataDir, "*.xls");
            string lastCityID = string.Empty;
            foreach (string filename in xlsFiles)
            {
                Console.WriteLine(filename);
                var workbook = xlApp1.Workbooks.Open(filename);
                var sheet = workbook.ActiveSheet;
                // city id is always at A3
                string cityID = sheet.Cells[3, 1].Value2;
                if (!string.IsNullOrEmpty(cityID)) // continue if not empty
                {
                    StringBuilder rangeAddress = new StringBuilder();
                    Range r = sheet.UsedRange;//.End[XlDirection.xlToRight];
                    rangeAddress.Append(r.get_Address());

                    if (!cityID.Equals(lastCityID)) // get headers if we switched to a new city
                    {
                        rangeAddress[3] = '2';
                        lastCityID = cityID;
                    }
                    else // get without headers
                    {
                        rangeAddress[3] = '3';
                    }

                    Range content = sheet.Range[rangeAddress.ToString()];
                    content.Copy();

                    var rr = outSheet.Range["A" + (outSheet.Rows.Count).ToString()].End[XlDirection.xlUp];
                    rr = rr.Offset(1, 0);

                    Thread.Sleep(100); // wait for com interop
                    outSheet.Paste(rr);
                }
                workbook.Close(false);

            }

            outWorkbook.SaveAs(outputFilename);
            xlApp1.Quit();
            xlApp2.Quit();
        }
    }
}
