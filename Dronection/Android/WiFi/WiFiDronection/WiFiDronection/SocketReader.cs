using System;

using Java.Lang;
using Java.IO;
using Android.Util;

namespace WiFiDronection
{
    public class SocketReader
    {
        private static readonly string TAG = "SocketReader";

        // Input stream
        private DataInputStream mDataInputStream;

        // Data reader thread
        public Thread mDataReaderThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WiFiDronection.SocketReader"/> class.
        /// </summary>
        /// <param name="inputStream">Input stream.</param>
        public SocketReader(DataInputStream inputStream)
        {
            mDataInputStream = inputStream;
            this.mDataReaderThread = new Thread(OnRead);
        }

        /// <summary>
        /// Reads data from Raspberry in a thread.
        /// </summary>
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
                }
                catch (Java.IO.IOException ex)
                {
                    Log.Debug(TAG, "Error reading (" + ex.Message + ")");
                }
                catch (NullReferenceException ex)
                {
                    Log.Debug(TAG, "No socket connection (" + ex.Message + ")");
                   //  throw new NullReferenceException();
                }
            }
        }

        /// <summary>
        /// Creates and starts the thread. 
        /// </summary>
        public void OnStart()
        {
            this.mDataReaderThread = new Thread(OnRead);
            this.mDataReaderThread.Start();
        }

		/// <summary>
		/// Reads data from Raspberry in a thread.
		/// </summary>
		

		/// <summary>
		/// Closes connection
		/// </summary>
		public void Close()
        {
            try
            {
                this.mDataReaderThread = null;

                if (mDataInputStream != null)
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