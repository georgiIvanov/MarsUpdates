﻿using System;
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

namespace MarsUpdates.Flyouts
{
    class FlyoutHelper
    {
        public static void ShowRelativeToAppBar(Popup popup, Page page, AppBar appbar, Button button)
        {
            Func<UIElement, UIElement, Point> getOffset =
                delegate(UIElement control1, UIElement control2)
                {
                    return control1.TransformToVisual(control2)
                        .TransformPoint(new Point(0, 0));
                };

            Point popupOffset = getOffset(popup, page);
            Point buttonOffset = getOffset(button, page);
            popup.HorizontalOffset = buttonOffset.X - popupOffset.X - (popup.ActualWidth / 2) + (button.ActualWidth / 2);
            popup.VerticalOffset = getOffset(appbar, page).Y - popupOffset.Y - popup.ActualHeight;

            if (popupOffset.X + popup.HorizontalOffset + popup.ActualWidth > page.ActualWidth)
            {
                popup.HorizontalOffset = page.ActualWidth - popupOffset.X - popup.ActualWidth;
            }
            else if (popup.HorizontalOffset + popupOffset.X < 0)
            {
                popup.HorizontalOffset = -popupOffset.X;
            }
        }
    }
}
