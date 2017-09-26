using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VolebniPrukaz.OfficesManager
{
    [Serializable]
    public class Office
    {
        public string name { get; set; }
        public string zip { get; set; }
        public string city { get; set; }
        public string houseNumber { get; set; }
        public string street { get; set; }
        public string address { get; set; }
        public string datovaSchranka { get; set; }
        public string email { get; set; }
    }

}
