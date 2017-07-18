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
using System.IO;
using Java.IO;
using System.Threading.Tasks;
using Android.Util;

namespace Android_Wifi_Test
{
    [Activity(Label = "DataTransferActivity")]
    public class DataTransferActivity : Activity
    {
        private EditText etInput;
        private Button btSendData;

        //private Socket mSocket;
        //private Stream mOutputStream;
        private SocketConnection m_socketCon;

        private DataOutputStream mOutputStream;

        private static readonly string TAG = "DataTransferActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DataTransfer);

            this.m_socketCon = new SocketConnection();
            this.m_socketCon.Start();

            while (!SocketConnection.SOCKET.IsConnected)
            {
                if(SocketConnection.FLAG == false)
                {
                    break;
                }
            }

            if (SocketConnection.FLAG)
            {
                Log.Debug(TAG, "Connection erfolgreich");
            }

            etInput = FindViewById<EditText>(Resource.Id.etInput);

            btSendData = FindViewById<Button>(Resource.Id.btSendData);
            btSendData.Click += OnSendData;

            //mSocket = new Socket("172.24.1.1", 5050);
            //mSocket.Bind(null);
            //mSocket.Connect(new InetSocketAddress("172.24.1.1", 5050), 5000);

            //mOutputStream = new DataOutputStream(mSocket.OutputStream);
            //Hello();
        }

        /*
        public async Task EstablishConnection()
        {
            try
            {
                mSocket = new Socket("172.24.1.1", 5050);
                mOutputStream = new DataOutputStream(mSocket.OutputStream);
            }
            catch(System.Exception ex)
            {
                Log.Debug("!!!", "Error socket connection");
            }
        }

        public async void Hello()
        {
            await EstablishConnection();
        }
        */
        private void OnSendData(object sender, EventArgs e)
        {
            Java.Lang.String text = new Java.Lang.String(etInput.Text);
            byte[] byteText = text.GetBytes();

            try
            {
                mOutputStream.Write(byteText, 0, byteText.Length);
                mOutputStream.Flush();
            }
            catch(System.Exception ex)
            {
                Cancel();
                System.Console.WriteLine(ex.Message);
            }
        }

        private void Cancel()
        {
            try
            {
                mOutputStream.Close();
                SocketConnection.SOCKET.Close();
            }
            catch(System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}