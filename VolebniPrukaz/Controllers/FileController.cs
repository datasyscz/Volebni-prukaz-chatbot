using Novacode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using VolebniPrukaz.FileModels;

namespace VolebniPrukaz.Controllers
{
    public class FileController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage VoterPass(string name, string birthDate, string permanentAddress, string phone, string officeName, string officeAddress, string officePostalCode, string officeCity)
        {
            var stream = new MemoryStream();

            var doc = DocX.Load("./vzor-zadosti-o-vp.docx");
            doc.ReplaceText("%JMENO%", name);
            doc.ReplaceText("%NAROZENI%", birthDate);
            doc.ReplaceText("%TRVALAADRESA%", permanentAddress);
            doc.ReplaceText("%TELEFON%", phone);
            doc.ReplaceText("%URAD%", officeName);
            doc.ReplaceText("%ADRESA%", officeAddress);
            doc.ReplaceText("%PSC%", officePostalCode);
            doc.ReplaceText("%MESTO%", officeCity);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(stream.ToArray())
            };
            result.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "vzor-zadosti-o-vp.docx"
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}
