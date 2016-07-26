using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzureLib;
using HoloGraph;
using System.IO;

namespace RHoloGraphTransfer
{
    public class HoloGraphTransfer
    {
        private AzureDataSync _azureConnection;

        public HoloGraphTransfer(string connectionString, string containerName)
        {
            _azureConnection = new AzureDataSync(connectionString, containerName);
        }

        public void UploadCsvAsHgd(string csvPath, string hgdFilename, string geom, string aes)
        {
            var hgd = new HoloGraphData();

            hgd.ReadDataFromCSV(csvPath, aes);

            var headerStr = string.Format(@"{{
                'type': '{0}',
                'hasSeries': true
            }}", geom);
            headerStr = headerStr.Replace('\'','\"');
            hgd.HeadersJson = headerStr;

            Stream dataStream = new MemoryStream();
            hgd.ToStream(ref dataStream);
            dataStream.Seek(0, SeekOrigin.Begin);

            _azureConnection.Upload(hgdFilename, dataStream);
        }
    }
}
