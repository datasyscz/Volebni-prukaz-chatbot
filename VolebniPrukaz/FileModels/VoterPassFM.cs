using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VolebniPrukaz.FileModels
{
    [Serializable]
    public class VoterPassFM
    {
        [ReplaceTag("%JMENO%")]
        public string Name { get; set; }
        [ReplaceTag("%NAROZENI%")]
        public string BirthDate { get; set; }
        [ReplaceTag("%TRVALAADRESA%")]
        public string PernamentAddress { get; set; }
        [ReplaceTag("%TELEFON%")]
        public string Phone { get; set; }


        [ReplaceTag("%URAD%")]
        public string OfficeName { get; set; }
        [ReplaceTag("%ADRESA%")]
        public string OfficeAddress { get; set; }
        [ReplaceTag("%PSC%")]
        public int OfficePostalCode { get; set; }
        [ReplaceTag("%MESTO%")]
        public string OfficeCity { get; set; }
    }
}