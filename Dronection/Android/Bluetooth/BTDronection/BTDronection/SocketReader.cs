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

namespace BTDronection
{
   public class SocketReader
    {

        private static readonly string TAG = "SocketReader";

        private DataInputStream mDataInputStream;

        public Thread mDataReaderThread;

        public SocketReader(DataInputStream inputStream)
        {
            this.mDataInputStream = inputStream;
            this.mDataReaderThread = new Thread(OnRead);
        }

        public void OnRead()
        {
            int bytes = 0;

            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    bytes = mDataInputStream.Read(buffer);
                    string msg = new Java.Lang.String(buffer, 0, bytes).ToString();
                }catch(IOException ex)
                {
                    Log.Debug(TAG, "Error reading (" + ex.Message + ")");
                }catch(NullPointerException ex)
                {
                    Log.Debug(TAG, "No Bluetooth-Socket available  (" + ex.Message + ")");
                }
            }

        }


        public void OnStart()
        {
            this.mDataReaderThread = new Thread(OnRead);
            this.mDataReaderThread.Start();
        }

        public void Close()
        {
            try
            {
                mDataReaderThread = null;

                if(mDataInputStream != null)
                {
                    mDataInputStream.Close();
                }
            }catch(Java.Lang.Exception ex)
            {
                Log.Debug(TAG, ex.Message);
            }
        }
    }
}