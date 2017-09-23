using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VolebniPrukaz
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ReplaceTag : System.Attribute
    {
        /// <summary>
        /// Tag name
        /// <example>[ReplaceTag("%NAME%")</example>
        /// </summary>
        public string Key;

        /// <param name="key">Target name to replace</param>
        public ReplaceTag(string key) => this.Key = key;
    }
}