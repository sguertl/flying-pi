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
using Java.Lang;
using Java.IO;
using System.IO;
using Android.Util;

namespace WiFiDronection
{
    public class SocketReader : Thread
    {
        /// <summary>
        /// Members
        /// </summary>
        private DataInputStream mDataInputStream;
        
        public SocketReader(DataInputStream inputStream)
        {
            mDataInputStream = inputStream;
        }

        /// <summary>
        /// Reading thread
        /// Reads data from Raspberry
        /// </summary>
        public override void Run()
        {
            int bytes;
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    bytes = mDataInputStream.Read(buffer);
                    string msg = new Java.Lang.String(buffer, 0, bytes).ToString();
                }
                catch(Java.IO.IOException ex)
                {
                    Log.Debug("SocketReader", "Error reading");
                }
                catch(NullReferenceException ex)
                {
                    Log.Debug("SocketReader", "No socket connection");
                    throw;
                }
            }
        }

        /// <summary>
        /// Close connection
        /// </summary>
        public void Close()
        {
            mDataInputStream.Close();
        }
    }
}