using Newtonsoft.Json;
using RestSharp;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Google.Maps
{
    [Serializable]
    public class GoogleMapsClient
    {
        protected readonly string _apiKey;

        public GoogleMapsClient(string apiKey = null)
        {
            if (apiKey == null)
                _apiKey = WebConfigurationManager.AppSettings["GooleApiKey"];
            else
                _apiKey = apiKey;
        }

        public class GoogleApiResult
        {
            public object Data;
            public bool CorrectResponse;
        }

        private readonly string StaticMapBaseUrl = "https://maps.googleapis.com/maps/api/staticmap?";
        private readonly Uri GeocodeBaseUri = new Uri("https://maps.googleapis.com/maps/api");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">Longtitude, latitude or text adress.</param>
        /// <returns></returns>
        public async Task<GoogleApiResult> GetMapImageData(string address, int width = 600, int height = 300, int zoom = 16)
        {
            try
            {
                address = HttpUtility.UrlEncode(address);

                string query =
                    $"center={address}&zoom={zoom}&size={width}x{height}&maptype=roadmap&markers=color:red%7Clabel:C%7C{address}&key={_apiKey}";
                var webClient = new WebClient();
                var result = new GoogleApiResult();

                var imageBytes = await webClient.DownloadDataTaskAsync(StaticMapBaseUrl + query);
                WebHeaderCollection webHeaderCollection = webClient.ResponseHeaders;

                if (webHeaderCollection.AllKeys.Contains("X-Staticmap-API-Warning"))
                {
                    result.CorrectResponse = false;
                    return result;
                }
                else
                {
                    result.Data = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                    result.CorrectResponse = true;
                    return result;
                }
            }
            catch (Exception)
            {
                return new GoogleApiResult()
                {
                    CorrectResponse = false
                };
            }
        }

        public async Task<GoogleApiResult> GetGeocodeData(string address)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://maps.googleapis.com/maps/api");

            var request = new RestRequest();
            request.Resource = "/geocode/json?";
            request.AddQueryParameter("address", address);
            request.AddQueryParameter("language", "cs");
            request.AddQueryParameter("key", _apiKey);
            request.Method = Method.GET;

            var response = await client.ExecuteTaskAsync(request);
            var data = JsonConvert.DeserializeObject<Geocode>(response.Content);

            var apiResult = new GoogleApiResult()
            {
                CorrectResponse = (data != null && data.results.Count() != 0),
                Data = data
            };

            return apiResult;
        }
    }
}