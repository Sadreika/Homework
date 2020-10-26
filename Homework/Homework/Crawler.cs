using System;
using System.Collections.Generic;
using RestSharp;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Homework
{
    public class Crawler
    {
        public string airports = "";
        public string times = "";
        public string cheapestPrice = "";
        public string taxes = "";
        public void crawling()
        {
            string content = loadingDataCollectionPage(gettingCookies());
            extractData(content);
        }
        public Crawler() { }
        private IList<RestResponseCookie> gettingCookies()
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
        private void extractData(string content)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            List<string> htmlDataBlocks = new List<string>();

            foreach (HtmlNode data in htmlDocument.DocumentNode.SelectNodes("//tr")) //Dar netestavau
            {
                if (!data.InnerHtml.Equals("\n")) htmlDataBlocks.Add(data.InnerHtml);
            }
            foreach (string htmlDataBlock in htmlDataBlocks)
            {
                Crawler flight = new Crawler();

                List<string> departureAndArrivalAirports = new List<string>();
                htmlDocument.LoadHtml(htmlDataBlock);
                string query = "";
                try
                {
                    foreach (HtmlNode data in htmlDocument.DocumentNode.SelectNodes("//small"))
                    {
                        if (!data.InnerText.Contains(":") && !data.InnerText.Contains(";")) departureAndArrivalAirports.Add(data.InnerText);
                    }
                    foreach (string airport in departureAndArrivalAirports)
                    {
                        flight.airports = flight.airports + "|" + airport;
                    }
                }
                catch (Exception ex) { }

                List<string> priceAndDepartureArrivalTimes = new List<string>();
                try
                {
                    foreach (HtmlNode data in htmlDocument.DocumentNode.SelectNodes("//input"))
                    {
                        priceAndDepartureArrivalTimes.Add(data.OuterHtml);
                    }
                    foreach (string departureAndArrivalTimes in priceAndDepartureArrivalTimes)
                    {
                        query = "[0-9]+-[0-9]+-[0-9]+ [0-9:]+";
                        Regex regex = new Regex(query);
                        MatchCollection match = regex.Matches(departureAndArrivalTimes);
                        foreach (Match time in match)
                        {
                            if (!flight.times.Contains(time.Value)) flight.times = flight.times + "|" + time.Value;
                        }
                    }
                }
                catch (Exception ex) { }

                try
                {
                    foreach (string departureAndArrivalTimes in priceAndDepartureArrivalTimes)
                    {
                        query = "[0-9]+\\.[0-9]+";
                        Regex regex = new Regex(query);
                        MatchCollection match = regex.Matches(departureAndArrivalTimes);
                        if (flight.cheapestPrice.Equals("")) flight.cheapestPrice = match[0].Value;
                        try
                        {
                            if (Convert.ToDouble(flight.cheapestPrice) > Convert.ToDouble(match[0].Value)) flight.cheapestPrice = match[0].Value;
                        }
                        catch (Exception ex){ }
                    }
                }
                catch (Exception ex) { }
                Console.WriteLine();
            }
        }
    }
}
