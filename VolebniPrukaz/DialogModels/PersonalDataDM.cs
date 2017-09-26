using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace VolebniPrukaz.DialogModels
{
    [Serializable]
    public class PersonalDataDM
    {
        public string Name { get; set; }
        public string BirthDate { get; set; }
        public string Phone { get; set; }

        public DateTime? BirthDateConverted
        {
            get
            {
                var ci = new CultureInfo("cs-CZ");
                DateTime.TryParse(this.BirthDate, ci, DateTimeStyles.AllowWhiteSpaces, out DateTime dt);
                return dt;
            }
        }
    }
}