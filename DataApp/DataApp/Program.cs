using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureLib;
using System.IO;

namespace DataApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var azure = new AzureDataSync("holograph");

            var contents = File.ReadAllText("irisData.csv").Split('\n');
            var csv = from line in contents
                      select line.Split(',').ToArray();

            int headerRows = 5;
            foreach (var row in csv.Skip(headerRows)
                .TakeWhile(r => r.Length > 1 && r.Last().Trim().Length > 0))
            {
                String zerothColumnValue = row[0]; // leftmost column
                var firstColumnValue = row[1];
            }

            azure.Upload("testData.hgd", foo);

            Stream foo;
            azure.Download("testData.hgd", out foo);

            
        }
    }
}
