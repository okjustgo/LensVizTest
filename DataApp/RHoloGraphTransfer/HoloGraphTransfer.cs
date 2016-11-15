using AzureLib;
using System.IO;

namespace RHoloGraphTransfer
{
    public class HoloGraphTransfer
    {
        private readonly AzureDataSync _azureConnection;

        public HoloGraphTransfer(string connectionString, string containerName)
        {
            _azureConnection = new AzureDataSync(connectionString, containerName);
        }

        public void UploadCsvAsHgd(string tsvPath, string columnTypes, string geom = null, string xAxis = null, string yAxis = null, string zAxis = null, string color = null)
        {
            var hgd = new HoloGraphData.HoloGraphData();

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tsvPath);
            hgd.ReadDataFromTsv(tsvPath, columnTypes);
            hgd.SetViewProperties(fileNameWithoutExtension, geom, xAxis, yAxis, zAxis, color);

            Stream dataStream = new MemoryStream();
            hgd.ToStream(ref dataStream);
            dataStream.Seek(0, SeekOrigin.Begin);

            var hgdFilename = string.Format("{0}.hgd", fileNameWithoutExtension.Replace(" ", "_"));
            _azureConnection.Upload(hgdFilename, dataStream);
        }
    }
}
