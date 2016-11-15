using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MiniJSON;

namespace HoloGraph
{
    public class HoloGraphData
    {
        // Data properties
        public float[][] Data { get; set; }
        public Dictionary<string, List<string>> Mappings { get; private set; }
        public int NumCols { get; private set; }
        public List<string> ColumnNames { get; private set; }
        public List<string> ColumnTypes { get; private set; }

        // View properties
        public string Title { get; private set; }
        public string Geometry { get; private set; }
        public static List<string> SupportedAesthetics = new List<string> {"x", "y", "z", "color"};
        public Dictionary<string, string> Aesthetics { get; private set; }
        public string Statistic { get; private set; }

        // JSON strings for inspection
        public string ViewJson { get { return PackViewJson(); } }
        public string SchemaJson { get { return PackSchemaJson(); } }
        public string MappingJson { get { return PackMappingJson(); } }


        public HoloGraphData()
        {
            NumCols = 0;
            ColumnNames = new List<string>();
            ColumnTypes = new List<string>();
            Mappings = new Dictionary<string, List<string>>();

            Title = string.Empty;
            Geometry = string.Empty;
            Aesthetics = new Dictionary<string, string>();
            Statistic = string.Empty;
        }

        public HoloGraphData(Stream s)
        {
            var view = ReadJsonString(s);
            var schema = ReadJsonString(s);
            var mapping = ReadJsonString(s);

            UnpackViewJson(view);
            UnpackSchemaJson(schema);
            UnpackMappingJson(mapping);

            var list = new List<float[]>();
            while (s.Position < s.Length)
            {
                // create a second float array and copy the bytes into it...
                var floatRow = new float[NumCols];
                var byteRow = new byte[NumCols * sizeof(float)];
                s.Read(byteRow, 0, NumCols * sizeof(float));
                Buffer.BlockCopy(byteRow, 0, floatRow, 0, byteRow.Length);
                list.Add(floatRow);
            }
            Data = list.ToArray();
        }

        public string PackViewJson()
        {
            var str = string.Format("'title': '{0}',", Title);
            str += string.IsNullOrEmpty(Geometry) ? string.Empty : string.Format("'geom': '{0}',", Geometry);
            str += string.IsNullOrEmpty(Statistic) ? string.Empty : string.Format("'stat': '{0}',", Statistic);
            foreach (var aes in Aesthetics)
            {
                str += string.Format("'{0}': '{1}',", aes.Key, aes.Value);
            }
            str = str.TrimEnd(',');
            str = str.Replace('\'', '\"');
            return Json.Serialize(Json.Deserialize("{" + str + "}"));
        }

        public void UnpackViewJson(string viewJson)
        {
            var v = (Dictionary<string, object>)Json.Deserialize(viewJson);
            Title = v.ContainsKey("title") ? (string)v["title"] : string.Empty;
            Geometry = v.ContainsKey("geom") ? (string)v["geom"] : string.Empty;
            Statistic = v.ContainsKey("stat") ? (string)v["stat"] : string.Empty;

            Aesthetics = new Dictionary<string, string>();
            foreach (var aes in SupportedAesthetics)
            {
                if (v.ContainsKey(aes))
                {
                    Aesthetics[aes] = (string)v[aes];
                }
            }
        }

        public string PackSchemaJson()
        {
            if (NumCols != ColumnNames.Count || NumCols != ColumnTypes.Count)
            {
                throw new ArgumentException("columnNames and columnTypes have different lengths.");
            }

            var numColsJson = string.Format("\"numCols\":{0}", NumCols);
            var colNamesJson = string.Empty;
            var colTypesJson = string.Empty;
            for (var i = 0; i < NumCols; i++)
            {
                colNamesJson += string.Format("\"{0}\",", ColumnNames[i]);
                colTypesJson += string.Format("\"{0}\",", ColumnTypes[i]);
            }
            colNamesJson = colNamesJson.TrimEnd(',');
            colTypesJson = colTypesJson.TrimEnd(',');

            return Json.Serialize(Json.Deserialize(string.Format("{{{0}, \"colNames\":[{1}], \"colTypes\":[{2}]}}", numColsJson, colNamesJson, colTypesJson))); //RoundTrip the json to ensure proper format. TODO: remove in production
        }

        public void UnpackSchemaJson(string schemaJson)
        {
            var schema = (Dictionary<string, object>)Json.Deserialize(schemaJson);
            NumCols = int.Parse(schema["numCols"].ToString());
            var names = (List<object>)schema["colNames"];
            var types = (List<object>)schema["colTypes"];
            if (NumCols != names.Count || NumCols != types.Count)
            {
                throw new ArgumentException(string.Format("Stream indicated {0} columns, but names or types had incorrect number of columns.", NumCols));
            }

            ColumnNames = new List<string>();
            ColumnTypes = new List<string>();
            for (var i = 0; i < NumCols; i++)
            {
                ColumnNames.Add(names[i].ToString());
                ColumnTypes.Add(types[i].ToString());
            }
        }

        public string PackMappingJson()
        {
            var mappingJson = string.Empty;
            foreach (var series in Mappings)
            {
                var name = series.Key;
                var map = series.Value;

                var seriesJson = string.Empty;
                for (var i = 0; i < map.Count; i++)
                {
                    seriesJson += string.Format("\"{0}\",", map[i]);
                }
                seriesJson = seriesJson.TrimEnd(',');

                mappingJson += string.Format("\"{0}\":[{1}],", name, seriesJson);
            }
            mappingJson = mappingJson.TrimEnd(',');
            return Json.Serialize(Json.Deserialize(string.Format("{{{0}}}", mappingJson))); //RoundTrip the json to ensure proper format. TODO: remove in production
        }

