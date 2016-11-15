using System;
using AzureLib;
using System.IO;
using RHoloGraphTransfer;
using System.Configuration;

namespace DataApp
{
    class Program
    {
        static void Main()
        {
            var azure = new AzureDataSync("holograph");

            //var hgd = new HoloGraphData();        
            var connection = ConfigurationManager.AppSettings["StorageConnectionString"];
            var ht = new HoloGraphTransfer(connection, "holograph");
            
            ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\Fuel Economy.tsv", "string;string;float;float;float;string;string;float;float;string", "point", "HighwayMpg", "DisplacementInLiters", "Year", "NumberOfCylinders");
            CheckFile("Fuel_Economy.hgd");

            ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\Price of Diamonds by Quality.tsv", "float;float;string;string;string;float;float;float;float;float", "point", "Carat", "Clarity", "Price", "Quality");
            CheckFile("Price_of_Diamonds_by_Quality.hgd");

            //ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\volcano.tsv", "volcano.hgd", "Maunga Whau Volcano", "surface", "x;y;z;series");

            //ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\iris.tsv", "iris.hgd", "Comparison of Iris Species", "scatter", "x;y;z;series");
        }

        public static void CheckFile(string hgdFileName)
        {
            var azure = new AzureDataSync("holograph");
            Stream outStream = new MemoryStream();
            azure.Download(hgdFileName, ref outStream);
            outStream.Seek(0, SeekOrigin.Begin);
            var hgd2 = new HoloGraphData.HoloGraphData(outStream);
            Console.WriteLine(hgd2.ViewJson);
            Console.WriteLine(hgd2.SchemaJson);
            Console.WriteLine(hgd2.MappingJson);
        }
    }
}
