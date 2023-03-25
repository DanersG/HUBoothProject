using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using OralHistoryRecorder.Models;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Media.Playback;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Text.RegularExpressions;
using Windows.UI.Popups;
using Windows.System;
using OralHistoryRecorder.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OralHistoryRecorder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminPage : Page
    {
        private ObservableCollection<StudentRecording> studentRecordingList = new ObservableCollection<StudentRecording>();

        AudioRecorderLib audioRecorder;

        private DispatcherTimer dispatcherTimer, demoDispatcher, audioPlayingTimer;
        private DateTime startedTime, stopTime;
        private TimeSpan timePassed, curRecDuration;
        bool isStop = false;
        bool isPaused = false;
        bool isPlaying = false;
        int currIndex = 0;
        StudentRecording student;
        private MediaPlayer playbackMediaElement;

        private string currRecSelected;

        public AdminPage()
        {
            this.InitializeComponent();

            playbackMediaElement= new MediaPlayer();

            currRecSelected = "";

            InitializeAsync();

            audioPlayingTimer = new DispatcherTimer();
            audioPlayingTimer.Tick += AudioPlayingTimer_Tick;
            audioPlayingTimer.Interval = TimeSpan.FromSeconds(1);

            PlaybackSlider.IsEnabled = false;
            btnPlay.IsEnabled = false;
            CurrentPositionTextBlock.IsTapEnabled = false;
            btnRemoveRecording.IsEnabled = false;
        }

        private void AudioPlayingTimer_Tick(object sender, object e)
        {
            if (playbackMediaElement.PlaybackSession.Position != TimeSpan.Zero)
            {
                PlaybackSlider.Value = playbackMediaElement.PlaybackSession.Position.TotalSeconds;
                CurrentPositionTextBlock.Text = $"{MakeDigitString(playbackMediaElement.PlaybackSession.Position.Minutes, 2)}:{MakeDigitString(playbackMediaElement.PlaybackSession.Position.Seconds, 2)}";
            }
        }

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


        private void PlaybackSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            playbackMediaElement.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
            //CurrentPositionTextBlock.Text = CurrentPositionTextBlock.Text = $"{MakeDigitString(0, 2)}:{MakeDigitString(Convert.ToInt32(e.NewValue), 2)}";

        }


        private void recordingsDisplayer_SelectionChanged(object sender, TappedRoutedEventArgs e)
        {
            //  Enabling audio player buttons:
            PlaybackSlider.IsEnabled = true;
            btnPlay.IsEnabled = true;
            CurrentPositionTextBlock.IsTapEnabled = true;
            btnRemoveRecording.IsEnabled = true;

            //  Setting current selected item:
            currRecSelected = ((sender as GridView).SelectedItem as StudentRecording).Title;
            curRecDuration = ((sender as GridView).SelectedItem as StudentRecording).duration;
            currIndex = (sender as GridView).SelectedIndex;

            PlaybackSlider.Maximum = curRecDuration.TotalSeconds;

            //  If the selection has changed the audio must be stopped:
            if (isPlaying != false)
            {
                PlayText.Text = "Play";
                PlayIcon.Symbol = Symbol.Play;
                isPlaying = false;
                audioPlayingTimer.Stop();
                //audioRecorder.StopPlaying();
                playbackMediaElement.Pause();

                //  RESETTING CONTROLS
                playbackMediaElement.PlaybackSession.Position = TimeSpan.Zero;
                PlaybackSlider.Value = playbackMediaElement.PlaybackSession.Position.TotalSeconds;
                CurrentPositionTextBlock.Text = $"{MakeDigitString(0, 2)}:{MakeDigitString(0, 2)}";
            }



        }

        private void AddRange(ObservableCollection<StudentRecording> collection, List<StudentRecording> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        private async void btnRemoveRecording_Click(object sender, RoutedEventArgs e)
        {

            var messageDialog = new MessageDialog($"Are you sure you want to remove {currRecSelected}?");

            // Add commands and set their callbacks
            messageDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler(this.CommandInvokedHandler)));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();

        }

        private async void CommandInvokedHandler(IUICommand command)
        {
            // Check which command was invoked
            if (command.Label == "Yes")
            {
                // Remove the file
                // file removal logic here:
                StorageFolder audioDir = ApplicationData.Current.LocalFolder;
                StorageFile audioFile = await audioDir.GetFileAsync(currRecSelected + ".mp3");
                await audioFile.DeleteAsync();

                // Remove recording from list

                studentRecordingList.RemoveAt(currIndex);

                // Show a message that the file was successfully removed
                var messageDialog = new MessageDialog($"{currRecSelected} was successfully removed.");
                await messageDialog.ShowAsync();
                btnRemoveRecording.IsEnabled = false;
                btnPlay.IsEnabled = false;
                recordingsDisplayer.SelectedIndex= 0;
            }
            else
            {
                // Do nothing
            }
        }

        private async Task InitializeAsync()
        {
            var tempList = await RecordingManager.retrieveRecordings();
            AddRange(studentRecordingList, tempList);
        }


        private async Task playRecording(CoreDispatcher dispatcher)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                //MediaElement playbackMediaElement = new MediaElement();
                playbackMediaElement = new MediaPlayer();
                //StorageFolder storageFolder = Package.Current.InstalledLocation;
                var storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile storageFile = await storageFolder.GetFileAsync(currRecSelected + ".mp3");
                IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read);
                playbackMediaElement.Source = MediaSource.CreateFromStorageFile(storageFile);
                //playbackMediaElement.SetSource(stream, storageFile.FileType);
                playbackMediaElement.Play();
            });
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
                await playRecording(Dispatcher);
            }
            else
            {
                // Stop audio
                PlayText.Text = "Play";
                PlayIcon.Symbol = Symbol.Play;
                isPlaying = false;
                //audioRecorder.StopPlaying();
                audioPlayingTimer.Stop();
                playbackMediaElement.PlaybackSession.Position = TimeSpan.Zero;
                CurrentPositionTextBlock.Text = CurrentPositionTextBlock.Text = $"{MakeDigitString(0, 2)}:{MakeDigitString(0, 2)}";
                PlaybackSlider.Value = playbackMediaElement.PlaybackSession.Position.TotalSeconds;
                playbackMediaElement.Pause();
            }
        }

        private void PlayText_SelectionChanged(object sender, RoutedEventArgs e)
        {
            btnPlay.IsEnabled= false;
        }
    }
}
