using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace VolebniPrukaz.API.Facebook
{
    public class FacebookClient
    {
        public static string Token = WebConfigurationManager.AppSettings["FacebookAccesToken"];

        public void SendTyping(string userId)
        {

            try
            {
                
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(
                    $"https://graph.facebook.com/v2.6/me/messages?access_token=" +
                    $"{Token}");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                var content = new
                {
                    recipient = new { id = userId },
                    sender_action = "typing_on"
                };
                var json = JsonConvert.SerializeObject(content);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch
            {

            }
        }
    }
}