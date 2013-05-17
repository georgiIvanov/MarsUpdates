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
using Windows.UI.ViewManagement;
using MarsUpdates.Data;
using MarsUpdates.Flyouts;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Runtime.Serialization.Json;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MarsUpdates.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ViewModel viewModel;
        FeedsViewModel feedsViewModel;
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SetRandomImageName();
            viewModel = (ViewModel)e.Parameter;
            feedsViewModel = new FeedsViewModel();
            Task.Run(new Func<Task>(InitFeedsViewModel));

            this.DataContext = feedsViewModel;
            FeedsPage.Navigate(typeof(Feeds), e.Parameter);

        }

        async Task InitFeedsViewModel()
        {
            await feedsViewModel.LoadTwitterFeeds();
        }

        void SetRandomImageName()
        {
            Random rng = new Random();
            Uri uri = new Uri(this.BaseUri, "/Assets/" + 1 +".png"); //rng.Next(1, 5)

            if (uri.IsAbsoluteUri)
            {
                this.BackGroundImage.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(uri);
            }
        }

        private void RefreshReport(object sender, RoutedEventArgs e)
        {
            if (viewModel.LoadReportInListView())
            {
                viewModel.QueriedList.Clear();
            }
        }

        private void TweetsClick(object sender, RoutedEventArgs e)
        {
            TweetFeedsFlyout.Show(this, this.BottomAppBar, this.AddRemoveTweetsBtn, feedsViewModel);
        }

        private async void XMLExportClick(object sender, RoutedEventArgs e)
        {
            var _Picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                DefaultFileExtension = ".xml",
                SuggestedFileName = "MarsUpdates",
                SettingsIdentifier = ".xml"
            };
            _Picker.FileTypeChoices.Add("xml", new List<string> { ".xml" });

            var _File = await _Picker.PickSaveFileAsync();
            if (_File == null)
            {
                return;
            }

            StringWriter Output = new StringWriter(new StringBuilder());
            XmlSerializer xs = new XmlSerializer(viewModel.QueriedList.GetType());
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("MarsUpdates", "http://spaceappschallenge.org/challenge/wish-you-were-here/");
            xs.Serialize(Output, viewModel.QueriedList, ns);
            
            using (IRandomAccessStream fileStream = await _File.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
                {
                    using (DataWriter dataWriter = new DataWriter(outputStream))
                    {
                        dataWriter.WriteString(Output.ToString().ToString());
                        await dataWriter.StoreAsync();
                        dataWriter.DetachStream();
                    }

                    await outputStream.FlushAsync();
                }
            }
        }

        private async void JSONExportClick(object sender, RoutedEventArgs e)
        {
            var _Picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                DefaultFileExtension = ".json",
                SuggestedFileName = "MarsUpdates",
                SettingsIdentifier = ".json"
            };
            _Picker.FileTypeChoices.Add("json", new List<string> { ".json" });

            var _File = await _Picker.PickSaveFileAsync();
            if (_File == null)
            {
                return;
            }

            var serialized = JSONSerializer<ObservableCollection<WeatherReport>>.Serialize(viewModel.QueriedList);

            using (IRandomAccessStream fileStream = await _File.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
                {
                    using (DataWriter dataWriter = new DataWriter(outputStream))
                    {
                        dataWriter.WriteBytes(serialized);
                        await dataWriter.StoreAsync();
                        dataWriter.DetachStream();
                    }

                    await outputStream.FlushAsync();
                }
            }
        }

        private void ClearCacheClick(object sender, RoutedEventArgs e)
        {
            ClearCacheFlyout.Show(this, this.BottomAppBar, this.AddRemoveTweetsBtn);
        }

        static class JSONSerializer<T>
        {
            public static byte[] Serialize(T instance)
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, instance);
                    
                    return stream.ToArray();
                }
            }
        }

    }
}
