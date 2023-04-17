using OralHistoryRecorder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        private void yesButton_Click(object sender, RoutedEventArgs e)
        {
            createPassInst.Visibility = Visibility.Visible;
            setUpButton.Visibility = Visibility.Visible;
            setUpButton.Focus(FocusState.Programmatic);
            prevPasswordBox.Visibility = Visibility.Visible;
            newPasswordBox.Visibility = Visibility.Visible;
        }

        private void noButton_Click(object sender, RoutedEventArgs e)
        {
            adminPasswordBox.Focus(FocusState.Programmatic);

            if (createPassInst.Visibility != Visibility.Collapsed)
            {
                createPassInst.Visibility = Visibility.Collapsed;
                setUpButton.Visibility = Visibility.Collapsed;

                prevPasswordBox.Visibility = Visibility.Collapsed;
                newPasswordBox.Visibility = Visibility.Collapsed;
            }
        }

        private async void setUpButton_Click(object sender, RoutedEventArgs e)
        {
            createPassInst.Visibility = Visibility.Collapsed;
            setUpButton.Visibility = Visibility.Collapsed;


            if (userNameTextBox.Text != "" && adminPasswordBox.Password != "")
            {
                users.Add(new UserTemplate { username = userNameTextBox.Text, password = adminPasswordBox.Password });

                ContentDialog userCreated = new ContentDialog
                {
                    Title = "Successful",
                    Content = "The username and password were created succesfully",
                    CloseButtonText = "OK"
                };

                await userCreated.ShowAsync();
            }
            else
            {
                ContentDialog creationErr = new ContentDialog
                {
                    Title = "Error",
                    Content = "You must have a username or password. Try again.",
                    CloseButtonText = "OK"
                };

                await creationErr.ShowAsync();
            }


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