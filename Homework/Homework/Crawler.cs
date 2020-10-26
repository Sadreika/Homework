using System;
using System.Collections.Generic;
using RestSharp;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data;

namespace Homework
{
    public class Crawler
    {
        public string airports = "";
        public string times = "";
        public string cheapestPrice = "";
        public string taxes = "";
        public string priceString = "";
        public string tableId = "";
        public void crawling()
        {
            IList<RestResponseCookie> cookieJar = gettingCookies();
            string content = loadingDataCollectionPage(cookieJar);
            List<Crawler> collectedData = extractData(content);
            gettingTaxes(cookieJar, collectedData);
           
        }
        public Crawler() { }

        private void showData (string airports, string times, string cheapestPrice, string airportsSecond, string timesSecond, string cheapestPriceSecond, string price, string taxes, string priceWithTaxes)
        {
            Console.WriteLine(airports + " " + times + " " + cheapestPrice + "\n" + airportsSecond + " " + timesSecond + " " + cheapestPriceSecond + "\nPrice: " + price + "\nTaxes: " + taxes + "\nPrice with taxes: " + priceWithTaxes + "\n");
        }
        private IList<RestResponseCookie> gettingCookies()
        {
            RestClient client = new RestClient("https://www.starperu.com/");

            client.AddDefaultHeader("Host", "www.starperu.com");
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0";
            client.AddDefaultHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.AddDefaultHeader("Accept-Language", "lt,en-US;q=0.8,en;q=0.6,ru;q=0.4,pl;q=0.2");
            client.AddDefaultHeader("DNT", "1");
            client.AddDefaultHeader("Connection", "keep-alive");
            client.AddDefaultHeader("Upgrade-Insecure-Requests", "1");
            client.FollowRedirects = false;

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
        private List<Crawler> extractData(string content)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);
            List<Crawler> collectedDataList = new List<Crawler>();

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
                        string queryForTableNumber = "id=\"[a-zA-Z0-9]+\" value";
                        Regex regex = new Regex(query);

                        MatchCollection match = regex.Matches(departureAndArrivalTimes);
                        regex = new Regex(queryForTableNumber);
                        MatchCollection tableMatch = regex.Matches(departureAndArrivalTimes);

                        if (flight.cheapestPrice.Equals(""))
                        {
                            flight.cheapestPrice = match[0].Value;
                            flight.priceString = departureAndArrivalTimes;
                            flight.tableId = tableMatch[0].Value;
                        }
                        
                        try
                        {
                            if (Convert.ToDouble(flight.cheapestPrice) > Convert.ToDouble(match[0].Value))
                            {
                                flight.cheapestPrice = match[0].Value;
                                flight.priceString = departureAndArrivalTimes;
                                flight.tableId = tableMatch[0].Value;
                            }
                        }
                        catch (Exception ex){ }
                    }
                }
                catch (Exception ex) { }
                if(!flight.airports.Equals(""))
                {
                    collectedDataList.Add(flight);
                }
            }
            return collectedDataList;
        }
        private void gettingTaxes(IList<RestResponseCookie> cookieJar, List<Crawler> collectedData)
        {
            RestClient client = new RestClient("https://www.starperu.com/");
            List<Crawler> combinationList = new List<Crawler>();
            client.AddDefaultHeader("Host", "www.starperu.com");
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0";
            client.AddDefaultHeader("Accept", "*/*");
            client.AddDefaultHeader("Accept-Language", "lt,en-US;q=0.8,en;q=0.6,ru;q=0.4,pl;q=0.2");
            client.AddDefaultHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            client.AddDefaultHeader("X-Requested-With", "XMLHttpRequest");
            client.AddDefaultHeader("Origin", "https://www.starperu.com");
            client.AddDefaultHeader("DNT", "1");
            client.AddDefaultHeader("Connection", "keep-alive");
            client.AddDefaultHeader("Referer", "https://www.starperu.com/Booking1");
            
            string query = "\"[0-9.|A-Z- :]+\"";
            Regex regex = new Regex(query);
            string postBody = "";

            for (int i = 0; i < collectedData.Count; i++)
            {
                for(int j = i + 1; j <= collectedData.Count - 1; j++)
                {
                    Console.WriteLine("\n");
                    if(collectedData[i].tableId.Equals("id=\"exampleRadios2\" value") && collectedData[j].tableId.Equals("id=\"Radios2\" value"))
                    {
                        RestRequest request = new RestRequest("Booking1/ObtenerTarifas", Method.POST);
                        foreach (RestResponseCookie cookie in cookieJar)
                        {
                            request.AddCookie(cookie.Name, cookie.Value);
                        }
                        MatchCollection firstTable = regex.Matches(collectedData[i].priceString);
                        MatchCollection secondTable = regex.Matches(collectedData[j].priceString);
                        string firstTableString = firstTable[0].Value;
                        string secondTableString = secondTable[0].Value;
                        firstTableString = firstTableString.Trim('"');
                        secondTableString = secondTableString.Trim('"');
                        firstTableString = changingTime(firstTableString);
                        secondTableString = changingTime(secondTableString);

                        postBody = "cod_origen=LIM&cod_destino=IQT&cant_adl=1&cant_chd=0&cant_inf=0&codigo_desc=&fecha_ida=2020-11-07&fecha_retorno=2020-11-14&tipo_viaje=R&grupo_retorno=" + secondTableString + "&grupo_ida=" + firstTableString;
                        request.AddParameter("text/xml", postBody, ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);

                        string queryForTaxes = "[0-9.]+<";
                        Regex regexForTaxes = new Regex(queryForTaxes);
                        MatchCollection matchTaxes = regexForTaxes.Matches(response.Content);

                        showData(collectedData[i].airports, collectedData[i].times, collectedData[i].cheapestPrice, collectedData[j].airports, collectedData[j].times, collectedData[j].cheapestPrice, matchTaxes[0].Value, matchTaxes[1].Value, matchTaxes[2].Value);
                    }
                }
            }
        }

        private string changingTime(string postBodyString)
        {
            string[] splitPostBodyString = postBodyString.Split('|');
            DateTime firstDateTime = DateTime.ParseExact(splitPostBodyString[3], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime updatedFirstDateTime = firstDateTime.Add(new TimeSpan(3, 30, 0));
            DateTime secondDateTime = DateTime.ParseExact(splitPostBodyString[4], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime updatedSecondDateTime = secondDateTime.Add(new TimeSpan(3, 30, 0));


            string fixedPostBodyString = splitPostBodyString[0] + "|" +
                splitPostBodyString[1] + "|" +
                splitPostBodyString[2] + "|" +
                updatedFirstDateTime.Year + "-" + updatedFirstDateTime.Month + "-" + updatedFirstDateTime.Day + " " + updatedFirstDateTime.Hour + ":" + updatedFirstDateTime.Minute + ":00|" +
                updatedSecondDateTime.Year + "-" + updatedSecondDateTime.Month + "-" + updatedSecondDateTime.Day + " " + updatedSecondDateTime.Hour + ":" + updatedSecondDateTime.Minute + ":00|" +
                splitPostBodyString[5];
         
            return fixedPostBodyString;
        }
    }
}
