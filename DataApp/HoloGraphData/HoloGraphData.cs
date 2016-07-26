﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MiniJSON;

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
        object headersJson;
        object seriesJson;
        int seriesIdx;
        int xIdx;
        int yIdx;
        int zIdx;
        string[] columnNames;
        float[][] data;

        public HoloGraphData()
        {
            headersJson = new object();
            seriesJson = new object();
        }

        public HoloGraphData(object _headers, float[][] _data)
        {
            headersJson = _headers;
            seriesJson = new object();
            data = _data;
        }

        public HoloGraphData(Stream s)
        {
            var str = HoloGraphData.ReadJsonString(s);
            headersJson = Json.Deserialize(str);
            seriesJson = Json.Deserialize(HoloGraphData.ReadJsonString(s));

            // headersJson = JObject.Parse(HoloGraphData.ReadJsonString(s));
            //seriesJson = JObject.Parse(HoloGraphData.ReadJsonString(s));
            //foreach (var pair in JObject.Parse(HoloGraphData.ReadJsonString(s)))
            //{
            //    seriesDict.Adds
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
            //headersJson = JObject.Parse(json);
            headersJson = Json.Serialize(json);
        }

        public static string GetSeriesDictionaryJson(List<string> seriesList)
        {
            var seriesJson = string.Empty;
            for (int i = 0; i < seriesList.Count; i++)
            {
                seriesJson += string.Format("\"{0}\": \"{1}\",", i, seriesList[i]);
            }
            seriesJson = seriesJson.TrimEnd(',');

            return Json.Serialize(Json.Deserialize("{" + seriesJson + "}")); //RoundTrip the json to ensure proper format. TODO: remove in production
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

        public string HeadersJson
        {
            get
            {
                return Json.Serialize(headersJson);
            }

            set
            {
                headersJson = Json.Deserialize(value);
            }
        }

        public string SeriesJson
        {
            get
            {
                return Json.Serialize(seriesJson);
            }

            set
            {
                seriesJson = Json.Deserialize(value);
            }
        }

        public Stream ToStream(ref Stream s)
        {
            var headerStr = this.HeadersJson;
            WriteJsonString(s, headerStr);

            var seriesStr = this.SeriesJson;
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

        // Expects ReadDataFromCSV to be called already so column names and order already known
        public void CreateAndSetHeader(string title, string geometry)
        {
            var xAxisName = columnNames[xIdx];
            var yAxisName = columnNames[yIdx];
            var zAxisName = columnNames[zIdx];
            var seriesName = seriesIdx == -1 ? "" : columnNames[seriesIdx];
            var headerStr = string.Format(@"{{
                'title': '{0}'
                'type': '{1}',
                'xAxis': '{2}',
                'yAxis': '{3}',
                'zAxis': '{4}',
                'seriesName': '{5}',
                'hasSeries': {6}
            }}", title, geometry, xAxisName, yAxisName, zAxisName, seriesName, seriesIdx == -1 ? "false" : "true");
            headerStr = headerStr.Replace('\'', '\"');
            HeadersJson = headerStr;
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
            columnNames = new string[headers.Length];
            for(int i = 0; i < headers.Length; i++)
            {
                columnNames[i] = headers[i].Trim(new[] { '"' });
            }
            var cols = headers.Length;

            var aes = aesthetics.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            seriesIdx = Array.FindIndex(aes, t => t.IndexOf(seriesStr, StringComparison.OrdinalIgnoreCase) >= 0);
            xIdx = Array.FindIndex(aes, t => t.IndexOf(xStr, StringComparison.OrdinalIgnoreCase) >= 0);
            yIdx = Array.FindIndex(aes, t => t.IndexOf(yStr, StringComparison.OrdinalIgnoreCase) >= 0);
            zIdx = Array.FindIndex(aes, t => t.IndexOf(zStr, StringComparison.OrdinalIgnoreCase) >= 0);

            var xyzIdx = new int[] { xIdx, yIdx, zIdx };

            //foreach (var row in csv.Skip(headerRows)
            //   .TakeWhile(r => r.Length > 1 && r.Last().Trim().Length > 0))
            //{          
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
                        
            this.SeriesJson = HoloGraphData.GetSeriesDictionaryJson(seriesList); //TODO: Inefficent

            data = dataList.ToArray();

        }

    }
}
