using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

// Model definition inspired by Collin Blake - https://www.youtube.com/watch?v=sCz_lqovLRg

namespace OralHistoryRecorder.Models
{
    public class StudentRecording
    {
        public int RecId { get; set; }
        public String Title { get; set; }
        public String tag { get; set; }

        public TimeSpan duration { get; set; }
        public string decade { get; set; }
        public StudentRecording()
        {
            tag = "";
            RecId = 0;
        }
    }


    public class RecordingManager
    {
        public async static Task<List<StudentRecording>> retrieveRecordings()
        {
            var stuRecordings =  new List<StudentRecording>();

            //  Opening loca folder to read all files saved inside.
            var recFolder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path);

            var files = await recFolder.GetFilesAsync();


            int id = 0;

            //  Looping through each of the files to read all mp3s saved inside local folder
            foreach (var file in files)
            {

                var dir = ApplicationData.Current.LocalFolder.Path;


                var tempFile = TagLib.File.Create(file.Path);

                //  Assigning all properties for a student recording to be displayed in the observable
                //  collection in the administrator's view.
                stuRecordings.Add(new StudentRecording { RecId = id, 
                                                         Title = /*String.IsNullOrEmpty(tempFile.Tag.Title) ? "something" : */tempFile.Tag.Title,
                                                         tag = /*String.IsNullOrEmpty(tempFile.Tag.Comment) ? "something" : */tempFile.Tag.Comment,
                                                         duration = tempFile.Properties.Duration, 
                                                         decade = tempFile.Tag.Year.ToString() });

                id++;   
            }

            //Returning the list of mp3 files representing the students' recordings.
            return stuRecordings;
        }
    }
}
