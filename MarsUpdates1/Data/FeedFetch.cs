using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.Data.Json;
using System.Net.Http;
using Windows.Devices.Geolocation;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net;
using Windows.Web;
using System.Net.Http.Headers;

namespace MarsUpdates.Data
{
    class FeedFetch
    {
        public static async Task<ObservableCollection<WeatherReport>> GetWeatherFeedAsync(string query)
        {
            ObservableCollection<WeatherReport> result = new ObservableCollection<WeatherReport>();

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage httpResult = new HttpResponseMessage();
            try
            {
                httpResult = await httpClient.GetAsync(query);
            }
            catch (HttpRequestException)
            {
                return result;
            }

            string content = await httpResult.Content.ReadAsStringAsync();


            JsonArray jsonReports = new JsonArray();
            
            if(!JsonArray.TryParse(content, out jsonReports))
            {
                return result;
            }
            

            if (jsonReports.Count > 0)
            {
                SetLatestID(jsonReports[0].GetObject().GetNamedValue("id_str").Stringify());
            }

            foreach (var item in jsonReports)
            {
                JsonObject jsObj = item.GetObject();
                content = jsObj.GetNamedValue("text").Stringify();
                if(content.StartsWith("\"Sol "))
                {
                    result.Add(CreateReport(content, 
                                            jsObj.GetNamedValue("created_at").Stringify()));
                }
            }

            return result;
        }

        public static void SetLatestID(string id)
        {
            AppSettingsData.LatestReportID = id;
        }

