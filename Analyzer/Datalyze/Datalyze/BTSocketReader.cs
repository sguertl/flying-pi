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
using Android.Util;
using System.IO;

namespace Datalyze
{
   public class BTSocketReader
    {
        private DataInputStream mInputStream;
        private Thread mReaderThread;
        private bool isReading;
        private SaveLastMessage mSaveLastMessage;

        public BTSocketReader(DataInputStream inputStream, SaveLastMessage saveLastMessage)
        {
            mInputStream = inputStream;
            mSaveLastMessage = saveLastMessage;
            mReaderThread = new Thread(Read);
            mReaderThread.Start();
        }

        private void Read()
        {
            int bytes = 0;
            byte[] buffer = new byte[1024];
            isReading = true;

            while (isReading)
            {
                try
                {
                    bytes = mInputStream.Read(buffer);
                    string str = new Java.Lang.String(buffer).ToString();
                    mSaveLastMessage(str);
                }
                catch (Java.Lang.Exception ex)
                {
                    Log.Debug("BtSocketReader", "Error while reading");
                }
            }
        }

        public void Close()
        {
            isReading = false;
            
            if(mInputStream != null)
            {
                mInputStream.Close();
            }
            if(mReaderThread != null)
            {
                mReaderThread.Join();
            }
        }
    }
}