        public void UnpackMappingJson(string mappingJson)
        {
            var map = (Dictionary<string, object>)Json.Deserialize(mappingJson);
            Mappings = new Dictionary<string, List<string>>();
            foreach (var m in map)
            {
                Mappings[m.Key] = new List<string>();
                var items = (List<object>)m.Value;
                foreach (var item in items)
                {
                    Mappings[m.Key].Add(item.ToString());
                }
            }
        }


        public Stream ToStream(ref Stream s)
        {
            WriteJsonString(s, ViewJson);
            WriteJsonString(s, SchemaJson);
            WriteJsonString(s, MappingJson);

            foreach (var row in Data)
            {
                var byteArray = new byte[row.Length * sizeof(float)];
                Buffer.BlockCopy(row, 0, byteArray, 0, byteArray.Length);
                s.Write(byteArray, 0, byteArray.Length);
            }
            s.Flush();

            return s;
        }

        private static void WriteJsonString(Stream s, string jsonString)
        {
            var byteRepresentation = Encoding.ASCII.GetBytes(jsonString);

            var numBytes = byteRepresentation.Length;
            var countBytes = BitConverter.GetBytes(numBytes);
            s.Write(countBytes, 0, countBytes.Length);

            s.Write(byteRepresentation, 0, numBytes);
        }

        public static string ReadJsonString(Stream s)
        {
            var countByte = new byte[sizeof(int)];
            s.Read(countByte, 0, countByte.Length);
            var jsonStringLength = BitConverter.ToInt32(countByte, 0);

            var jsonBytes = new byte[jsonStringLength];
            s.Read(jsonBytes, 0, jsonBytes.Length);
            return Encoding.ASCII.GetString(jsonBytes);
        }

        // Expects ReadDataFromCSV to be called already so column names and order already known
        public void SetViewProperties(string title, string geom = null, string xAxis = null, string yAxis = null, string zAxis = null, string color = null, string stat = null)
        {
            Title = title;
            Geometry = geom;
            Statistic = stat;
            if (xAxis != null)
            {
                Aesthetics["x"] = xAxis;
            }
            if (yAxis != null)
            {
                Aesthetics["y"] = yAxis;
            }
            if (zAxis != null)
            {
                Aesthetics["z"] = zAxis;
            }
            if (color != null)
            {
                Aesthetics["color"] = color;
            }
        }

        public void ReadDataFromTsv(string fileName, string columnTypes)
        {
            var dataList = new List<float[]>();
            var reader = new StreamReader(File.OpenRead(fileName));

            var readLine = reader.ReadLine();
            if (readLine != null)
            {
                Mappings = new Dictionary<string, List<string>>();
                var headers = readLine.Trim().Split('\t');
                var columnNames = new string[headers.Length];
                for (var i = 0; i < headers.Length; i++)
                {
                    columnNames[i] = headers[i].Trim('"');
                }
                ColumnNames = new List<string>(columnNames);
                NumCols = headers.Length;

                var s = columnTypes.Split(';');
                if (s.Length != NumCols)
                {
                    throw new ArgumentException("columnTypes did not have the same number of types as there are columns in the data.");
                }
                var types = new string[NumCols];
                for (var i = 0; i < NumCols; i++)
                {
                    switch (s[i])
                    {
                        case "int":
                        case "float":
                            types[i] = s[i];
                            break;
                        case "string":
                            types[i] = s[i];
                            Mappings[ColumnNames[i]] = new List<string>();
                            break;
                        default:
                            throw new ArgumentException(string.Format("Column type '{0}' unsupported.", s[i]));
                    }
                }
                ColumnTypes = new List<string>(types);

                while (!reader.EndOfStream)
                {
                    readLine = reader.ReadLine();
                    if (readLine == null)
                    {
                        throw new Exception("End of stream not found, but read a null line...");
                    }
                    var row = readLine.Trim().Split('\t');
                    if (row.Length != NumCols)
                    {
                        throw new ArgumentException("Data does not have enough columns.");
                    }

                    var dataRow = new float[NumCols];
                    for (var i = 0; i < NumCols; i++)
                    {
                        var type = ColumnTypes[i];
                        if (type == "string")
                        {
                            var colName = ColumnNames[i];
                            var value = row[i].Trim('"').Replace("\"", "_");
                            if (!Mappings[colName].Contains(value))
                            {
                                Mappings[colName].Add(value);
                            }

                            dataRow[i] = Mappings[colName].IndexOf(value);
                        }
                        else
                        {
                            dataRow[i] = float.Parse(row[i]);
                        }
                    }

                    dataList.Add(dataRow);
                }
            }

            Data = dataList.ToArray();
        }

        public int GetColumnIndex(string columnName)
        {
            return ColumnNames.IndexOf(columnName);
        }

        public float[] GetData(string aestheticName)
        {
            var idx = GetColumnIndex(Aesthetics[aestheticName]);
            var d = new float[Data.Length];
            for (var i = 0; i < Data.Length; i++)
            {
                d[i] = Data[i][idx];
            }
            return d;
        }
    }
}
