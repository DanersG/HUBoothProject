using OralHistoryRecorder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection;
using Windows.System;
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
    public sealed partial class AdminPageLogin : Page
    {
        private List<UserTemplate> users;
        public AdminPageLogin()
        {
            users = new List<UserTemplate>();
            this.InitializeComponent();
            users.Add(new UserTemplate { username = "klaing", password = "H12345678" });
            users.Add(new UserTemplate { username = "test", password = "test" });
            users.Add(new UserTemplate { username = "test", password = "test" });
        }


        private void SendEmail(string emailTo, string emailFrom, string emailSubject, string emailBody)
        {

        }

        private async void yesButton_Click(object sender, RoutedEventArgs e)
        {
            // create code and send email to admin
            Random rnd = new Random();
            int code = rnd.Next(1000,9999);
            // wait for admin to enter code

            var resetCodeDialogBox = new ContentDialog()
            {
                Content = new TextBox()
                {
                    PlaceholderText = "Reset code"
                },
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "OK",
            };

            await resetCodeDialogBox.ShowAsync();

            // if entered code is equal to reset code
            // prompt admin to change password

            // need new authentication code from SMTP to work
        }

      

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            var tempUser = new UserTemplate { username = userNameTextBox.Text, password = adminPasswordBox.Password };

            var isUser = users.Where(user => (user.username == tempUser.username && user.password == tempUser.password));

            if (isUser.Any())
            {
                this.Frame.Navigate(typeof(AdminPage));
            }

            else
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Invalid credentials",
                    Content = "The username or password entered is incorrect!",
                    CloseButtonText = "OK"

                };

                // Show the error prompt page to the user
                await errorDialog.ShowAsync();

            }
        }
    }
}