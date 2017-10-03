using System;
using System.Drawing;

using CoreGraphics;
using Foundation;
using UIKit;
using System.IO;
using System.Xml.Serialization;

namespace WiFiDronection
{
    [Register("LogsActivity")]
    public class LogsActivity : UIView
    {

        private static readonly string TAG = "LogsActivity";

        private String[] path;
        private NSString dataPath;
        private NSString documentsDirectory;
        public LogsActivity()
        {
            Initialize();
        }

        public LogsActivity(RectangleF bounds) : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Red;
            path = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, true);
        }


        public void CreateFolder()
        {
            try
            {
                NSString foldername = new NSString("MyFolder");

                documentsDirectory = new NSString(path[0]);
                dataPath = documentsDirectory.AppendPathComponent(foldername);


                NSFileManager.DefaultManager.CreateDirectory(dataPath, false, null);
            }catch(Exception ex)
            {
              
            }
        }

        public void ReadFile()
        {
            try
            {
                // Gets the direct path to the file
                NSString folderPath = dataPath.AppendPathComponent(new NSString("testfile.txt"));

                // Handle the data and read it from the specific path
                NSFileHandle nsfh = NSFileHandle.OpenRead(folderPath);

                // Gets the data from the file
                NSData data =  nsfh.ReadDataToEndOfFile();

                string text = data.ToString();

                

            }
            catch (Exception ex)
            {
                // Failed read data from a file

            }
        }

        public void WriteFile()
        {
          
            try
            {
                // Gets the direct path to the file
                NSString folderPath = dataPath.AppendPathComponent(new NSString("testfile.txt"));

                // The text who should be written
                NSString someText = new NSString("Test");

                // The data
                NSData data = someText.Encode(NSStringEncoding.UTF8);

                // Handle the data and writes it to the specific path
                NSFileHandle nsfh = NSFileHandle.OpenWrite(folderPath);

                // Writes the data
                nsfh.WriteData(data);


            }
            catch(Exception ex)
            {
                // Failed write data into a file

            }
        }


        public void TestWrite()
        {
            // Get all directories
            var directories = Directory.EnumerateDirectories("./");
            foreach (var directory in directories)
            {
                Console.WriteLine(directory);
            }

            // Reading files
            var text = File.ReadAllText("TestData/ReadMe.txt");
            Console.WriteLine(text);


            // Creating Files
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
           
            var filename = Path.Combine(documents, "Write.txt");
            File.WriteAllText(filename, "Write this text into a file");

            // Creating a directory
            var documents2 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var directoryname = Path.Combine(documents, "NewDirectory");

            

        }
    }
}