using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Service.StockMarket.Entities.JSON.Response;

namespace Service.StockMarket.Repositories
{
    public class RESTRepository
    {
        public string Endpoint { get; set; }

        public IEnumerable<SymbolResponse> GetSymbols()
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Endpoint);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var response = streamReader.ReadToEnd();

                    return JsonConvert.DeserializeObject<IEnumerable<SymbolResponse>>(response);
                }
            }
            catch (Exception) { throw; }
        }

        public IEnumerable<CountryResponse> GetCountries()
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Endpoint);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var response = streamReader.ReadToEnd();

                    return JsonConvert.DeserializeObject<IEnumerable<CountryResponse>>(response);
                }
            }
            catch (Exception) { throw; }
        }

        public ProfileResponse GetCompanyProfile()
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Endpoint);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var response = streamReader.ReadToEnd();

                    return JsonConvert.DeserializeObject<ProfileResponse>(response);
                }
            }
            catch (Exception) { throw; }
        }
    }
}
