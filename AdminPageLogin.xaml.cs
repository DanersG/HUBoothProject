using OralHistoryRecorder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Net.Mail;
using System.Runtime.CompilerServices;
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
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace OralHistoryRecorder
{
    public sealed partial class AdminPageLogin : Page
    {
        #region Cason's Code

        // -----------------------------------------
        //      HU HISTORY BOOTH GMAIL ACCOUNT   
        // -----------------------------------------
        // username: hu.history.booth@gmail.com    
        // password: HistoryBoothHU!               
        // -----------------------------------------

        // admin email for password reset
        private string adminEmail = "ckirschner@harding.edu";
        private string adminPassword;

        public AdminPageLogin()
        {
            this.InitializeComponent();

            // if custom password exists...
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("AdminPassword"))
            {
               // retrieve stored password
               adminPassword = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["AdminPassword"];
            }
            else 
            {
                // otherwise set to default password
                adminPassword = "password";
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (adminPasswordBox.Password == adminPassword)
            {
                this.Frame.Navigate(typeof(AdminPage));
            }
            else
            {
                displayWrongPasswordDialog();
            }
        }

        private void passwordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                loginButton_Click(sender, e);
            }
        }

        private void btnGoHome_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
           
        }
        private async void displayWrongPasswordDialog()
        {
            ContentDialog dialogBox = new ContentDialog();
            dialogBox.Title = "Incorrect Password";
            dialogBox.Content = "The password you entered does not match our records.\n" +
                "Please try again or click 'Forgot Password' to reset.";

            dialogBox.IsPrimaryButtonEnabled = true;
            dialogBox.PrimaryButtonText = "OK";

            await dialogBox.ShowAsync();
        }

        private void SendEmail(string emailTo, string emailSubject, string emailBody)
        {
            // SMTP Guide Used --> https://mailtrap.io/blog/csharp-send-email-gmail/
            string emailFrom = "hu.history.booth@gmail.com";

            // this is an app specific SMTP password
            string emailFromPassword = "vakkcvucemwnoukp";
            // the actual login password located at the top of this file

            #region Setup+SendEmail

            var email = new MimeMessage();

            email.From.Add(new MailboxAddress("HU History Booth", emailFrom));
            email.To.Add(new MailboxAddress("Admin", emailTo));

            email.Subject = emailSubject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = emailBody
            };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, false);

                // Note: only needed if the SMTP server requires authentication
                smtp.Authenticate(emailFrom, emailFromPassword);

                smtp.Send(email);
                smtp.Disconnect(true);
            }

            #endregion
        }

        private async void forgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            #region Generate+SendResetCode

            // generate random 4 digit number for reset code
            Random random = new Random();
            int resetCode = random.Next(1000, 9999);

            // send reset code to admins email            
            string emailSubject = "Password Reset Code";
            string emailBody = ("Your password reset code for the HU History Booth is: [" + resetCode + "]");
            SendEmail(adminEmail, emailSubject, emailBody);

            #endregion

            #region PromptUserForResetCode

            var textBlock = new TextBlock()
            {
                Text = $"A new reset code has been sent to the admin's email: {adminEmail} \nCheck your spam folder if not found.\n\nPlease enter the code below:",
                Margin = new Thickness(5, 5, 5, 5)
            };

            var textBox = new TextBox()
            {
                Height = 33,
                Width = 90,
                MaxLength = 4,
                Margin = new Thickness(5, 15, 5, 0)
            };

            var stackPanel = new StackPanel()
            {
                Children = { textBlock, textBox }
            };

            ContentDialog dialogBox = new ContentDialog()
            {
                Title = "Forgot Password",
                Content = stackPanel,
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel"
            };
            var result = await dialogBox.ShowAsync();

            #endregion

            #region CaptureUserCode

            int userCode = -1;

            // if text box is not empty
            if (textBox.Text != "" && result == ContentDialogResult.Primary)
            {
                // check to make sure user has only entered digits
                bool validInput = true;
                for (int i = 0; i < textBox.Text.Length; i++)
                {
                    if (!char.IsDigit(textBox.Text[i]))
                    {
                        validInput = false;
                    }
                }

                // capture code that user entered
                if (validInput)
                {
                    userCode = Int32.Parse(textBox.Text);
                }
                else
                {
                    // if userCode remains -1, it will not display incorrect code dialog box
                    // so set userCode to an invalid number so dialog box displays for user
                    userCode = 0000;
                }
            }

            #endregion

            #region HandlePasswordReset

            if (userCode == resetCode)
            {
                // enforce one-time use of the code
                resetCode = 0;

                // prompt user for new admin password
                dialogBox.Title = "Admin Password Reset";

                var passwordBox = new PasswordBox()
                {
                    PlaceholderText = "Password...",
                    MaxLength = 25,
                    Height = 33
                };

                dialogBox.Content = passwordBox;
                await dialogBox.ShowAsync();

                if (passwordBox.Password != "")
                {
                    // save admin password
                    adminPassword = passwordBox.Password;
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["AdminPassword"] = adminPassword;

                    emailSubject = "Admin Password Updated!";
                    emailBody = "Your administrative password for the HU History Booth has just been reset to: \n\n\t\t" + adminPassword;
                    SendEmail(adminEmail, emailSubject, emailBody);
                }
            }
            else if (userCode != -1)
            {
                dialogBox.Title = "Invalid Reset Code";
                dialogBox.Content = "Failed to reset admin password.\nPlease try again.";
                dialogBox.IsSecondaryButtonEnabled = false;
                dialogBox.SecondaryButtonText = "";
                await dialogBox.ShowAsync();
            }

            #endregion

        }

        #endregion
    }
}