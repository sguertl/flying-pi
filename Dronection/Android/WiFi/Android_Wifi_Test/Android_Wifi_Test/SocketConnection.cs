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
using Java.Net;
using Android.Util;

namespace Android_Wifi_Test
{
    public class SocketConnection : Thread
    {

        private static readonly string SERVER_ADDRESS = "172.24.1.1";

        private static readonly int SERVERPORT = 5050;

        private static readonly string TAG = "SocketConnection";

        public static bool FLAG = true;

        public static Socket SOCKET;

        public SocketConnection()
        {
            SOCKET = new Socket();
        }

        public override void Run()
        {
            try
            {
                SOCKET = new Socket(InetAddress.GetByName(SERVER_ADDRESS), SERVERPORT);
            }catch(Java.Lang.Exception ex)
            {
                FLAG = false;
                Log.Debug(TAG, ex.Message);
                return;
            }
        }

    }
}