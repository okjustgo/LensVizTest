﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureLib;
using System.IO;
using HoloGraph;
using RHoloGraphTransfer;

namespace DataApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //var azure = new AzureDataSync("holograph");

            //var hgd = new HoloGraphData();

            var ht = new HoloGraphTransfer("DefaultEndpointsProtocol=http;AccountName=deftgeneralstorage;AccountKey=Wouldn'tYouLikeToKnow", "holograph");
            ht.UploadCsvAsHgd(@"C:\Users\kerussel\Desktop\mtcars.tsv", "mtcars.hgd", "scatter", "x;y;z;series");
                        
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

            //Stream outStream = new MemoryStream();
            //azure.Download("testData.hgd", ref outStream);
            //outStream.Seek(0, SeekOrigin.Begin);
            //var hgd2 = new HoloGraphData(outStream);

        }
    }
}
