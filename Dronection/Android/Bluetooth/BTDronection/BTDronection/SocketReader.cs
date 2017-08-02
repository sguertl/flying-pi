/************************************************************************
*                                                                       *
*  Copyright (C) 2017 Infineon Technologies Austria AG.                 *
*                                                                       *
*  Licensed under the Apache License, Version 2.0 (the "License");      *
*  you may not use this file except in compliance with the License.     *
*  You may obtain a copy of the License at                              *
*                                                                       *
*    http://www.apache.org/licenses/LICENSE-2.0                         *
*                                                                       *
*  Unless required by applicable law or agreed to in writing, software  *
*  distributed under the License is distributed on an "AS IS" BASIS,    *
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or      *
*  implied.                                                             *
*  See the License for the specific language governing                  *
*  permissions and limitations under the License.                       *
*                                                                       *
*                                                                       *
*  File: SocketReader.cs                                                *
*  Created on: 2017-07-26                                               *
*  Author(s): Klapsch Adrian Vasile (IFAT PMM TI COP)                   *
*                                                                       *
*  SocketReader reads data which is sent by Raspberry.                  *
*                                                                       *
************************************************************************/

using Java.IO;
using Java.Lang;
using Android.Util;

namespace BTDronection
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
            this.mDataInputStream = inputStream;
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
                }catch(IOException ex)
                {
                    Log.Debug(TAG, "Error reading (" + ex.Message + ")");
                }catch(NullPointerException ex)
                {
                    Log.Debug(TAG, "No Bluetooth-Socket available  (" + ex.Message + ")");
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
		/// Closes connection.
		/// </summary>
		public void Close()
        {
            try
            {
                mDataReaderThread = null;

                if(mDataInputStream != null)
                {
                    mDataInputStream.Close();
                }
            }
            catch(Java.Lang.Exception ex)
            {
                Log.Debug(TAG, ex.Message);
            }
        }
    }
}