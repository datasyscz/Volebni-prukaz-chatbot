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
using VolebniPrukaz.DialogModels;
using VolebniPrukaz.FileModels;

namespace VolebniPrukaz.Controllers
{
    public class FileController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage VoterPass(
            string name, 
            string birthDate, 
            string permanentAddress, 
            string contactAddress, 
            string phone, 
            string officeName, 
            string officeAddress, 
            string officePostalCode, 
            string officeCity, 
            VotePersonType voterPersonType, 
            VoteRoundType voteRoundType
            )
        {
            var stream = new MemoryStream();

            var doc = DocX.Load(System.Web.HttpContext.Current.Server.MapPath("/vzor-zadosti-o-vp-prezident.docx"));
            doc.ReplaceText("%JMENO%", name);
            doc.ReplaceText("%NAROZENI%", birthDate);
            doc.ReplaceText("%TRVALAADRESA%", permanentAddress);
            doc.ReplaceText("%TELEFON%", phone);

            if (!string.IsNullOrEmpty(officeName))
            {
                doc.ReplaceText("%URAD%", officeName);
                doc.ReplaceText("%ADRESA%", officeAddress);
                doc.ReplaceText("%PSC%", officePostalCode);
                doc.ReplaceText("%MESTO%", officeCity);
            }
            else
            {
                doc.ReplaceText("%URAD%", "Městský úřad:");
                doc.ReplaceText("%ADRESA%", string.Empty);
                doc.ReplaceText("%PSC%", string.Empty);
                doc.ReplaceText("%MESTO%", string.Empty);
            }

            if (voterPersonType == VotePersonType.AuthorizedPerson)
                doc.ReplaceText("%VOTERTYPE2%", "x");
            else
                doc.ReplaceText("%VOTERTYPE2%", "");

            if (voterPersonType == VotePersonType.SendHome)
                doc.ReplaceText("%VOTERTYPE3%", "x");
            else
                doc.ReplaceText("%VOTERTYPE3%", "");

            if (voterPersonType == VotePersonType.SendToDifferentAddress)
            {
                doc.ReplaceText("%VOTERTYPE4%", "x");
                doc.ReplaceText("%KONTAKTNIADRESA%", contactAddress);
            } 
            else
            {
                doc.ReplaceText("%VOTERTYPE4%", "");
                doc.ReplaceText("%KONTAKTNIADRESA%", "…………………….…………………….…………………….");
            }

            if (voteRoundType == VoteRoundType.AllRounds)
            {
                doc.ReplaceText("%DATUMVOLBY%", "pro první kolo 12. a 13. ledna 2018 a pro druhé kolo 26. a 27. ledna 2018");
            }
            else if (voteRoundType == VoteRoundType.FirstRound)
            {
                doc.ReplaceText("%DATUMVOLBY%", "pro první kolo 12. a 13. ledna 2018");
            }
            else if (voteRoundType == VoteRoundType.SecondRound)
            {
                doc.ReplaceText("%DATUMVOLBY%", "pro druhé kolo 26. a 27. ledna 2018");
            }

            doc.SaveAs(stream);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(stream.ToArray())
            };
            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "zadost-o-vp-prezident.docx"
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }

        [HttpGet]
        public HttpResponseMessage WarrantPass()
        {
            var stream = new MemoryStream();

            var doc = DocX.Load(System.Web.HttpContext.Current.Server.MapPath("/plna-moc-volba-prezidenta-2018.docx"));
            doc.SaveAs(stream);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(stream.ToArray())
            };
            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "plna-moc-volba-prezidenta-2018.docx"
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}
