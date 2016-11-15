using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MiniJSON;

namespace HoloGraphData
{
    public class HoloGraphData
    {
        private object _viewJson;
        private object _schemaJson;
        private object _mappingJson;
        private int _numCols;
        private List<string> _columnNames;
        private List<string> _columnTypes;

        public HoloGraphData()
        {
            _viewJson = new object();
            _schemaJson = new object();
            _mappingJson = new object();
        }

        public HoloGraphData(object view, object schema, object mapping, float[][] data)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            _viewJson = view;
            _schemaJson = schema;
            _mappingJson = mapping;
            Data = data;
        }

        public HoloGraphData(Stream s)
        {
            _viewJson = Json.Deserialize(ReadJsonString(s));
            _schemaJson = Json.Deserialize(ReadJsonString(s));
            _mappingJson = Json.Deserialize(ReadJsonString(s));

            var h = (Dictionary<string, object>)_schemaJson;
            _numCols = int.Parse(h["numCols"].ToString());
            var names = (List<object>)h["colNames"];
            var types = (List<object>)h["colTypes"];
            if (_numCols != names.Count || _numCols != types.Count)
            {
                throw new ArgumentException(string.Format("Stream indicated {0} columns, but names or types had incorrect number of columns.", _numCols));
            }
            _columnNames = names.ConvertAll(obj => obj.ToString());
            _columnTypes = types.ConvertAll(obj => obj.ToString());
            
            var list = new List<float[]>();
            while (s.Position < s.Length)
            {
                // create a second float array and copy the bytes into it...
                var floatRow = new float[_numCols];
                var byteRow = new byte[_numCols * 4];
                s.Read(byteRow, 0, _numCols * 4);
                Buffer.BlockCopy(byteRow, 0, floatRow, 0, byteRow.Length);
                list.Add(floatRow);
            }
            Data = list.ToArray();
        }

        public string GetSchemaDictionaryJson(int numCols, List<string> columnNames, List<string> columnTypes)
        {
            if (numCols != columnNames.Count || numCols != columnTypes.Count)
            {
                throw new ArgumentException("columnNames and columnTypes have different lengths.");
            }

            var numColsJson = string.Format("\"numCols\":{0}", _numCols);
            var colNamesJson = string.Empty;
            var colTypesJson = string.Empty;
            for (var i = 0; i < numCols; i++)
            {
                colNamesJson += string.Format("\"{0}\",", columnNames[i]);
                colTypesJson += string.Format("\"{0}\",", columnTypes[i]);
            }
            colNamesJson = colNamesJson.TrimEnd(',');
            colTypesJson = colTypesJson.TrimEnd(',');
            
            return Json.Serialize(Json.Deserialize(string.Format("{{{0}, \"colNames\":[{1}], \"colTypes\":[{2}]}}", numColsJson, colNamesJson, colTypesJson))); //RoundTrip the json to ensure proper format. TODO: remove in production
        }

        public static string GetMappingDictionaryJson(Dictionary<string, List<string>> mappings)
        {
            var mappingJson = string.Empty;
            foreach(var series in mappings)
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

        public float[][] Data { get; set; }

        public string ViewJson
        {
            get { return Json.Serialize(_viewJson); }
            set { _viewJson = Json.Deserialize(value); }
        }

        public string SchemaJson
        {
            get { return Json.Serialize(_schemaJson); }
            set { _schemaJson = Json.Deserialize(value); }
        }

        public string MappingJson
        {
            get { return Json.Serialize(_mappingJson); }
            set { _mappingJson = Json.Deserialize(value); }
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

        // Expects ReadDataFromTsv to be called already so column names and order already known
        public void CreateAndSetViewHeader(string title, string geom = null, string xAxis = null, string yAxis = null, string zAxis = null, string color = null)
        {
            var str = string.Format(@"{{
                'title': '{0}',
                'geom': '{1}',
                'x': '{2}',
                'y': '{3}',
                'z': '{4}',
                'color': '{5}'
            }}", title, geom, xAxis, yAxis, zAxis, color);
            str = str.Replace('\'', '\"');
            ViewJson = str;
        }

        public void ReadDataFromTsv(string fileName, string columnTypes)
        {
            var dataList = new List<float[]>();
            var reader = new StreamReader(File.OpenRead(fileName));

            var readLine = reader.ReadLine();
            if (readLine != null)
            {
                var mappings = new Dictionary<string, List<string>>();
                var headers = readLine.Trim().Split('\t');
                var columnNames = new string[headers.Length];
                for(var i = 0; i < headers.Length; i++)
                {
                    columnNames[i] = headers[i].Trim('"');
                }
                _columnNames = new List<string>(columnNames);
                _numCols = headers.Length;

                var s = columnTypes.Split(';');
                if (s.Length != _numCols)
                {
                    throw new ArgumentException("columnTypes did not have the same number of types as there are columns in the data.");
                }
                var types = new string[_numCols];
                for (var i = 0; i < _numCols; i++)
                {
                    switch (s[i])
                    {
                        case "int":
                        case "float":
                            types[i] = s[i];
                            break;
                        case "string":
                            types[i] = s[i];
                            mappings[_columnNames[i]] = new List<string>();
                            break;
                        default:
                            throw new ArgumentException(string.Format("Column type '{0}' unsupported.", s[i]));
                    }
                }
                _columnTypes = new List<string>(types);
                
                while (!reader.EndOfStream)
                {
                    readLine = reader.ReadLine();
                    if (readLine == null)
                    {
                        throw new Exception("End of stream not found, but read a null line...");
                    }
                    var row = readLine.Trim().Split('\t');
                    if (row.Length != _numCols)
                    {
                        throw new ArgumentException("Data does not have enough columns.");
                    }

                    var dataRow = new float[_numCols];
                    for (var i = 0; i < _numCols; i++)
                    {
                        var type = _columnTypes[i];
                        if (type == "string")
                        {
                            var colName = _columnNames[i];
                            var value = row[i].Trim('"');
                            if (!mappings[colName].Contains(value))
                            {
                                mappings[colName].Add(value);
                            }

                            dataRow[i] = mappings[colName].IndexOf(value);
                        }
                        else
                        {
                            dataRow[i] = float.Parse(row[i]);
                        }
                    }

                    dataList.Add(dataRow);
                }
                       
                MappingJson = GetMappingDictionaryJson(mappings); //TODO: Inefficent
                SchemaJson = GetSchemaDictionaryJson(_numCols, _columnNames, _columnTypes);
            }

            Data = dataList.ToArray();
        }

    }
}
