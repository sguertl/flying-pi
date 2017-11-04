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

namespace Datalyze
{
    public delegate void SaveLastMessage(string msg);

    [Activity(Label = "WifiAnalyzeActivity",
        Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class WifiAnalyzeActivity : Activity
    {
        private readonly string SERVER_ADDRESS = "172.24.1.1";
        private readonly int SERVER_PORT = 5050;
        private readonly string TAG = "WifiAnalyzeActivity";

        private EditText mEtText;
        private Button mBtSend;
        private TextView mTvRead;

        private static Socket mSocket;
        private Thread mConnectionThread;
        private WifiSocketWriter mSocketWriter;
        private WifiSocketReader mSocketReader;
        private string mLastMsg;

        public string LastMsg
        {
            get { return mLastMsg; }
            set { mLastMsg = value; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WifiAnalyzeLayout);

            mEtText = FindViewById<EditText>(Resource.Id.etText);
            mBtSend = FindViewById<Button>(Resource.Id.btSend);
            mTvRead = FindViewById<TextView>(Resource.Id.tvRead);

            mBtSend.Enabled = false;

            mBtSend.Click += OnSendData;

            if(mSocket == null || mSocket.IsConnected == false)
            {
                mSocket = new Socket();
                mConnectionThread = new Thread(OnConnect);
                mConnectionThread.Start();
            }
        }

        private void OnSendData(object sender, EventArgs e)
        {
            Java.Lang.String text = new Java.Lang.String(mEtText.Text);
            byte[] bytes = text.GetBytes();
            mSocketWriter.Write(bytes);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(mSocketWriter != null)
            {
                mSocketWriter.Close();
            }

            if(mSocketReader != null)
            {
                mSocketReader.Close();
            }

            if(mSocket != null)
            {
                mSocket.Close();
            }
        }

        private void PrintLastMsg(string msg)
        {
            RunOnUiThread(() =>
            {
                mLastMsg += msg + "\n";
                mTvRead.Text = mLastMsg;
            });
        }

        private void OnConnect()
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
                mSocketWriter = new WifiSocketWriter(mSocket.OutputStream);
                mSocketReader = new WifiSocketReader(mSocket.InputStream, PrintLastMsg);
                RunOnUiThread(() =>
                {
                    mBtSend.Enabled = true;
                    mBtSend.Text = "Send";
                });
            }
        }
    }
}