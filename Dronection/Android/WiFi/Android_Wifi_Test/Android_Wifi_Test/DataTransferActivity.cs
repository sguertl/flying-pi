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
using Android.Net.Wifi;

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

        private readonly string TAG = "DataTransferActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DataTransfer);

            this.m_socketCon = new SocketConnection();
            this.m_socketCon.Start();
            

            while (!SocketConnection.SOCKET.IsConnected)
            {
                if (SocketConnection.FLAG == false)
                {
                    break;
                }
            }

            if (SocketConnection.FLAG)
            {
                Log.Debug(TAG, "Connection erfolgreich");
                this.mOutputStream = new DataOutputStream(SocketConnection.SOCKET.OutputStream);
            }
            else
            {
                try
                {
                    Cancel();
                }
                catch (Exception ex)
                {
                    Log.Debug(TAG, "Beim schlie0en ist ein fehler aufgetreten");
                }
            }
            etInput = FindViewById<EditText>(Resource.Id.etInput);

            btSendData = FindViewById<Button>(Resource.Id.btSendData);
            btSendData.Click += OnSendData;
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

            etInput.Text = "";
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