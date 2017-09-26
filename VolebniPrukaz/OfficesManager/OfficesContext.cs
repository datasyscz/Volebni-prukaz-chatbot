using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VolebniPrukaz;

namespace VolebniPrukaz.OfficesManager
{
    public class OfficesContext
    {
        private string _file { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">Json file</param>
        public OfficesContext(string filePath)
        {
            _file = filePath;
        }

        public List<Office> GetOffices(int zip)
        {
            using (StreamReader file = File.OpenText(_file))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                var obj = JToken.ReadFrom(reader);
                var deserialize = obj.ToObject<List<Office>>();
                return deserialize.Where(a=> a.zip == zip.ToString()).ToList();
            }
        }
    }
}