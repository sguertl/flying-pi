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
using Android.Util;
using Java.Net;
using Java.IO;

namespace WiFiDronection
{
    public class SocketConnection : Thread
    {
        private static readonly string SERVER_ADDRESS = "172.24.1.1";

        private static readonly int SERVERPORT = 5050;

        private readonly string TAG = "SocketConnection";

        public static bool FLAG = true;

        public static Socket m_Socket;

        public SocketConnection()
        {
            m_Socket = new Socket();
        }

        public override void Run()
        {
            try
            {
                m_Socket = new Socket(SERVER_ADDRESS, SERVERPORT);
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
                if (!m_Socket.IsConnected)
                {
                    SocketAddress socketAdr = new InetSocketAddress(SERVER_ADDRESS, SERVERPORT);
                    Thread.Sleep(5000);
                    m_Socket.Connect(socketAdr, 2000);
                }
            }
            catch (Java.Lang.Exception ex)
            {
                FLAG = false;
                Log.Debug(TAG, ex.Message);
                return;
            }
        }

    }
}