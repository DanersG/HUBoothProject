using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Radios;
using Windows.UI.Xaml.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.Media.Playback;
using Windows.Media.Core;

// Built with the help of this article: https://learn.microsoft.com/en-us/archive/msdn-magazine/2016/june/modern-apps-playing-with-audio-in-the-uwp
namespace OralHistoryRecorder.ViewModels
{
    public class AudioRecorderLib
    {
        private MediaCapture _mediaCapture;
        private InMemoryRandomAccessStream _memoryBuffer;
        public bool IsRecording { get; set; }
        private string DEFAULT_AUDIO_FILENAME = "NewRecording.mp3";
        public string audioFileName { get; set; }
        private string _fileName { get; set; }

        private MediaPlayer playbackMediaElement = new MediaPlayer();

        public TimeSpan AudioTimePosition
        {
            set
            {
                playbackMediaElement.PlaybackSession.Position = value;
            }
            get
            {
                return playbackMediaElement.PlaybackSession.Position;
            }
        }


        public async Task Record()
        {
            if (IsRecording)
            {
                //throw new InvalidOperationException("Recording already in progress!");
                Debug.Write("Button pressed when recording");
            }
            //await Initialize();
            //await DeleteExistingFile();
            MediaCaptureInitializationSettings settings =
            new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio
            };

            _mediaCapture = new MediaCapture();
            _memoryBuffer = new InMemoryRandomAccessStream();
            await _mediaCapture.InitializeAsync(settings);
            await _mediaCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), _memoryBuffer);
            IsRecording = true;
        }

        public async Task StopRecording()
        {
            await _mediaCapture.StopRecordAsync();
            IsRecording = false;
            SaveAudioToFile();
        }
        public async Task PauseRecording()
        {
            await _mediaCapture.PauseRecordAsync(Windows.Media.Devices.MediaCapturePauseBehavior.RetainHardwareResources);
            IsRecording = false;
        }
        public async Task ResumeRecording()
        {
            await _mediaCapture.ResumeRecordAsync();
            IsRecording = true;
        }
        private async void SaveAudioToFile()
        {
            IRandomAccessStream audioStream = _memoryBuffer.CloneStream();
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = await storageFolder.CreateFileAsync(
            String.IsNullOrEmpty(audioFileName) ? DEFAULT_AUDIO_FILENAME : audioFileName, CreationCollisionOption.GenerateUniqueName);
            _fileName = storageFile.Name;
            using (IRandomAccessStream fileStream =
            await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await RandomAccessStream.CopyAndCloseAsync(
                audioStream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                await audioStream.FlushAsync();
                audioStream.Dispose();
            }
        }

        public async void RemoveAudioFile()
        {
            StorageFolder audioDir = ApplicationData.Current.LocalFolder;
            StorageFile audioFile = await audioDir.GetFileAsync(_fileName);
            await audioFile.DeleteAsync();
        }

        public void Play()
        {
            MediaElement playbackMediaElement = new MediaElement();
            playbackMediaElement.SetSource(_memoryBuffer, "MP3");
            playbackMediaElement.Play();
        }


        public async Task PlayFromDisk(CoreDispatcher dispatcher)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                //MediaElement playbackMediaElement = new MediaElement();
                playbackMediaElement = new MediaPlayer();
                //StorageFolder storageFolder = Package.Current.InstalledLocation;
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile storageFile = await storageFolder.GetFileAsync(_fileName);
                IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read);
                playbackMediaElement.Source = MediaSource.CreateFromStorageFile(storageFile);
                //playbackMediaElement.SetSource(stream, storageFile.FileType);
                playbackMediaElement.Play();
            });
        }

        public void StopPlaying()
        {
            playbackMediaElement.Pause();
        }
    }
}