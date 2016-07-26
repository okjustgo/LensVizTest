using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;

namespace HoloGraph
{
    public enum HoloGraphType
    {
        scatter,
        bar,
        line
    }
    public class HoloGraphData
    {
        JObject headersJson;
        JObject seriesJson;
        float[][] data;        
        
        public HoloGraphData()
        {
            headersJson = new JObject();            
        }

        public HoloGraphData(JObject _headers, float[][] _data)
        {
            headersJson = _headers;
            data = _data;
        }

        public HoloGraphData(Stream s)
        {
            headersJson = JObject.Parse(HoloGraphData.ReadJsonString(s));
            seriesJson = JObject.Parse(HoloGraphData.ReadJsonString(s));
            //foreach (var pair in JObject.Parse(HoloGraphData.ReadJsonString(s)))
            //{
            //    seriesDict.Add
            //    seriesDict[Int32.Parse(pair.Key)] = pair.Value.ToString();
            //}

            List<float[]> list = new List<float[]>();
            int colCount = 4;
            //int byteCount = (int)(dataStream.Length - dataStream.Position);
            while (s.Position < s.Length)
            {
                // create a second float array and copy the bytes into it...
                var floatRow = new float[colCount];
                byte[] byteRow = new byte[colCount * 4];
                s.Read(byteRow, 0, colCount * 4);
                Buffer.BlockCopy(byteRow, 0, floatRow, 0, byteRow.Length);
                list.Add(floatRow);
            }
            this.data = list.ToArray();
        }

        public void SetHeader(string json)
        {
            headersJson = JObject.Parse(json);
        }

        public static string GetSeriesDictoinaryJson(List<string> seriesList)
        {
            var seriesJson = string.Empty;
            for (int i = 0; i < seriesList.Count; i++)
            {
                seriesJson += string.Format("'{0}': '{1}',", i, seriesList[i]);
            }
            return JObject.Parse("{" + seriesJson + "}").ToString(); //RoundTrip the json to clean it up.
        }

        public float[][] Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }

        public Stream ToStream(ref Stream s)
        {
            var headerStr = this.headersJson.ToString();
            WriteJsonString(s, headerStr);

            var seriesStr = this.seriesJson.ToString();
            WriteJsonString(s, seriesStr);

            foreach (var row in data)
            {
                var byteArray = new byte[row.Length * 4];
                Buffer.BlockCopy(row, 0, byteArray, 0, byteArray.Length);
                s.Write(byteArray, 0, byteArray.Length);
            }
            s.Flush();

            return s;
        }

        private void WriteJsonString(Stream s, string jsonString)
        {
            
            byte[] toBytes = Encoding.ASCII.GetBytes(jsonString);

            var byteCount = toBytes.Length;
            var countBytes = BitConverter.GetBytes(byteCount);
            s.Write(countBytes, 0, countBytes.Length);

            s.Write(toBytes, 0, byteCount);
        }

        public static string ReadJsonString(Stream s)
        {
            byte[] countByte = new byte[sizeof(int)];
            s.Read(countByte, 0, countByte.Length);
            int count = BitConverter.ToInt32(countByte, 0);

            byte[] jsonBytes = new byte[count];
            s.Read(jsonBytes, 0, jsonBytes.Length);
            return Encoding.ASCII.GetString(jsonBytes);
        }

        public void ReadDataFromCSV(string fileName, string aesthetics)
        {
            List<float[]> dataList = new List<float[]>();
            var reader = new StreamReader(File.OpenRead(fileName));            

            var seriesStr = "series";
            var xStr = "x";
            var yStr = "y"; 
            var zStr = "z";
           
            var headers = reader.ReadLine().Trim().Split('\t');
            var cols = headers.Length;

            var aes = aesthetics.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var seriesIdx = Array.FindIndex(aes, t => t.IndexOf(seriesStr, StringComparison.InvariantCultureIgnoreCase) >= 0);
            var xIdx = Array.FindIndex(aes, t => t.IndexOf(xStr, StringComparison.InvariantCultureIgnoreCase) >= 0);
            var yIdx = Array.FindIndex(aes, t => t.IndexOf(yStr, StringComparison.InvariantCultureIgnoreCase) >= 0);
            var zIdx = Array.FindIndex(aes, t => t.IndexOf(zStr, StringComparison.InvariantCultureIgnoreCase) >= 0);

            var xyzIdx = new int[] { xIdx, yIdx, zIdx };

            //foreach (var row in csv.Skip(headerRows)
            //   .TakeWhile(r => r.Length > 1 && r.Last().Trim().Length > 0))
            //{
            JObject seriesJ = new JObject();
            List<string> seriesList = new List<string>();
            while (!reader.EndOfStream)
            {
                var row = reader.ReadLine().Trim().Split('\t');
                var dataRow = new float[cols];
                var dataIdx = 0;

                if (seriesIdx != -1)
                {
                    var item = row[seriesIdx];
                    if (!seriesList.Contains(item))
                    {
                        seriesList.Add(item);
                    }

                    dataRow[dataIdx] = seriesList.IndexOf(item);
                    dataIdx++;
                }

                foreach (var idx in xyzIdx)
                {
                    dataRow[dataIdx] = float.Parse(row[idx]);
                    dataIdx++;
                }
                dataList.Add(dataRow);
            }

            this.seriesJson = JObject.Parse(HoloGraphData.GetSeriesDictoinaryJson(seriesList)); //Inefficent

            data = dataList.ToArray();

        }

    }
}
