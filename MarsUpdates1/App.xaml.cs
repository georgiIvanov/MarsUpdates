using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using MarsUpdates.Pages;
using MarsUpdates.Data;
using System.Collections.ObjectModel;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MarsUpdates
{
    sealed partial class App : Application
    {
        ViewModel viewModel;
        Task getWeatherReportsTask;
        Task getTweetsTask;
        CancellationTokenSource getWeatherReportsSource;
        CancellationTokenSource getTweetsSource;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            AppSettingsData.AppSettingsDataInit();
            viewModel = new ViewModel();
            
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), viewModel))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();

            StartGettingFeed(rootFrame);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            StopGettingWReports();
            StopGettingTweets();
            Task.Run(new Action(AppSettingsData.SaveTweets));
            deferral.Complete();
        }

        void StartGettingFeed(Frame frame)
        {
            getWeatherReportsSource = new CancellationTokenSource();
            CancellationToken WRtoken = getWeatherReportsSource.Token;
            getTweetsSource = new CancellationTokenSource();
            CancellationToken TweetsToken = getTweetsSource.Token;
            AppDataManager.viewModel = viewModel;
            
            getWeatherReportsTask = new Task(async () =>
            {
                viewModel.LoadReportCache = await AppDataManager.RestoreFromLocalCacheAsync<ObservableCollection<WeatherReport>>("reports");
                while (!WRtoken.IsCancellationRequested)
                {
                    await frame.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        async () =>
                        {
                            viewModel.ReportsList = await FeedFetch.GetWeatherFeedAsync(AppSettingsData.GetReportQuery);
                            await AppDataManager.SaveToLocalCacheAsync<ObservableCollection<WeatherReport>>
                                    (viewModel.ReportsList, AppSettingsData.reportsFileName);
                        });
                    
                    WRtoken.WaitHandle.WaitOne(AppSettingsData.UpdatesIntervalMsec);
                }
            });

            getTweetsTask = new Task(async () =>
                {
                    while (!TweetsToken.IsCancellationRequested)
                    {
                        AppSettingsData.TweetsToLoad = await AppSettingsData.LoadTweets();
                        await frame.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            async () =>
                            {
                                viewModel.TweetList = await FeedFetch.GetTweetsAsync(AppSettingsData.TweetsToLoad);
                            });
                        TweetsToken.WaitHandle.WaitOne(AppSettingsData.UpdatesIntervalMsec);
                    }
                });


            getWeatherReportsTask.Start();
            getTweetsTask.Start();
            
        }

        

        async protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            ObservableCollection<WeatherReport> queryResult = new ObservableCollection<WeatherReport>();

            if (args.QueryText.StartsWith("sol: "))
            {
                int sol, startIndex = args.QueryText.IndexOf(' ');
                if(int.TryParse(args.QueryText.Substring(args.QueryText.IndexOf(' '), args.QueryText.Length - startIndex), out sol))
                {
                    foreach (var item in viewModel.QueriedList)
                    {
                        if (item.Sol == sol)
                        {
                            queryResult.Add(item);
                            viewModel.QueriedList = queryResult;
                            return;
                        }
                    }
                }
            }
            else if (args.QueryText.Contains(" - "))
            {
                string[] dates = args.QueryText.Split( new char[]{' ', '-',' '});
                DateTime firstDate = new DateTime();
                DateTime secondDate = new DateTime();
                if (!DateTime.TryParse(dates[0], out firstDate) || !DateTime.TryParse(dates[3], out secondDate))
                {
                    return;
                }
                if (firstDate >= secondDate)
                {
                    return;
                }

                foreach (var item in viewModel.QueriedList)
                {
                    if (item.MeasuredDate >= firstDate && item.MeasuredDate <= secondDate)
                    {
                        queryResult.Add(item);
                    }
                }
                viewModel.QueriedList = queryResult;
            }
            else if (args.QueryText == "reset")
            {
                viewModel.LoadReportInListView();
                viewModel.QueriedList.Clear();
                // display search results.
            }
        }

        void StopGettingWReports()
        {
            getWeatherReportsSource.Cancel();
        }

        void StopGettingTweets()
        {
            getTweetsSource.Cancel();
        }
    }
}
