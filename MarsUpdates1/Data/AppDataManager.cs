using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;

namespace MarsUpdates.Data
{
    class AppDataManager
    {
        public static ViewModel viewModel;
        static bool CacheDeleted = false;

        public static async Task SaveToLocalCacheAsync<T>(T data, string filename)
        {
            if (CacheDeleted)
            {
                return;
            }

            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                filename, CreationCollisionOption.ReplaceExisting);

            var stream = await file.OpenStreamForWriteAsync();

            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteObject(stream, data);

            await stream.FlushAsync();
        }

        public static async Task<T> RestoreFromLocalCacheAsync<T>(string filename)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
                var instream = await file.OpenStreamForReadAsync();
                var serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(instream);
            }
            catch (Exception)
            {
                return default(T);
            };
        }

        public static async Task<bool> DeleteCacheAndSettingsAsync()
        {
            StorageFile file;
            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(AppSettingsData.reportsFileName);
                await file.DeleteAsync();
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(AppSettingsData.twitterChannelsFile);
                await file.DeleteAsync();
            }
            catch (Exception)
            {
                return false;
            }


            AppSettingsData.LatestReportID = null;
            CacheDeleted = true;
            return true;
        }

        public static async void GetNewTweets()
        {
            try
            {
                viewModel.UpdateTweetListWithoutRefresh =
                            await FeedFetch.GetTweetsAsync(AppSettingsData.TweetsToLoad);
                AppSettingsData.NewTweetsLoaded = true;
            }
            catch (Exception)
            {
                string[] saveOldTwitters = new string[AppSettingsData.TweetsToLoad.Length - 1];
                for (int i = 0; i < saveOldTwitters.Length; i++)
                {
                    saveOldTwitters[i] = AppSettingsData.TweetsToLoad[i];
                }
                AppSettingsData.SaveTweets(saveOldTwitters);

            }
            //await frame.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            //    async () =>
            //    {
                    
            //    });
        }
    }
}
