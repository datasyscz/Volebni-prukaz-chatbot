using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VolebniPrukaz.OfficesManager
{
    public class OfficesContext
    {
        /// <summary>
        /// </summary>
        /// <param name="file">Json file</param>
        public OfficesContext(string filePath)
        {
            _file = filePath;
        }

        private string _file { get; }

        public List<Office> GetOffices(int zip)
        {
            using (var file = File.OpenText(_file))
            using (var reader = new JsonTextReader(file))
            {
                var collection = JToken.ReadFrom(reader).ToObject<List<Office>>();
                return collection.Where(a => a.zip == zip.ToString()).ToList();
            }
        }
    }
}