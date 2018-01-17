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
        private static readonly string TAG = "SocketConnection";

        private readonly string SERVER_ADDRESS = "172.24.1.1";
        private readonly int SERVER_PORT = 5050;

        private Socket mSocket;
        private SocketWriter mSocketWriter;
        private SocketReader mSocketReader;
        private Thread mConnectionThread;
        private string mLogData;
        private long mStartMillis;
        private RaspberryClose mRaspberryClose;

        public string LogData
        {
            get { return mLogData; }
        }

        public bool IsConnected
        {
            get { return mSocket.IsConnected; }
        }

        public Dictionary<string, LogData> DroneLogs
        {
            get { return mSocketReader.DroneLogs; }
        }

        public SocketConnection(RaspberryClose raspClose)
        {
            mRaspberryClose = raspClose;
            mSocket = new Socket();
            mConnectionThread = new Thread(Connect);
        }

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
                }
            }
        }

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

                    if (mCommunicationBegin > 10000)
                    {
                        isReady = 3;
                    }
                    System.Threading.Thread.Sleep(500);
                }
                isReady = 1;
                if (isReady > 1)
                {
                    mRaspberryClose();
                    return;
                }
            }
        }

        public void WriteControl(params Int16[] args)
        {
            if (mSocket.IsConnected == true)
            {
                mLogData += mStartMillis + "," + args[0] + "," + args[1] + "," + args[2] + "," + args[3] + "," + (ControllerView.Settings.AltitudeControlActivated ? 1 : 0) + "\n";
                mStartMillis += 10;
                mSocketWriter.Write(args);
            }
        }

        public void WriteLog(params byte[] args)
        {
            if (mSocket.IsConnected == true)
            {
                mSocketWriter.Write(args);
            }
        }

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