using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MarsUpdates.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MarsUpdates.Flyouts
{
    public sealed partial class TweetsList : UserControl
    {
        FeedsViewModel feedsViewModel;
        public TweetsList()
        {
            this.InitializeComponent();
        }

        public void Show(Page page, AppBar appbar, Button button, object context)
        {
            TweetsListPopup.IsOpen = true;
            feedsViewModel = (FeedsViewModel)context;
            TweetList.ItemsSource = feedsViewModel.TwitterFeedsList;
            FlyoutHelper.ShowRelativeToAppBar(TweetsListPopup, page, appbar, button);
        }

        void AddButtonClick(object sender, RoutedEventArgs e)
        {
            if (AddTwitterStack.Visibility != Visibility.Visible)
            {
                AddTwitterChannel.Text = "";
                AddTwitterChannel.Focus(Windows.UI.Xaml.FocusState.Pointer);
                TwitterChannelsStack.Visibility = Visibility.Collapsed;
                AddTwitterStack.Visibility = Visibility.Visible;
            }
            else
            {
                TwitterChannelsStack.Visibility = Visibility.Visible;
                AddTwitterStack.Visibility = Visibility.Collapsed;
            }
        }

        void OKClick(object sender, RoutedEventArgs e)
        {
            feedsViewModel.AddNewFeed(AddTwitterChannel.Text);
            AddTwitterChannel.Text = "";
            AddButtonClick(this, e);
        }

        void FeedsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            feedsViewModel.SelectedItemIndex = TweetList.SelectedIndex;
        }

        void RemoveSelectedClick(object sender, RoutedEventArgs e)
        {
            int index = TweetList.SelectedIndex;
            if (index > -1)
            {
                feedsViewModel.TwitterFeedsList.RemoveAt(index);
                AppSettingsData.TweetsToLoad = SaveLeftTwitters(AppSettingsData.TweetsToLoad[index]);
            }
        }

        private string[] SaveLeftTwitters(string nameForRemoval)
        {
            string[] newTweets = new string[feedsViewModel.TwitterFeedsList.Count];
            if (newTweets.Length == 0)
            {
                return newTweets;
            }
            int tIndex = 0;
            for (int i = 0; i < AppSettingsData.TweetsToLoad.Length; i++)
            {
                if (AppSettingsData.TweetsToLoad[i] != nameForRemoval)
                {
                    newTweets[tIndex++] = AppSettingsData.TweetsToLoad[i];
                }
            }
            return newTweets;
        }
    }
}
