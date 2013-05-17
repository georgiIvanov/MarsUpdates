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
    public sealed partial class ClearCacheFlyout : UserControl
    {
        public ClearCacheFlyout()
        {
            this.InitializeComponent();
        }

        public void Show(Page page, AppBar appbar, Button button)
        {
            ClearCachePopup.IsOpen = true;
            FlyoutHelper.ShowRelativeToAppBar(ClearCachePopup, page, appbar, button);
        }

        void NoClick(object sender, RoutedEventArgs e)
        {
            ClearCachePopup.IsOpen = false;
        }

        async void YesClick(object sender, RoutedEventArgs e)
        {
            bool success = await AppDataManager.DeleteCacheAndSettingsAsync();

            if (success)
            {
                MessageTextBlock.Text = "Cache deleted successfully";
                YesButton.Visibility = Visibility.Collapsed;
                NoButton.Content = "OK";
            }
            else
            {
                MessageTextBlock.Text = "Error deleting cache";
            }
        }
    }
}
