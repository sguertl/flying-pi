using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using System.IO;
using Android.Util;

namespace Datalyze
{
    public class WifiSocketReader
    {
        private DataInputStream mInputStream;
        private Thread mReaderThread;
        private bool isReading;
        private SaveLastMessage mSaveLastMessage;

        public WifiSocketReader(Stream inputStream, SaveLastMessage saveLastMessage)
        {
            mInputStream = new DataInputStream(inputStream);
            mSaveLastMessage = saveLastMessage;
            mReaderThread = new Thread(Read);
            mReaderThread.Start();
        }

        private void Read()
        {
            int bytes = 0;
            byte[] buffer = new byte[32];
            isReading = true;

            while (isReading)
            {
                try
                {
                    bytes = mInputStream.Read(buffer);
                    Java.Lang.String str = new Java.Lang.String(buffer);
                    mSaveLastMessage(str.ToString());
                }
                catch(Java.Lang.Exception ex)
                {
                    Log.Debug("WifiSocketReader", "Error while reading");
                }
            }
        }

        public void Close()
        {
            isReading = false;
            mInputStream.Close();
            mReaderThread.Join();
        }
    }
}