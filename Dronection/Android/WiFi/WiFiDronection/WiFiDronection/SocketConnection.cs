/************************************************************************
*                                                                       *
*  Copyright (C) 2017-2018 Infineon Technologies Austria AG.            *
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
*  File: SocketConnection.cs                                            *
*  Created on: 2017-07-27                                               *
*  Author(s): Christoph Englert                                         *
*             Adrian Klapsch                                            *
*                                                                       *
*  SocketConnection is a Singleton class that establishes a socket      *
*  connection to the Raspberry.                                         *
*                                                                       *
************************************************************************/

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
using Java.Net;
using Java.Lang;
using Android.Util;
using Java.IO;

namespace WiFiDronection
{
    public class SocketConnection
    {
        // Constants
        private static readonly string TAG = "SocketConnection";

        private readonly string SERVER_ADDRESS = "172.24.1.1";
        private readonly int SERVER_PORT = 5050;

        // Members
        private Socket mSocket;
        private SocketWriter mSocketWriter;
        private SocketReader mSocketReader;
        private Thread mConnectionThread;
        private string mLogData;
        private long mStartMillis;
        private RaspberryClose mRaspberryClose;
        private bool isConnectingFinished;

        public bool IsConnectingFinished
        {
            get { return isConnectingFinished; }
        }

        /// <summary>
        /// Gets the log data.
        /// </summary>
        /// <value>The log data.</value>
        public string LogData
        {
            get { return mLogData; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:WiFiDronection.SocketConnection"/> is connected.
        /// </summary>
        /// <value><c>true</c> if is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected
        {
            get { return mSocket.IsConnected; }
        }

        /// <summary>
        /// Gets the drone logs.
        /// </summary>
        /// <value>The drone logs.</value>
        public Dictionary<string, LogData> DroneLogs
        {
            get { return mSocketReader.DroneLogs; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WiFiDronection.SocketConnection"/> class.
        /// </summary>
        /// <param name="raspClose">Raspberry close delegate</param>
        public SocketConnection(RaspberryClose raspClose)
        {
            mRaspberryClose = raspClose;
            mSocket = new Socket();
            mConnectionThread = new Thread(Connect);
            isConnectingFinished = false;
        }

        /// <summary>
        /// Starts the connection to the raspberry.
        /// </summary>
        public void StartConnection()
        {
            mConnectionThread.Start();
        }

        /// <summary>
        /// Tries to connect the socket with a specific ip address and port.
        /// If first connection attempt fails a second one is executed
        /// socket streams are created for reading and writing.
        /// </summary>
        private void Connect()
        {
            if (mSocket.IsConnected == false)
            {
                isConnectingFinished = false;
                try
                {
                    // Connect to socket
                    mSocket = new Socket(SERVER_ADDRESS, SERVER_PORT);
                }
                catch (UnknownHostException uhe)
                {
                    Log.Debug(TAG, uhe.Message + " if the IP address of the host could not be determined.");
                }
                catch (IOException uhe)
                {
                    Log.Debug(TAG, uhe.Message + " if an I/O error occurs when creating the socket.");
                }
                catch (SecurityException uhe)
                {
                    Log.Debug(TAG, uhe.Message + " if a security manager exists and its checkConnect method doesn't allow the operation.");
                }
                catch (IllegalAccessException uhe)
                {
                    Log.Debug(TAG, uhe.Message + " if the port parameter is outside the specified range of valid port values, which is between 0 and 65535, inclusive.");
                }

                try
                {
                    if (!mSocket.IsConnected)
                    {
                        // If first connection attempt fails try again
                        SocketAddress socketAdr = new InetSocketAddress(SERVER_ADDRESS, SERVER_PORT);
                        Thread.Sleep(5000);
                        mSocket.Connect(socketAdr, 2000);
                    }
                }
                catch (Java.Lang.Exception ex)
                {
                    Log.Debug(TAG, ex.Message);

                }
                finally
                {
                    if (mSocket.IsConnected)
                    {
                        // Create socket reading and writing streams
                        mSocketWriter = new SocketWriter(mSocket.OutputStream);
                        mSocketReader = new SocketReader(new DataInputStream(mSocket.InputStream), mRaspberryClose);
                    }
                    isConnectingFinished = true;
                }
            }
        }

        /// <summary>
        /// Starts listening to the raspberry.
        /// </summary>
        public void StartListening()
        {
            if(mSocket.IsConnected == true)
            {
                mSocketReader.StartListening();

                byte isReady = 0;
                int mCommunicationBegin = 0;

                while (isReady == 0)
                {
                    mCommunicationBegin += 500;
                    if (mSocketReader.CurrentMsg == "0")
                    {
                        isReady = 1;
                    }

                    if (mSocketReader.CurrentMsg == "2")
                    {
                        isReady = 2;
                    }

                    if (mCommunicationBegin > 3000)
                    {
                        isReady = 3;
                    }
                    System.Threading.Thread.Sleep(500);
                }

                if (isReady > 1)
                {
                    mRaspberryClose();
                    return;
                }
            }
        }

        /// <summary>
        /// Writes the data to output writer.
        /// </summary>
        /// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
        public void WriteControl(params Int16[] args)
        {
            if (mSocket.IsConnected == true)
            {
                mLogData += mStartMillis + "," + args[0] + "," + args[1] + "," + args[2] + "," + args[3] + "," + (ControllerView.Settings.AltitudeControlActivated ? 1 : 0) + "\n";
                mStartMillis += 10;
                mSocketWriter.Write(args);
            }
        }

        /// <summary>
        /// Writes the log data to output writer.
        /// </summary>
        /// <param name="args">Log data</param>
        public void WriteLog(params byte[] args)
        {
            if (mSocket.IsConnected == true)
            {
                mSocketWriter.Write(args);
            }
        }

        /// <summary>
        /// Closes, if necessary, the socket connection.
        /// </summary>
        public void Close()
        {
            if(mSocket != null && mSocket.IsConnected == true)
            {
                mSocketWriter.Close();
                mSocketReader.Close();
                mSocket.Close();
            }
        }
    }
}