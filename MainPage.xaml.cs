using OralHistoryRecorder.Models;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TagLib.Ogg;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using OralHistoryRecorder.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OralHistoryRecorder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        AudioRecorderLib audioRecorder;

        // Slider playback implemented based on:
        // https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/play-audio-and-video-with-mediaplayer
        // https://stackoverflow.com/questions/45001470/uwp-slider-wont-move-during-playback
        private DispatcherTimer audioRecordingTimer, audioPlayingTimer, audioTextBoxTimer;

        // Timer implemented based on: https://stackoverflow.com/questions/42763850/c-sharp-countdown-timer-pause
        private DateTime startedTime, stopTime;

        private TimeSpan timePassed, timeSinceLastStop;
        bool isStop = false;
        bool isPaused = false;
        bool isPlaying = false;
        StudentRecording student;
        private TimeSpan RECORDING_MINUTES_LIMIT = TimeSpan.FromMinutes(1);    

        //Function to set the interval for TextBox Blinking message while audio is stopped
        private void Timer_Tick(object sender, object e)
        {
            if (RecordingMessageBox.Text == "")
            {
                RecordingMessageBox.Text = "Paused";
            }
            else
            {
                RecordingMessageBox.Text = "";
            }
        }

        public MainPage()
        {
            InitializeComponent();
            audioRecorder = new AudioRecorderLib();
            student = new StudentRecording();

            RestoreToDefault();

            audioPlayingTimer = new DispatcherTimer();
            audioPlayingTimer.Tick += AudioPlayingTimer_Tick;
            audioPlayingTimer.Interval = TimeSpan.FromSeconds(1);
        }

        private void RestoreToDefault()
        {
            // Restore controllers configuration to default, except name.
            enteredCustomTag.Text = String.Empty;
            btnStartRecording.IsEnabled = false;
            btnPauseRecording.IsEnabled = false;
            ChapelTag.IsChecked = false;
            DormTag.IsChecked = false;
            ClubTag.IsChecked = false;
            PlaybackSlider.IsEnabled = false;
            btnRemoveRecording.IsEnabled = false;
            btnPlay.IsEnabled = false;
            btnEnterTag.IsEnabled = false;
            timeText.Text = RECORDING_MINUTES_LIMIT.ToString();
            PlaybackSlider.Value = 0;
            RecordingMessageBox.Text = "Click the button to start recording";
            RecordingAlertMessageBox.Text = "Must not exceed 10 minutes";
            CurrentPositionTextBlock.Text = "00:00";
            PlayText.Text = "Play";
            PlayIcon.Symbol = Symbol.Play;
        }

        private void AudioPlayingTimer_Tick(object sender, object e)
        {
            if (audioRecorder.AudioTimePosition != TimeSpan.Zero)
            {
                PlaybackSlider.Value = audioRecorder.AudioTimePosition.TotalSeconds;
                CurrentPositionTextBlock.Text = $"{MakeDigitString(audioRecorder.AudioTimePosition.Minutes, 2)}:{MakeDigitString(audioRecorder.AudioTimePosition.Seconds, 2)}";
            }

        }

        private async void btnPauseRecording_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused == false)
            {
                // Pause
                isPaused = true;
                audioRecordingTimer.Stop();
                RecordingMessageBox.Text = "Paused";
                PauseText.Text = "Resume";
                PauseIcon.Symbol = Symbol.RepeatAll;
                stopTime = DateTime.Now;
                await audioRecorder.PauseRecording();

                // Set up the DispatcherTimer to make the TextBlock blink
                audioTextBoxTimer = new DispatcherTimer();
                audioTextBoxTimer.Interval = TimeSpan.FromSeconds(1);
                audioTextBoxTimer.Tick += Timer_Tick;
                audioTextBoxTimer.Start();
            }
            else
            {
                // Resume
                isPaused = false;
                audioRecordingTimer.Start();
                RecordingMessageBox.Text = "Recording...";
                startedTime += (DateTime.Now - stopTime);
                PauseText.Text = "Pause";
                PauseIcon.Symbol = Symbol.Pause;
                await audioRecorder.ResumeRecording();

                // Stop the blinking on RecordingMessageBox
                if (audioTextBoxTimer != null)
                {
                    audioTextBoxTimer.Stop();
                    audioTextBoxTimer = null;
                    RecordingMessageBox.Visibility = Visibility.Visible; // TextBlock is visible when the blinking stops
                }
            }
        }

        private async void btnStartRecording_Click(object sender, RoutedEventArgs e)
        {
            if (isStop == false)
            {
                // Start recording
                isStop = true;

                // Needs fix: timer starts on button press but should start after microphone confirmation is accepted.
                startedTime = DateTime.Now;
                DispatcherTimerSetup();

                RecordingMessageBox.Text = "Recording...";
                RecordingIcon.Symbol = Symbol.Stop;
                RecordingText.Text = "Stop";
                btnPauseRecording.IsEnabled = true;
                await audioRecorder.Record();
            }

            else
            {  
                // Stop recording

                if (isStop == true)
                {
                    // Stop the blinking on RecordingMessageBox
                    if (audioTextBoxTimer != null)
                    {
                        audioTextBoxTimer.Stop();
                        audioTextBoxTimer = null;
                        RecordingMessageBox.Visibility = Visibility.Visible; // TextBlock is visible when the blinking stops
                    }
                }
                isStop = false;
                audioRecordingTimer.Stop();
                RecordingMessageBox.Text = "Recording Stopped.";
                RecordingAlertMessageBox.Text = "Submit to save your recording or Remove to start over.";
                timeText.Text = RECORDING_MINUTES_LIMIT.ToString();
                RecordingIcon.Symbol = Symbol.Microphone;
                RecordingText.Text = "Start";

                PauseIcon.Symbol = Symbol.Pause;
                PauseText.Text = "Pause";

                // Enable audio playing and removing controllers
                btnStartRecording.IsEnabled = false;
                btnRemoveRecording.IsEnabled = true;
                PlaybackSlider.IsEnabled = true;
                btnEnterTag.IsEnabled = true;
                btnPlay.IsEnabled = true;
                btnPauseRecording.IsEnabled = false;
                btnEnterTag.IsEnabled = true;
                PlaybackSlider.Maximum = timePassed.TotalSeconds;

                ++student.RecId;
                audioRecorder.audioFileName = nameTextBox.Text + student.RecId + ".mp3";

                await audioRecorder.StopRecording();
            }
        }

        private async void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying == false)
            {
                // Play Audio
                PlayText.Text = "Stop";
                PlayIcon.Symbol = Symbol.Stop;
                isPlaying = true;
                audioPlayingTimer.Start();
                await audioRecorder.PlayFromDisk(Dispatcher);
            }
            else
            {
                // Stop audio
                PlayText.Text = "Play";
                PlayIcon.Symbol = Symbol.Play;
                isPlaying = false;
                audioPlayingTimer.Stop();
                audioRecorder.AudioTimePosition = TimeSpan.Zero;
                PlaybackSlider.Value = 0;
                CurrentPositionTextBlock.Text = "00:00";
                audioRecorder.StopPlaying();
            }
        }

        private void DispatcherTimerSetup()
        {
            timeSinceLastStop = RECORDING_MINUTES_LIMIT;
            timeText.Text = RECORDING_MINUTES_LIMIT.ToString();
            audioRecordingTimer = new DispatcherTimer();
            audioRecordingTimer.Tick += DemoDispatcher_Tick;
            audioRecordingTimer.Start();
        }

        private async void btnEnterTag_Click(object sender, RoutedEventArgs e)
        {
            // Create TagLib instance
            var dir = ApplicationData.Current.LocalFolder.Path;
            var tfile = TagLib.File.Create(dir + "\\" + audioRecorder.audioFileName);

            // Add title based on the name provided
            tfile.Tag.Title = audioRecorder.audioFileName.Replace(".mp3", "");

            // Add the fact that the person is/was a Harding Student in Subtitle 
            tfile.Tag.Subtitle = (bool)HardingStudentCheck.IsChecked ? "Harding Student" : "Not a Harding Student";

            ComboBoxItem selectedItem = decadeComboBox.SelectedItem as ComboBoxItem;
            string selectedOption = selectedItem.Content.ToString();
            // Add decade entered
            tfile.Tag.Year = UInt32.Parse(selectedOption);

            // Add selected tags
            tfile.Tag.Comment = (bool)ChapelTag.IsChecked ? "Chapel," : "";
            tfile.Tag.Comment += (bool)DormTag.IsChecked ? "Dorm," : "";
            tfile.Tag.Comment += (bool)ClubTag.IsChecked ? "Club," : "";
            tfile.Tag.Comment += String.IsNullOrEmpty(enteredCustomTag.Text) ? "" : (enteredCustomTag.Text + ",");


            //Restore to default
            RestoreToDefault();
            btnStartRecording.IsEnabled = true;

            // Save files with the Title, Year, and Comment (tags) modified
            tfile.Save();

            ContentDialog successfulSubmissionDialog = new ContentDialog
            {
                Title = "Successful Submission",
                Content = "Thank you for submitting your story. It was successfully submitted.",
                CloseButtonText = "Ok",
            };
            await successfulSubmissionDialog.ShowAsync();
            this.Frame.Navigate(typeof(UserSelectionPage));
        }

        // While it works, it is not being used at the moment. However, is being kept for future development.
        private void Tag_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            string tag = toggleButton.Name.Replace("Tag", "");
            if (!student.tag.Contains(tag))
            {
                student.tag += toggleButton.Name.Replace("Tag", "") + ',';
            }
        }

        // While it works, it is not being used at the moment. However, is being kept for future development.
        private void Tag_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            string tag = toggleButton.Name.Replace("Tag", "");
            if (student.tag.Contains(tag))
            {
                student.tag = student.tag.Replace(tag + ',', "");
            }
        }

        // While it works, it is not being used at the moment. However, is being kept for future development.
        private void decadeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = decadeComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string selectedOption = selectedItem.Content.ToString();
                // Do something with the selected option...
            }
        }

        private async void CommandInvokedHandler(IUICommand command)
        {
            // Check which command was invoked
            if (command.Label == "Yes")
            {
                // Remove the file
                audioRecorder.RemoveAudioFile();

                // Show a message indicating that the file with the provided name has been successfully removed
                ContentDialog removedFileDialog = new ContentDialog
                {
                    Title = $"Removed recording",
                    Content = $"Your recording has been removed.",
                    CloseButtonText = "Ok",
                };
                await removedFileDialog.ShowAsync();

                btnRemoveRecording.IsEnabled = false;
                btnPlay.IsEnabled = false;
                PauseText.Text = "Pause";
                PauseIcon.Symbol = Symbol.Pause;
            }
            else
            {
                // Do nothing
            }
        }

        private async void btnRemoveRecording_Click(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog($"Are you sure you want to delete your recording?");

            // Add commands and set their callbacks
            messageDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler(this.CommandInvokedHandler)));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();

            // Disable Submit button
            btnEnterTag.IsEnabled = false;

            // Display Initial Recording Controllers if user's name was entered
            nameTextBox_TextChanged(nameTextBox, null);
        }

        private void PlaybackSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            audioRecorder.AudioTimePosition = TimeSpan.FromSeconds(e.NewValue);
        }

        private void HardingStudentCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (HardingStudentCheckIcon != null)
            {
                CheckedText.Text = "Yes";
                HardingStudentCheckIcon.Symbol = Symbol.Accept;
            }
        }

        private void HardingStudentCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (HardingStudentCheckIcon != null)
            {
                CheckedText.Text = "No";
                HardingStudentCheckIcon.Symbol = Symbol.Cancel;
            }
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(nameTextBox.Text))
            {
                RestoreToDefault();
            }
            else
            {
                RestoreToDefault();
                btnStartRecording.IsEnabled = true;
                btnPauseRecording.IsEnabled = false;
                student.Title = nameTextBox.Text + student.RecId;
                audioRecorder.audioFileName = nameTextBox.Text + student.RecId + ".mp3";
            } 
        }

        // Taken from: https://www.youtube.com/watch?v=dKhcS-ec-p8
        private string MakeDigitString(int number, int count)
        {
            string result = "0";
            if (count == 2)
            {
                if (number < 10)
                {
                    result = "0" + number;
                }
                else
                {
                    result = number.ToString();
                }
            }
            else if (count == 3)
            {
                if (number < 10)
                    result = "00" + number;
                else if (number > 9 && number < 100)
                    result = "0" + number;
                else
                    result = number.ToString();
            }
            return result;
        }

        private async void DemoDispatcher_Tick(object sender, object e)
        {
            timePassed = DateTime.Now - startedTime;
            if (timePassed != RECORDING_MINUTES_LIMIT)
            {
                timeText.Text = MakeDigitString((timeSinceLastStop - timePassed).Hours, 2) + ":"
                    + MakeDigitString((timeSinceLastStop - timePassed).Minutes, 2) + ":"
                    + MakeDigitString((timeSinceLastStop - timePassed).Seconds, 2);
                    // Milliseconds
                    //+ ":"
                    //+ MakeDigitString((timeSinceLastStop - timePassed).Milliseconds, 3);
            }
            else
            {
                await audioRecorder.StopRecording();
            }
        }
    }
}
