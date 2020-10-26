using System;
using System.Collections.Generic;
using RestSharp;
using HtmlAgilityPack;

namespace Homework
{
    public class Crawler
    {
        public void crawling()
        {
            string content = loadingDataCollectionPage(gettingCookies());
        }

        private IList<RestResponseCookie> gettingCookies ()
        {
            RestClient client = new RestClient("https://www.starperu.com/");

            client.AddDefaultHeader("Host", "www.starperu.com");
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0";
            client.AddDefaultHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.AddDefaultHeader("Accept-Language", "lt,en-US;q=0.8,en;q=0.6,ru;q=0.4,pl;q=0.2");
            client.AddDefaultHeader("Accept-Encoding", "gzip, deflate, br");
            client.AddDefaultHeader("DNT", "1");
            client.AddDefaultHeader("Connection", "keep-alive");
            client.AddDefaultHeader("Upgrade-Insecure-Requests", "1");
            client.FollowRedirects = true; // Jeigu false, nedarys redirekto ir vis tiek cookius surinks, bet nesu tikras ar nepaveiks sekanciu requestu
   
            RestRequest request = new RestRequest("", Method.GET);
            IRestResponse response = client.Execute(request);
    
            return response.Cookies;
        }
        private string loadingDataCollectionPage(IList<RestResponseCookie> cookieJar)
        {
            RestClient client = new RestClient("https://www.starperu.com/");
            RestRequest request = new RestRequest("Booking1", Method.POST);

            client.AddDefaultHeader("Host", "www.starperu.com");
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0";
            client.AddDefaultHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.AddDefaultHeader("Accept-Language", "lt,en-US;q=0.8,en;q=0.6,ru;q=0.4,pl;q=0.2");
            //client.AddDefaultHeader("Accept-Encoding", "gzip, deflate, br"); nereikia atkoduoti, bet gali atsirasti problemu veliau
            client.AddDefaultHeader("Content-Type", "application/x-www-form-urlencoded");
            client.AddDefaultHeader("Origin", "https://www.starperu.com");
            client.AddDefaultHeader("DNT", "1");
            client.AddDefaultHeader("Connection", "keep-alive");
            client.AddDefaultHeader("Referer", "https://www.starperu.com/es");
            client.AddDefaultHeader("Upgrade-Insecure-Requests", "1");
            foreach (RestResponseCookie cookie in cookieJar)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            string postBody = "tipo_viaje=R&origen=LIM&destino=IQT&date_from=07%2F11%2F2020&date_to=14%2F11%2F2020&cant_adultos=1&cant_ninos=0&cant_infantes=0&codigo_desc=";
            request.AddParameter("text/xml", postBody, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response.Content;
        }
        private void extractData ()
        {

        }
    }
}
