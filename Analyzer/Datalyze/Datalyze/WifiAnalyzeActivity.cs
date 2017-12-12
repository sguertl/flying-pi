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

        private Button mBtAnalyse;
        private Button mBtGetResult;
        private Button mBtSaveResult;
        private EditText mEtText;
        private EditText mEtRepetitions;
        private EditText mEtDelay;
        private TextView mTvRead;

        private static Socket mSocket;
        private Thread mConnectionThread;
        private WifiSocketWriter mSocketWriter;
        private WifiSocketReader mSocketReader;
        private string mLastMsg;
        private DataResult mCurrentWifiResult;

        public string LastMsg
        {
            get { return mLastMsg; }
            set { mLastMsg = value; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WifiAnalyzeLayout);

            
            mBtAnalyse = FindViewById<Button>(Resource.Id.btAnalyse);
            mBtGetResult = FindViewById<Button>(Resource.Id.btGetResult);
            mBtSaveResult = FindViewById<Button>(Resource.Id.btSaveResult);
            mEtText = FindViewById<EditText>(Resource.Id.etText);
            mEtRepetitions = FindViewById<EditText>(Resource.Id.etRepetitions);
            mEtDelay = FindViewById<EditText>(Resource.Id.etDelay);
            mTvRead = FindViewById<TextView>(Resource.Id.tvRead);

            mBtAnalyse.Enabled = false;
            mBtGetResult.Enabled = false;

            mBtAnalyse.Click += OnSendData;
            mBtGetResult.Click += OnGetResult;
            mBtSaveResult.Click += OnSaveResult;

            if(mSocket == null || mSocket.IsConnected == false)
            {
                mSocket = new Socket();
                mConnectionThread = new Thread(OnConnect);
                mConnectionThread.Start();
            }
        }

        private void OnSendData(object sender, EventArgs e)
        {
            mLastMsg = "";
            mTvRead.Text = "";
            
            int repetitions = 1;
            int delay = 100;
            string text = "aaaaaaaaaa";
            try
            {
                int anz = Integer.ParseInt(mEtText.Text);
                repetitions = Integer.ParseInt(mEtRepetitions.Text);
                delay = Integer.ParseInt(mEtDelay.Text);
                text = "".PadLeft(anz, 'a');
            }
            catch(NumberFormatException ex)
            {
                Log.Debug("!!!", "Numberformat exception WifiAnalyze");
            }
            if (text.Length > 0)
            {
                if (repetitions == 0) repetitions = 1;
                byte[] bytes = new byte[text.Length + 5];
                mCurrentWifiResult = new DataResult(bytes.Length, repetitions, delay);
                mSocketWriter.Write(1, (byte)bytes.Length);
                int checksum = 0;
                bytes[0] = 10;
                for(int i = 1; i <= text.Length; i++)
                {
                    checksum ^= (byte)text[i - 1];
                    bytes[i] = (byte)text[i - 1];
                }

                bytes[bytes.Length - 4] = (byte)((checksum >> 24) & 0xFF);
                bytes[bytes.Length - 3] = (byte)((checksum >> 16) & 0xFF);
                bytes[bytes.Length - 2] = (byte)((checksum >> 8) & 0xFF);
                bytes[bytes.Length - 1] = (byte)(checksum & 0xFF);

                for (int i = 0; i < repetitions; i++)
                {
                    mSocketWriter.Write(bytes);
                    Thread.Sleep(delay);
                }
                mBtGetResult.Enabled = true;
            }
        }

        private void OnGetResult(object sender, EventArgs e)
        {
            mSocketWriter.Write(11);
            mBtSaveResult.Visibility = ViewStates.Visible;
        }

        private void OnSaveResult(object sender, EventArgs e)
        {
            mCurrentWifiResult.Write();
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
                mLastMsg += msg.Replace('-', '\n');
                if(mLastMsg.Last() == '#')
                {
                    mCurrentWifiResult.SetWifiResults(mLastMsg.Remove(mLastMsg.Length - 2).Split('\n'));
                    mTvRead.Text += $"Received Packets: {mCurrentWifiResult.GetCorrectnessPercentage()}%\n"
                                  + $"Datarate: {mCurrentWifiResult.GetDataRate()} B/s\n"
                                  + $"Average time difference: {mCurrentWifiResult.GetAverageTimeDif()} ms\n\n"
                                  + mLastMsg.Remove(mLastMsg.Length - 2).Replace(';', ' ');
                }
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
                    mBtAnalyse.Enabled = true;
                });
            }
        }
    }
}