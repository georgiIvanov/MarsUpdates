using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MarsUpdates.Data
{

    class FeedsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TweetFeedName> twitterFeeds;
        private int selectedItemIndex;

        public FeedsViewModel()
        {
            twitterFeeds = new ObservableCollection<TweetFeedName>();
            selectedItemIndex = -1;
        }

        public async Task LoadTwitterFeeds()
        {
            await Task.Run(new Action(GetFeeds));
        }

        void GetFeeds()
        {
            while (AppSettingsData.TweetsToLoad == null)
            {

            }

            if (AppSettingsData.TweetsToLoad == null)
            {
                return;
            }
            string[] tweets = AppSettingsData.TweetsToLoad;
            int index = 0;
            twitterFeeds = new ObservableCollection<TweetFeedName>(
                from i in Enumerable.Range(0, tweets.Length)
                where tweets[index] != null
                select new TweetFeedName(tweets[index++]));

        }

        public void AddNewFeed(string feed)
        {
            if (feed == "" || feed == null || feed.Length > 66)
            {
                return;
            }

            feed = "https://api.twitter.com/1/statuses/user_timeline.json?screen_name=" + feed + "&count=20";

            //TweetFeedName name = new TweetFeedName(feed);

            if (!ContainsTwitter(feed))
            {
                twitterFeeds.Add(new TweetFeedName(feed));
                AppSettingsData.AddTwitterChannel(feed);
            }

            //if (!twitterFeeds.Contains(name))
            //{
                
            //}

            
        }

        bool ContainsTwitter(string name)
        {
            foreach (var item in twitterFeeds)
            {
                if (item.GetFullRequest.ToLower() == name.ToLower())
                {
                    return true;
                }
            }
            return false;
        }


        public ObservableCollection<TweetFeedName> TwitterFeedsList
        {
            get { return twitterFeeds; }
            set
            {
                //if (value.Count > 0 && twitterFeeds != null)
                //{
                //    for (int i = 0; i < twitterFeeds.Count; i++)
                //    {
                //        value.Add(twitterFeeds[i]);
                //    }
                //    twitterFeeds = value;
                //}
                //else if (twitterFeeds == null)
                //{
                //    twitterFeeds = value;
                //}
                NotifyPropertyChanged("LoadComplete");
            }
        }

        public ObservableCollection<TweetFeedName> LoadReportCache
        {
            get { return twitterFeeds; }
            set { twitterFeeds = value; }
        }


        public int SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set { selectedItemIndex = value; NotifyPropertyChanged("SelectedItemIndex"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    }

    class TweetFeedName
    {
        string name;
        public TweetFeedName(string name)
        {
            this.name = name;
        }
        public string GetName
        {
            get
            {
                if (name != null)
                {
                    return name.Substring(66, name.IndexOf('&') - 66);
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set { this.name = value; }
        }

        public string GetFullRequest
        {
            get { return name; }
        }
    }
}
