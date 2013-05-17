using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MarsUpdates.Data
{
    
    class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<WeatherReport> reportsList;
        private ObservableCollection<WeatherReport> queriedList;
        private ObservableCollection<Tweet> tweetsList; //other tweets
        private ObservableCollection<string> linksFromTweets;
        private int selectedItemIndex;

        public ViewModel()
        {
            reportsList = new ObservableCollection<WeatherReport>();
            queriedList = new ObservableCollection<WeatherReport>();
            tweetsList = new ObservableCollection<Tweet>();
            linksFromTweets = new ObservableCollection<string>();

            selectedItemIndex = -1;
            LoadingFeed = true;
        }

        public ObservableCollection<WeatherReport> ReportsList
        {
            get { return reportsList; }
            set {
                if (value.Count > 0 && reportsList != null)
                {
                    for (int i = 0; i < reportsList.Count; i++)
                    {
                        value.Add(reportsList[i]);
                    }
                    reportsList = value;
                }
                else if (reportsList == null)
                {
                    reportsList = value;
                }
                NotifyPropertyChanged("LoadComplete"); }
        }

        public ObservableCollection<WeatherReport> QueriedList
        {
            get 
            {
                if (queriedList.Count > 0)
                {
                    return queriedList;
                }
                else
                {
                    return reportsList;
                }
            }
            set { queriedList = value; NotifyPropertyChanged("QueryLoaded"); }
        }

        public ObservableCollection<string> LinksFromTweets
        {
            get { return linksFromTweets; }
            set
            {
                linksFromTweets = value;
                NotifyPropertyChanged("LoadComplete");
            }
        }

        public ObservableCollection<WeatherReport> LoadReportCache
        {
            get { return reportsList;  }
            set { reportsList = value; }
        }

        public ObservableCollection<Tweet> TweetList
        {
            get { return tweetsList; }
            set { tweetsList = value; NotifyPropertyChanged("TweetsLoaded"); }
        }

        public void RefreshTweets()
        {
            NotifyPropertyChanged("TweetsLoaded");
        }

        public bool LoadReportInListView()
        {
            if (queriedList.Count != 0)
            {
                NotifyPropertyChanged("ReportLoaded");
                return true;
            }
            return false;
        }

        public ObservableCollection<Tweet> UpdateTweetListWithoutRefresh
        {
            get { return tweetsList; }
            set { tweetsList = value; }
        }

        public int SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set { selectedItemIndex = value; NotifyPropertyChanged("SelectedItemIndex"); }
        }

        public string SelectedTwitterAvatar
        {
            get { return tweetsList[selectedItemIndex].GetAvatarUrl; }
        }

        

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public bool LoadingFeed
        {
            get;
            set;
        }

    }
}
