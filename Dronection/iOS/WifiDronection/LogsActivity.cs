using System;
using System.Drawing;

using CoreGraphics;
using Foundation;
using UIKit;

namespace WiFiDronection
{
    [Register("LogsActivity")]
    public class LogsActivity : UIView
    {
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
            NSString foldername = new NSString("MyFolder");

            documentsDirectory = new NSString(path[0]);
            dataPath = documentsDirectory.AppendPathComponent(foldername);

            try
            {
                NSFileManager.DefaultManager.CreateDirectory(dataPath, false, null);
            }catch(Exception ex)
            {
               
            }
        }

        public void ReadFile()
        {

        }

        public void WriteFile()
        {
            NSString folderPath = dataPath.AppendPathComponent(new NSString("testfile.txt"));


            NSString someText = new NSString("Test");

            NSData data = someText.Encode(NSStringEncoding.UTF8);

            NSCoder ncs = new NSCoder();

            


            try
            {

            }catch(Exception ex)
            {

            }
        }

    }
}