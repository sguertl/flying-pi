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

namespace Android_Wifi_Test
{
    public class SocketThread : Thread
    {
        private Socket mSocket;

        private readonly string address = "172.24.1.1";
        private readonly int port = 5050;

        public SocketThread(ref Socket socket)
        {
            mSocket = socket;
        }

        public override void Run()
        {
            mSocket = new Socket(address, port);
        }
    }
}