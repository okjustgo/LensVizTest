using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureLib;
using System.IO;
using HoloGraph;
using RHoloGraphTransfer;
using System.Configuration;

namespace DataApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var azure = new AzureDataSync("holograph");

            //var hgd = new HoloGraphData();        
            var connection = ConfigurationManager.AppSettings["StorageConnectionString"];
            var ht = new HoloGraphTransfer(connection, "holograph");
            //ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\mtcars.tsv", "mtcars.hgd", "scatter", "x;y;z;series");

            ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\mtcars.tsv", "mtcars.hgd", "Motor Trend Car Road Tests", "scatter", "x;y;z;series");

            ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\diamonds.tsv", "diamonds.hgd", "Price of Diamonds By Quality", "bar", "x;y;z;series");

            ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\volcano.tsv", "volcano.hgd", "Maunga Whau Volcano", "surface", "x;y;z;series");

            ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\iris.tsv", "iris.hgd", "Comparison of Iris Species", "scatter", "x;y;z;series");

            //var headerStr = @"{
            //    'type': 'scatter',
            //    'hasSeries': true,
            //}";
            //hgd.SetHeader(headerStr);
            ////hgd.Data = finalData;


            //Stream dataStream = new MemoryStream();
            //hgd.ToStream(ref dataStream);
            //dataStream.Seek(0, SeekOrigin.Begin);
            ////var sr = new StreamReader(dataStream);

            //azure.Upload("testData.hgd", dataStream);

            Stream outStream = new MemoryStream();
            azure.Download("irisTest_test.hgd", ref outStream);
            outStream.Seek(0, SeekOrigin.Begin);
            var hgd2 = new HoloGraphData(outStream);
            Console.WriteLine(hgd2.HeadersJson);
        }
    }
}
