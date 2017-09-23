using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Novacode;

namespace VolebniPrukaz
{
    /// <summary>
    ///     Replace all tags in document
    /// </summary>
    public class TemplateDocument
    {
        private readonly FileInfo _templateDocument;

        public TemplateDocument(FileInfo templateDocument)
        {
            _templateDocument = templateDocument;
        }

        /// <summary>
        ///     Replace all tags in docx, by class where properties are with attribute [ReplaceTag].
        /// </summary>
        /// <param name="sourceData"></param>
        /// <returns></returns>
        public async Task<DocX> ReplaceAllTagsAsync<T>(T sourceData) where T : class
        {
            //Load file
            var document = await LoadFile();

            //get all properties from object
            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                var attribute = prop.GetCustomAttribute(typeof(ReplaceTag), true) as ReplaceTag;
                if (attribute != null)
                {
                    //Get key and value from property to replace
                    var value = prop.GetValue(sourceData).ToString();
                    var key = attribute.Key;

                    //Replace tag in document by property with atribute
                    document.ReplaceText(key, value);
                }
            }

            return document;
        }

        /// <summary>
        ///     Load document async from file
        /// </summary>
        /// <returns></returns>
        private async Task<DocX> LoadFile()
        {
            return await Task.Run(() => DocX.Load(_templateDocument.FullName));
            ;
        }
    }
}