        public static async Task<ObservableCollection<Tweet>> GetTweetsAsync(string[] feedsToGet)
        {
            ObservableCollection<Tweet> result = new ObservableCollection<Tweet>();
            HttpClient httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.ExpectContinue = false;
            
            string content = "";
            foreach (var feed in feedsToGet)
            {
                //TwitterAuth.ResourceURL = feed;
                //httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(TwitterAuth.CreateGetAuthHeader(feed));
                try
                {
                    HttpResponseMessage httpResult = await httpClient.GetAsync(feed);
                    content = await httpResult.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {
                    return result;
                }
                if (content.Length < 80)
                {
                    continue;
                }

                JsonArray jsonTweets = new JsonArray();

                if (!JsonArray.TryParse(content, out jsonTweets))
                {
                    return result;
                }

                foreach (var rawTweet in jsonTweets)
                {
                    JsonObject jsObj = rawTweet.GetObject();
                    content = jsObj.GetNamedValue("text").Stringify();
                    if (!content.StartsWith("\"Sol "))
                    {
                        JsonObject user = jsObj.GetNamedObject("user");
                        result.Add(GetTweet(content, 
                                            user.GetNamedValue("screen_name").Stringify(),
                                            jsObj.GetNamedValue("created_at").Stringify(),
                                            user.GetNamedValue("profile_image_url").Stringify()));
                    }
                }
            }


            var ordered = (from item in result
                           orderby item.DatePublished descending
                           select item).ToList();
            result.Clear();
            foreach (var item in ordered)
            {
                result.Add(item);
            }
            
            return result;
        }

        static Tweet GetTweet(string content, string publisher, string pubDateString, string avaUrl)
        {
            
            StringBuilder sb = new StringBuilder();
            string[] descriptionSplit = pubDateString.Split(' ');
            sb.AppendFormat("{0} {1} {2} {3}",
                descriptionSplit[2], descriptionSplit[1],
                descriptionSplit[5].Substring(0, descriptionSplit[5].Length - 1),
                descriptionSplit[3]);
            sb.Length--;

            DateTime pubDate = DateTime.Parse(sb.ToString());
            sb.Clear();

            Tweet t = new Tweet(content.Substring(1, content.Length - 2),
                                publisher.Substring(1, publisher.Length - 2),
                                pubDate,
                                avaUrl.Substring(1, avaUrl.Length - 2 )
                                );

            return t;
        }

        //public static async Task<ObservableCollection<Tweet>> GetTweetsAsyncXML()
        //{
        //    ObservableCollection<Tweet> result = new ObservableCollection<Tweet>();
        //    string[] feedsToGet = { "https://api.twitter.com/1/statuses/user_timeline.rss?screen_name=MarsCuriosity",
        //                            "https://api.twitter.com/1/statuses/user_timeline.rss?screen_name=MarsWxReport",
        //                            "https://api.twitter.com/1/statuses/user_timeline.rss?screen_name=Nasa"};
        //    for (int i = 0; i < feedsToGet.Length; i++)
        //    {
        //        XDocument xmlDoc = XDocument.Load(feedsToGet[i]);

        //        IEnumerable<XElement> twitterItems = xmlDoc.Root.Descendants("item");

        //        foreach (var item in twitterItems)
        //        {
        //            string content = item.Value;
        //            if (!content.Contains("MarsWxReport: Sol "))
        //            {
        //                result.Add(GetTweetXML(item));
        //            }
        //        }
        //    }
        //    var ordered = (from item in result
        //                  orderby item.DatePublished descending
        //                  select item).ToList();
        //    result.Clear();
        //    foreach (var item in ordered)
        //    {
        //        result.Add(item);
        //    }
            
        //    //viewModel.LoadingFeed = true;
        //    return result;
        //}

        //static Tweet GetTweetXML(XElement item)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    string str = item.Descendants("pubDate").First().Value;
        //    string[] descriptionSplit = str.Split(' ');
        //    sb.AppendFormat("{0} {1} {2} {3}",
        //        descriptionSplit[2], descriptionSplit[1], descriptionSplit[3], descriptionSplit[4]);
        //    DateTime datePublished = DateTime.Parse(sb.ToString());

        //    str = item.Descendants("description").First().Value;
        //    int nameEnd = str.IndexOf(':');
        //    string name = str.Substring(0, nameEnd);
            

        //    Tweet tweet = new Tweet(str.Substring(++nameEnd, str.Length - nameEnd), name, datePublished, "");

        //    return tweet;
        //}

        static WeatherReport CreateReport(string text, string pubDate)
        {
            WeatherReport buildReport = new WeatherReport();
            StringBuilder sb = new StringBuilder();

            //there is a bug parsing raw publish date, so it has to be formatted
            string[] descriptionSplit = pubDate.Split(' ');
            sb.AppendFormat("{0} {1} {2} {3}",
                descriptionSplit[2], descriptionSplit[1], 
                descriptionSplit[5].Substring(0, descriptionSplit[5].Length -1),
                descriptionSplit[3]);
            sb.Length--;

            buildReport.PubDate = DateTime.Parse(sb.ToString());
            sb.Clear();

            descriptionSplit = text.Split(':');
            Match number = Regex.Match(descriptionSplit[0], @"[0-9]+");
            buildReport.Sol = int.Parse(number.Value.ToString());

            string[] splitStr = descriptionSplit[0].Split('(');
            //buildReport.Sol = int.Parse(splitStr[0].Split()[2]);
            sb.Append(splitStr[1]);
            sb.Length--;

            DateTime tryParse;
            if (!DateTime.TryParse(sb.ToString(), out tryParse))
            {
                buildReport = BadMeasureDate(buildReport, sb);
            }
            else
            {
                buildReport.MeasuredDate = tryParse;
            }

            descriptionSplit = descriptionSplit[1].Split(',');
            buildReport.Weather = descriptionSplit[0].Trim();

            MatchCollection numbers = Regex.Matches(descriptionSplit[1], @"-*[0-9,\.\,]+");
            buildReport.TempretureHighC = ParseToDouble(numbers[0].ToString()); // double.Parse(numbers[0].ToString());
            if (numbers.Count >= 2)
            {
                buildReport.TempretureHighF = ParseToDouble(numbers[1].ToString()); // double.Parse(numbers[1].ToString());
            }
            else
            {
                if (descriptionSplit.Length < 7)
                {
                    return buildReport;
                }
                descriptionSplit[2] = descriptionSplit[3];
                descriptionSplit[3] = descriptionSplit[4];
                descriptionSplit[4] = descriptionSplit[5];
                descriptionSplit[5] = descriptionSplit[6];
            }

            numbers = Regex.Matches(descriptionSplit[2], @"-*[0-9,\.]+");
            if (numbers.Count == 2)
            {
                buildReport.TempretureLowC = ParseToDouble(numbers[0].ToString()); // double.Parse(numbers[0].ToString());
                buildReport.TempretureLowF = ParseToDouble(numbers[1].ToString()); // double.Parse(numbers[1].ToString());
            }
            else
            {

            }

            numbers = Regex.Matches(descriptionSplit[3], @"-*[0-9,\.]+");
            double pressureValue = ParseToDouble(numbers[0].ToString());
            if (pressureValue > 100)
            {
                string pressureStr = numbers[0].ToString();
                buildReport.PressureHPA = ParseToDouble(string.Format("{0}.{1}{2}", pressureStr[0], pressureStr[1], pressureStr[2]));
            }
            else
            {
                buildReport.PressureHPA = pressureValue; // double.Parse(numbers[0].ToString());
            }

            numbers = Regex.Matches(descriptionSplit[4], @"-*[0-9,\.]+");
            if (numbers.Count == 2)
            {
                buildReport.WindSpeedKmh = ParseToDouble(numbers[0].ToString()); // double.Parse(numbers[0].ToString());
                buildReport.WindSpeedMph = ParseToDouble(numbers[1].ToString()); // double.Parse(numbers[1].ToString());
                buildReport.WindDirection = GetDirection(descriptionSplit[4].Split(new char[] { 'a', 't' })[0]);
            }

            

            numbers = Regex.Matches(descriptionSplit[5], @"[0-9,\.]+");
            if (numbers.Count == 2)
            {
                buildReport.DaylightStartAM = ParseToDouble(numbers[0].ToString()); // double.Parse(numbers[0].ToString());
                buildReport.DaylightEndPM = ParseToDouble(numbers[1].ToString()); // double.Parse(numbers[1].ToString()); 
            }

            return buildReport;
        }

        static WeatherReport BadMeasureDate(WeatherReport buildReport, StringBuilder sb)
        {
            string badDate = sb.ToString();
            if (badDate.Contains("ene"))
            {
                buildReport.MeasuredDate = new DateTime(2013, 01, int.Parse(sb[0].ToString() + sb[1]));
            }
            else if (badDate.StartsWith("Ene"))
            {
                buildReport.MeasuredDate = new DateTime(2013, 01, int.Parse(sb[4].ToString() + sb[5]));
            }
            else if (badDate.EndsWith("UTC"))
            {
                badDate = badDate.Remove(badDate.IndexOf("UTC"));
                if (badDate.StartsWith("Sept"))
                {
                    badDate = badDate.Replace("Sept", "Sep");
                }
                buildReport.MeasuredDate = DateTime.Parse(badDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                //buildReport.MeasuredDate = DateTime.ParseExact(badDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            }
            else if (badDate.StartsWith("Sept"))
            {
                if (badDate.StartsWith("Sept."))
                {
                    badDate = badDate.Replace("Sept.", "Sep");
                    buildReport.MeasuredDate = DateTime.Parse(badDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                }
                else if (badDate.StartsWith("Sept"))
                {
                    badDate = badDate.Replace("Sept", "Sep");
                    buildReport.MeasuredDate = DateTime.Parse(badDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                }

            }

            return buildReport;
        }

        static WeatherReport CreateReportXML(XElement item)
        {
            WeatherReport buildReport = new WeatherReport();
            StringBuilder sb = new StringBuilder();

            //there is a bug parsing raw publish date, so it has to be formatted
            string str = item.Descendants("pubDate").First().Value;
            string[] descriptionSplit = str.Split(' ');
            sb.AppendFormat("{0} {1} {2} {3}", 
                descriptionSplit[2], descriptionSplit[1], descriptionSplit[3], descriptionSplit[4]);
            buildReport.PubDate = DateTime.Parse(sb.ToString());
            sb.Clear();

            str = item.Descendants("description").First().Value;
            descriptionSplit = str.Split(':');
            string[] splitStr = descriptionSplit[1].Split('(');
            buildReport.Sol = int.Parse(splitStr[0].Split()[2]);
            sb.Append(splitStr[1]);
            sb.Length--;
            
            buildReport.MeasuredDate = DateTime.Parse(sb.ToString());

            descriptionSplit = descriptionSplit[2].Split(',');
            buildReport.Weather = descriptionSplit[0].Trim();

            

            MatchCollection numbers = Regex.Matches(descriptionSplit[1], @"-*[0-9,\.]+");
            buildReport.TempretureHighC = ParseToDouble(numbers[0].ToString()); // double.Parse(numbers[0].ToString());
            buildReport.TempretureHighF = ParseToDouble(numbers[1].ToString()); // double.Parse(numbers[1].ToString());

            numbers = Regex.Matches(descriptionSplit[2], @"-*[0-9,\.]+");
            buildReport.TempretureLowC = ParseToDouble(numbers[0].ToString()); // double.Parse(numbers[0].ToString());
            buildReport.TempretureLowF = ParseToDouble(numbers[1].ToString()); // double.Parse(numbers[1].ToString());

            numbers = Regex.Matches(descriptionSplit[3], @"-*[0-9,\.]+");
            buildReport.PressureHPA = ParseToDouble(numbers[0].ToString()); // double.Parse(numbers[0].ToString());

            numbers = Regex.Matches(descriptionSplit[4], @"-*[0-9,\.]+");
            buildReport.WindSpeedKmh = ParseToDouble(numbers[0].ToString()); // double.Parse(numbers[0].ToString());
            buildReport.WindSpeedMph = ParseToDouble(numbers[1].ToString()); // double.Parse(numbers[1].ToString());

            buildReport.WindDirection = GetDirection(descriptionSplit[4].Split(new char[] { 'a', 't' })[0]);

            numbers = Regex.Matches(descriptionSplit[5], @"[0-9,\.]+");
            buildReport.DaylightStartAM = ParseToDouble(numbers[0].ToString()); // double.Parse(numbers[0].ToString());
            buildReport.DaylightEndPM = ParseToDouble(numbers[1].ToString()); // double.Parse(numbers[1].ToString()); 


            return buildReport;
        }

        static Func<string, double> ParseToDouble = (x) =>
        {
            return double.Parse(x.ToString());
        };

        static WindDirection GetDirection(string input)
        {
            WindDirection direction = new WindDirection();

            if (input.Contains(" E "))
            {
                direction = WindDirection.East;
            }
            else if(input.Contains(" N "))
            {
                direction = WindDirection.North;
            }
            else if (input.Contains(" W "))
            {
                direction = WindDirection.West;
            }
            else if (input.Contains(" S "))
            {
                direction = WindDirection.South;
            }

            return direction;
        }
    }
}