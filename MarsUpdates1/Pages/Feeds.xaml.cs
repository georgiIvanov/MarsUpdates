using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI;
using System.ComponentModel;
using MarsUpdates.Data;

namespace MarsUpdates.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Feeds : Page
    {
        ViewModel viewModel;
        public Feeds()
        {
            this.InitializeComponent();
            SizeChanged += Feeds_SizeChanged;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ReportsList.MaxHeight = Window.Current.Bounds.Height - WeatherReportButton.Height;
            TweetsList.MaxHeight = ReportsList.MaxHeight;


            viewModel = (ViewModel)e.Parameter;
            this.DataContext = viewModel;
            if (viewModel.LoadingFeed)
            {
                ProgressRing.IsActive = true;
                ProgressRing.Width = GridLayout.ColumnDefinitions[0].Width.Value / 2 * 100;
                ProgressRing.Height = GridLayout.ColumnDefinitions[0].Width.Value / 2 * 100;
            }

            viewModel.PropertyChanged += this.PropertiesOnViewModelChanged;
            NavigationButtonsStack.Background = new SolidColorBrush(Color.FromArgb(50, 70, 23, 180));
            ReportBrowser.Visibility = Visibility.Collapsed;
        }

        void PropertiesOnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LoadComplete")
            {
                ReportsList.ItemsSource = viewModel.ReportsList;
                ProgressRing.IsActive = false;
            }
            else if (e.PropertyName == "QueryLoaded")
            {
                ReportsList.ItemsSource = viewModel.QueriedList;
            }
            else if (e.PropertyName == "ReportLoaded")
            {
                ReportsList.ItemsSource = viewModel.ReportsList;
            }
            else if (e.PropertyName == "TweetsLoaded")
            {
                TweetsList.ItemsSource = viewModel.TweetList;
            }
        }

        void ReportSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReportBrowser.Visibility = Visibility.Visible;
            viewModel.SelectedItemIndex = ReportsList.SelectedIndex;
            LoadReportBrowser();
            StatisticsBrowser.Visibility = Visibility.Collapsed;
        }

        void TweetsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedItemIndex = TweetsList.SelectedIndex;
            LinkWebView.Visibility = Visibility.Collapsed;
            LoadTwitterBrowser();
            TwitterBrowser.Visibility = Visibility.Visible;
        }

        
        

        void TwitterLinkSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TwitterLinks.SelectedIndex == -1)
            {
                return;
            }
            LinkProgressRing.Visibility = Visibility.Visible;
            LinkProgressRing.IsActive = true;
            Uri url = new Uri(viewModel.LinksFromTweets[TwitterLinks.SelectedIndex]);
            LinkWebView.Navigate(url);
        }

        void Feeds_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                GridLayout.ColumnDefinitions[0].Width = new GridLength(0);
                ReportsList.ItemTemplate = (DataTemplate)App.Current.Resources["SnappedReportItemTemplate"];
            }
            else
            {
                GridLayout.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                ReportsList.ItemTemplate = (DataTemplate)App.Current.Resources["ReportItemTemplate"];
                ShowWeatherReports(this, new RoutedEventArgs());
            }
        }

        void ShowWeatherReports(object sender, RoutedEventArgs e)
        {
            ReportsList.Visibility = Visibility.Visible;
            //WeatherReportButton.Background = new SolidColorBrush(Color.FromArgb(250, 70, 23, 180));
            //TweetsButton.Background = new SolidColorBrush(Color.FromArgb(50, 70, 23, 180));
            TweetsList.Visibility = Visibility.Collapsed;
            TwitterBrowser.Visibility = Visibility.Collapsed;
            StatisticsBrowser.Visibility = Visibility.Collapsed;
        }

        void ShowTweets(object sender, RoutedEventArgs e)
        {
            if (AppSettingsData.NewTweetsLoaded)
            {
                viewModel.RefreshTweets();
                AppSettingsData.NewTweetsLoaded = false;
            }

            TweetsList.Visibility = Visibility.Visible;
            //WeatherReportButton.Background = new SolidColorBrush(Color.FromArgb(50, 70, 23, 180));
            //TweetsButton.Background = new SolidColorBrush(Color.FromArgb(250, 70, 23, 180));
            ReportsList.Visibility = Visibility.Collapsed;

            TwitterBrowser.Visibility = Visibility.Visible;
            TwitterContent.Width = TwitterBrowser.ActualWidth * 3 / 5;
            TwitterLinks.Width = TwitterBrowser.ActualWidth * 3 / 5;
            LinkWebView.Width = TwitterBrowser.ActualWidth;
            LinkWebView.Height = TwitterBrowser.ActualHeight - TweetTextStack.ActualHeight;
            LinkWebView.Visibility = Visibility.Collapsed;
        }

        void ShowStatistics(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
            ReportBrowser.Visibility = Visibility.Collapsed;
            TwitterBrowser.Visibility = Visibility.Collapsed;
            StatisticsBrowser.Visibility = Visibility.Visible;
        }

        

        private void WebView_GotFocus(object sender, RoutedEventArgs e)
        {
            TweetsList.Focus(FocusState.Pointer); 
        }

        void LinkWebViewLoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            LinkWebView.Visibility = Visibility.Visible;
            LinkProgressRing.Visibility = Visibility.Collapsed;
            LinkProgressRing.IsActive = false;
        }

        private void LoadStatistics()
        {
            if (viewModel.ReportsList.Count < 1 || viewModel.QueriedList.Count < 2)
            {
                NoStatisticsInitializeControls();
                return;
            }

            DateRange.Text = string.Format("{0}  -  {1}", viewModel.QueriedList[viewModel.QueriedList.Count - 1].MeasuredDate.ToString("yyyy.MM.dd"),
                                                          viewModel.QueriedList[0].MeasuredDate.ToString("yyyy.MM.dd"));

            double maxTempC = double.MinValue, minTempC = double.MaxValue, avgTempC = 0;
            double maxTempF = double.MinValue, minTempF = double.MaxValue, avgTempF = 0;
            double maxPressure = double.MinValue, minPressure = double.MaxValue, avgPressure = 0;

            foreach (var report in viewModel.QueriedList)
            {
                if (report.TempretureHighC > maxTempC)
                {
                    maxTempC = report.TempretureHighC;
                }
                else if (report.TempretureLowC < minTempC)
                {
                    minTempC = report.TempretureLowC;
                }

                if (report.TempretureHighF > maxTempF)
                {
                    maxTempF = report.TempretureHighF;
                }
                else if (report.TempretureLowF < minTempF)
                {
                    minTempF = report.TempretureLowF;
                }

                avgTempC += (report.TempretureHighC + report.TempretureLowC) / 2;
                avgTempF += (report.TempretureHighF + report.TempretureLowF) / 2;

                if (report.PressureHPA > maxPressure)
                {
                    maxPressure = report.PressureHPA;
                }
                else if (report.PressureHPA < minPressure && report.PressureHPA > 3)
                {
                    
                    minPressure = report.PressureHPA;
                }

                avgPressure += report.PressureHPA;
            }

            avgTempC /= viewModel.QueriedList.Count;
            avgTempF /= viewModel.QueriedList.Count;
            avgPressure /= viewModel.QueriedList.Count;


            MaxTempC.Text = string.Format("{0} °C", maxTempC);
            MinTempC.Text = string.Format("{0} °C", minTempC);
            AvgTempC.Text = string.Format("{0:F2} °C", avgTempC);

            MaxTempF.Text = string.Format("{0} °F", maxTempF);
            MinTempF.Text = string.Format("{0} °F", minTempF);
            AvgTempF.Text = string.Format("{0:F2} °F", avgTempF);

            MaxPressureBlock.Text = string.Format("{0} hPa", maxPressure);
            MinPressureBlock.Text = string.Format("{0} hPa", minPressure);
            AvgPressureBlock.Text = string.Format("{0:F2} hPa", avgPressure);
            
        }

        private void NoStatisticsInitializeControls()
        {
            DateRange.Text = "Unable to get statistics";
            MaxTempC.Text = "";
            MinTempC.Text = "";
            AvgTempC.Text = "";

            MaxTempF.Text = "";
            MinTempF.Text = "";
            AvgTempF.Text = "";

            MaxPressureBlock.Text = "";
            MinPressureBlock.Text = "";
            AvgPressureBlock.Text = "";
        }

        private void LoadTwitterBrowser()
        {
            if (viewModel.SelectedItemIndex == -1)
            {
                return;
            }
            AvatarWebView.Source = new Uri(viewModel.TweetList[viewModel.SelectedItemIndex].GetAvatarUrl);
            TwitterName.Text = viewModel.TweetList[viewModel.SelectedItemIndex].Publisher;
            TwitterDatePublished.Text = viewModel.TweetList[viewModel.SelectedItemIndex].GetDateString;
            TwitterContent.Text = viewModel.TweetList[viewModel.SelectedItemIndex].Content;
            viewModel.LinksFromTweets.Clear();
            //viewModel.ClearLinksFromTweets();


            string[] words = TwitterContent.Text.Split();
            foreach (var item in words)
            {
                if (item.StartsWith("http://"))
                {
                    viewModel.LinksFromTweets.Add(item);
                }
            }
        }

        private void LoadReportBrowser()
        {
            if (ReportsList.SelectedIndex == -1)
            {
                ReportBrowser.Visibility = Visibility.Collapsed;
                return;
            }
            SolarDayBlock.Text = viewModel.QueriedList[viewModel.SelectedItemIndex].Sol.ToString();
            DateMeasuredBlock.Text = string.Format("{0}{1}{2}", "(", viewModel.QueriedList[viewModel.SelectedItemIndex].FullDateString, ")");

            MaxTempNumberC.Text = viewModel.QueriedList[viewModel.SelectedItemIndex].TempretureHighC.ToString() + "°C ";
            MinTempNumberC.Text = viewModel.QueriedList[viewModel.SelectedItemIndex].TempretureLowC.ToString() + "°C ";
            double tempreture = viewModel.QueriedList[viewModel.SelectedItemIndex].TempretureHighC * 10;
            if (tempreture > 0)
            {
                MaxTempRectC.Width = tempreture;
            }
            else
            {
                MaxTempRectC.Width = 0;
            }
            MinTempRectC.Width = Math.Abs(viewModel.QueriedList[viewModel.SelectedItemIndex].TempretureLowC * 10);


            MaxTempNumberF.Text = viewModel.QueriedList[viewModel.SelectedItemIndex].TempretureHighF.ToString() + "°F ";
            MinTempNumberF.Text = viewModel.QueriedList[viewModel.SelectedItemIndex].TempretureLowF.ToString() + "°F ";
            tempreture = viewModel.QueriedList[viewModel.SelectedItemIndex].TempretureHighF * 10;
            if (tempreture > 0)
            {
                MaxTempRectF.Width = tempreture;
            }
            else
            {
                MaxTempRectF.Width = 0;
            }
            MinTempRectF.Width = Math.Abs(viewModel.QueriedList[viewModel.SelectedItemIndex].TempretureLowF * 10);

            hPaTriangle.Points[1] = new Point(90, 100 - viewModel.QueriedList[viewModel.SelectedItemIndex].PressureHPA * 10);
            hPaValue.Text = string.Format("{0} hPa", viewModel.QueriedList[viewModel.SelectedItemIndex].PressureHPA.ToString());

            WeatherTextBlock.Text = viewModel.QueriedList[viewModel.SelectedItemIndex].Weather;
            WindTextBlock.Text = viewModel.QueriedList[viewModel.SelectedItemIndex].DirectionAndSpeed;
        }

    }
}
