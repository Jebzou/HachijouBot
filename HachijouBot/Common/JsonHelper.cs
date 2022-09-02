﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace HachijouBot.Common
{
    /// <summary>
    /// Helper to write and read json stuff
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Write object to a json file
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_data"></param>
        public static void WriteJson(string _path, object _data)
        {
            using (var _fileStream = File.Create(_path))
            using (var _streamWriter = new StreamWriter(_fileStream))
            using (var _jsonTextWriter = new JsonTextWriter(_streamWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t'
            })
            {
                JsonSerializer _jsonSerializer = JsonSerializer.CreateDefault();
                _jsonSerializer.Serialize(_jsonTextWriter, _data);
            }

        } 
        
        /// <summary>
        /// Write object to a json file
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_data"></param>
        public static void WriteJsonByOnlyIndentingOnce(string _path, object _data)
        {
            WriteJsonByOnlyIndentingXTimes(_path, _data, 1);
        }

        /// <summary>
        /// Write object to a json file
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_data"></param>
        public static void WriteJsonByOnlyIndentingXTimes(string _path, object _data, int _indenting)
        {
            WriteJsonByOnlyIndentingXTimes(_path, _data, _indenting, false);
        }

        /// <summary>
        /// Write object to a json file
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_data"></param>
        public static void WriteJsonByOnlyIndentingXTimes(string _path, object _data, int _indenting, bool _ignoreNull)
        {

            using (var _fileStream = File.Create(_path))
            using (var _streamWriter = new StreamWriter(_fileStream))
            using (var _jsonTextWriter = new CustomIndentingJsonTextWriter(_streamWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t',
                MaxIndentDepth = _indenting,
            })
            {
                JsonSerializer _jsonSerializer = JsonSerializer.CreateDefault();
                if (_ignoreNull) _jsonSerializer.NullValueHandling = NullValueHandling.Ignore;
                _jsonSerializer.Serialize(_jsonTextWriter, _data);
            }

        }

        /// <summary>
        /// Read json file and deserialize it
        /// </summary>
        /// <param name="_path"></param>
        public static JObject ReadJsonObject(string _path)
        {
            return ReadJson(_path) as JObject;
        }

        /// <summary>
        /// Read json file and deserialize it
        /// </summary>
        /// <param name="_path"></param>
        private static JToken ReadJson(string _path)
        {
            try
            {
                string _text = File.ReadAllText(_path);
                return ReadJsonFromString(_text);
            }
            catch (Exception _ex) when (_ex is FileNotFoundException || _ex is DirectoryNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Read json file and deserialize it
        /// </summary>
        /// <param name="_path"></param>
        public static JToken ReadJsonFromString(string _text)
        {
            return (JToken)JsonConvert.DeserializeObject(_text);
        }

        /// <summary>
        /// Read json file and deserialize it
        /// </summary>
        /// <param name="_path"></param>
        public static JArray ReadJsonArray(string _path)
        {
            return ReadJson(_path) as JArray;
        }

        /// <summary>
        /// Read a Kancolle json file and deserialize it<br></br>
        /// Separated method cause need to remove the svdata=
        /// </summary>
        /// <param name="_path"></param>
        public static JObject ReadKCJson(string _path)
        {
            try
            {
                string _text = File.ReadAllText(_path);

                // --- revome svdata=
                _text = _text[7..];

                return (JObject)JsonConvert.DeserializeObject(_text);
            }
            catch (Exception _ex) when (_ex is FileNotFoundException || _ex is DirectoryNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Read json from url
        /// </summary>
        /// <param name="_url"></param>
        /// <returns></returns>
        public static JToken ReadJsonFromUrl(string _url)
        {
            try
            {
                using var _webClient = new WebClient();

                string _rawJson = _webClient.DownloadString(_url);

                return ReadJsonFromString(_rawJson);
            }
            catch (Exception _ex)
            {
                Exception _exeption = new Exception("Error reading Edge.json", _ex);

                throw _exeption;
            }
        }

        /// <summary>
        /// Read json file and deserialize it
        /// </summary>
        /// <param name="_path"></param>
        public static T ReadJson<T>(string _path)
        {
            using StreamReader stream = new StreamReader(_path);
            using JsonReader _reader = new JsonTextReader(stream);

            return new JsonSerializer().Deserialize<T>(_reader);
        }
    }
}
