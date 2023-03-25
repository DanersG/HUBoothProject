using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OralHistoryRecorder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AcknowledgementPage : Page
    {
        public AcknowledgementPage()
        {
            this.InitializeComponent();
        }

        private async void declineBtn_Click(object sender, RoutedEventArgs e)
        {

            ContentDialog userCreated = new ContentDialog
            {
                Title = "Agreement Error",
                Content = "To proceed you must agree to the stament presented.",
                CloseButtonText = "OK"
            };

            await userCreated.ShowAsync();

            this.Frame.Navigate(typeof(UserSelectionPage));
        }

        private void agreeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
