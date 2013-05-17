using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.Storage;
using Windows.UI.Popups;
using System.Diagnostics;

namespace MarsUpdates.Data
{
    static class AppSettingsData
    {
        const string latestReportIDSetting = "latestReportID";
        const string updateIntervalSetting = "updateIntervalSetting";
        public const string twitterChannelsFile = "tweetsToLoad";
        public const string reportsFileName = "reports";
        static string latestReportID;
        static int interval;

        public static string LatestReportID
        {
            get { return latestReportID; }
            set { SaveSetting<string>(latestReportIDSetting, value); latestReportID = value; }
        }

        public static string GetReportQuery
        {
            get {
                if (latestReportID != null)
                {
                    return "https://api.twitter.com/1/statuses/user_timeline.json?screen_name=MarsWxReport&since_id=" + latestReportID.Substring(1, latestReportID.Length - 2);
                }
                else
                {
                    return "https://api.twitter.com/1/statuses/user_timeline.json?screen_name=MarsWxReport&count=10000";
                }
            }
        }

        public static int UpdatesIntervalMsec
        {
            get { return interval; }
            set 
            {
                if (value > 0 && value < int.MaxValue)
                {
                    SaveSetting<int>(updateIntervalSetting, value); interval = value;
                }
                else
                {
                    SaveSetting<int>(updateIntervalSetting, 3600000); interval = 3600000;
                }
            }
        }

        public static string[] TweetsToLoad
        {
            get;
            set;
        }

        public static bool NewTweetsLoaded { get; set; }

        public static async Task<string[]> LoadTweets()
        {
            TweetsToLoad = await AppDataManager.RestoreFromLocalCacheAsync<string[]>(twitterChannelsFile);

            if (TweetsToLoad == null)
            {
                await SaveDefaultTweets();

                return TweetsToLoad;
            }
            else
            {
                
                return TweetsToLoad;
            }
        }

        public static void AddTwitterChannel(string twitterch)
        {
            string[] addedTweet = new string[TweetsToLoad.Length + 1];

            for (int i = 0; i < TweetsToLoad.Length; i++)
			{
                addedTweet[i] = TweetsToLoad[i];
			}
            addedTweet[addedTweet.Length - 1] = twitterch;
            TweetsToLoad = addedTweet;
            Task.Run(new Action(AppDataManager.GetNewTweets));
        }

        public static async void SaveTweets()
        {
            try
            {
                await AppDataManager.SaveToLocalCacheAsync<string[]>(TweetsToLoad, twitterChannelsFile);
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to write on file, access denied");
            }
        }

        public static async void SaveTweets(string[] twitterChannels)
        {
            try
            {
                await AppDataManager.SaveToLocalCacheAsync<string[]>(twitterChannels, twitterChannelsFile);
            }
            catch (Exception)
            {
                //AppDataManager.SaveToLocalCacheAsync<string[]>(twitterChannels, twitterChannelsFile);
                //await AppDataManager.SaveToLocalCacheAsync<string[]>(twitterChannels, twitterChannelsFile);
            }
        }

        private static async Task SaveDefaultTweets()
        {
            TweetsToLoad = new string[] { "https://api.twitter.com/1/statuses/user_timeline.json?screen_name=MarsCuriosity&count=20",
                                    "https://api.twitter.com/1/statuses/user_timeline.json?screen_name=MarsWxReport&count=20",
                                    "https://api.twitter.com/1/statuses/user_timeline.json?screen_name=Nasa&count=20"};

            await AppDataManager.SaveToLocalCacheAsync<string[]>(TweetsToLoad, twitterChannelsFile);
        }

        //public static bool LoadTweets { get; set; }
        public static bool CachedDataExists { get; set; }
        public static bool ShowTwitterAvatars { get; set; }
        
        

        public static void AppSettingsDataInit()
        {
            LatestReportID = GetSettings<string>(latestReportIDSetting);
            UpdatesIntervalMsec = GetSettings<int>(updateIntervalSetting);
        }


        public static void ResetSettings()
        {
            LatestReportID = "";
            ShowTwitterAvatars = true;
            CachedDataExists = false;
            SaveDefaultTweets();
            UpdatesIntervalMsec = 3600000;
        }

        public static void SaveSetting<T>(string settingName, T val)
        {
            ApplicationData.Current.LocalSettings.Values[settingName] = val;
        }

        public static T GetSettings<T>(string settingName)
        {
            var localSettings = ApplicationData.Current.LocalSettings.Values;
            if (localSettings.ContainsKey(settingName))
            {
                var val = localSettings[settingName];
                if (val is T)
                {
                    return (T)localSettings[settingName];
                }
                else
                {
                    return default(T);
                }
            }
            return default(T);
        }
    }
}
