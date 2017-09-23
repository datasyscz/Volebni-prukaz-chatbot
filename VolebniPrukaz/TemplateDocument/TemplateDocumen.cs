using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Novacode;

namespace VolebniPrukaz
{
    /// <summary>
    /// Replace all tags in document
    /// </summary>
    public class TemplateDocument
    {
        private FileInfo _templateDocument;
        public TemplateDocument(FileInfo templateDocument) => _templateDocument = templateDocument;

        /// <summary>
        /// Replace all tags in docx, by class where properties are with attribute [ReplaceTag]. 
        /// </summary>
        /// <param name="sourceData"></param>
        /// <returns></returns>
        public async Task<DocX> ReplaceAllTagsAsync<T>(T sourceData) where T : class
        {
            //Load file
            DocX document = await LoadFile();

            //get all properties from object
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                ReplaceTag attribute = prop.GetCustomAttribute(typeof(ReplaceTag), true) as ReplaceTag;
                if (attribute != null)
                {
                    //Get key and value from property to replace
                    string value = prop.GetValue(sourceData).ToString();
                    string key = attribute.Key;

                    //Replace tag in document by property with atribute
                    document.ReplaceText(key, value);
                }
            }

            return document;
        }

        /// <summary>
        /// Load document async from file
        /// </summary>
        /// <returns></returns>
        private async Task<DocX> LoadFile()
        {
            return await Task.Run(() => DocX.Load(_templateDocument.FullName)); ;
        }
    }
}