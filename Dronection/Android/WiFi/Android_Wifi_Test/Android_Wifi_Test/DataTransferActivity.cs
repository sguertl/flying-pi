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

        private Socket mSocket;
        //private Stream mOutputStream;
        private DataOutputStream mOutputStream;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DataTransfer);

            etInput = FindViewById<EditText>(Resource.Id.etInput);

            btSendData = FindViewById<Button>(Resource.Id.btSendData);
            btSendData.Click += OnSendData;

            //mSocket = new Socket("172.24.1.1", 5050);
            //mSocket.Connect(new InetSocketAddress("172.24.1.1", 5050), 5000);
            mSocket = new Socket();
            SocketThread st = new SocketThread(ref mSocket);
            st.Start();
            if (mSocket.IsConnected)
            {
                mOutputStream = new DataOutputStream(mSocket.OutputStream);
            }
            else
            {
                System.Console.WriteLine("fail");
            }
        }

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
                mSocket.Close();
            }
            catch(System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}