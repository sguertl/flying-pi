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
using Android.Bluetooth;
using Java.Lang;
using Java.Lang.Reflect;
using Android.Util;
using Java.IO;

namespace Datalyze
{
    [Activity(Label = "BTAnalyzeActivity", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class BTAnalyzeActivity : Activity
    {
        private readonly string TAG = "BTAnalyzerActivity";

        private Button mBtAnalyse;
        private Button mBtGetResult;
        private Button mBtSaveResult;
        private EditText mEtText;
        private EditText mRepText;
        private EditText mDelayText;
        private TextView mTvRead;

        private static BluetoothSocket mSocket;
        private Thread mConnectionThread;
        private BTSocketWriter mSocketWriter;
        private BTSocketReader mSocketReader;
        private BluetoothAdapter mAdapter;
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
            SetContentView(Resource.Layout.BTAnalyzeLayout);

            mBtAnalyse = FindViewById<Button>(Resource.Id.btBTAnalyse);
            mBtGetResult = FindViewById<Button>(Resource.Id.btBTGetResult);
            mBtSaveResult = FindViewById<Button>(Resource.Id.btBTSaveResult);
            mEtText = FindViewById<EditText>(Resource.Id.etBTText);
            mRepText = FindViewById<EditText>(Resource.Id.etBTRepetitions);
            mDelayText = FindViewById<EditText>(Resource.Id.etBTDelay);
            mTvRead = FindViewById<TextView>(Resource.Id.tvBTRead);

            mBtAnalyse.Enabled = false;
            mBtGetResult.Enabled = false;

            mBtAnalyse.Click += OnSend;
            mBtGetResult.Click += OnGetResult;
            mBtSaveResult.Click += OnSaveResult;

            mAdapter = BluetoothAdapter.DefaultAdapter;
            BluetoothDevice device = (BluetoothDevice)Intent.GetParcelableExtra("device");
            Init(device);
        }

        public void Init(BluetoothDevice device)
        {
            if (mSocket == null || mSocket.IsConnected == false)
            {
                try
                {
                    BluetoothSocket tmp = null;
                    tmp = device.CreateInsecureRfcommSocketToServiceRecord(device.GetUuids()[0].Uuid);
                    Class helpClass = tmp.RemoteDevice.Class;
                    Class[] paramTypes = new Class[] { Integer.Type };
                    Method m = helpClass.GetMethod("createRfcommSocket", paramTypes);
                    Java.Lang.Object[] param = new Java.Lang.Object[] { Integer.ValueOf(1) };

                    mSocket = (BluetoothSocket)m.Invoke(tmp.RemoteDevice, param);
                    mConnectionThread = new Thread(OnConnect);

                    mConnectionThread.Start();
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception();
                }
            }
        }

        private void OnSend(object sender, EventArgs e)
        {
            mLastMsg = "";
            mTvRead.Text = "";

            int repetitions = 1;
            int delay = 100;
            string text = "aaaaaaaaaa";
            try
            {
                int anz = Integer.ParseInt(mEtText.Text);
                repetitions = Integer.ParseInt(mRepText.Text);
                delay = Integer.ParseInt(mDelayText.Text);
                text = "".PadLeft(anz, 'a');
            }
            catch (NumberFormatException ex)
            {
                Log.Debug("!!!", "Numberformat exception WifiAnalyze");
            }
            if (text.Length > 0)
            {
                if (repetitions == 0) repetitions = 1;
                byte[] bytes = new byte[text.Length + 5];
                int length = bytes.Length;
                mCurrentWifiResult = new DataResult(length, repetitions, delay);
                byte[] lengthBytes = new byte[5];
                lengthBytes[0] = 0x1;
                lengthBytes[1] = (byte)((length >> 24) & 0xFF);
                lengthBytes[2] = (byte)((length >> 16) & 0xFF);
                lengthBytes[3] = (byte)((length >> 8) & 0xFF);
                lengthBytes[4] = (byte)(length & 0xFF);
                mSocketWriter.Write(lengthBytes);
                int checksum = 0;
                bytes[0] = 10;
                for (int i = 1; i <= text.Length; i++)
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

        private void OnConnect()
        {
            mAdapter.CancelDiscovery();

            try
            {
                if (mSocket.IsConnected == false)
                {
                    Thread.Sleep(2000);
                    // a to the bluetooth device
                    mSocket.Connect();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                Log.Debug(TAG, "Connection could not be created (" + ex.Message + ")");
            }

            if (mSocket.IsConnected)
            {
                mSocketReader = new BTSocketReader(new DataInputStream(mSocket.InputStream), PrintLastMsg);
                mSocketWriter = new BTSocketWriter(new DataOutputStream(mSocket.OutputStream));
              
               RunOnUiThread(() =>
                {
                    mBtAnalyse.Text = "Send";
                    mBtAnalyse.Enabled = true;
                });
            }
        }

        private void PrintLastMsg(string msg)
        {
            RunOnUiThread(() =>
            {
                mLastMsg += msg.Replace('-', '\n');
                if (mLastMsg.Last() == '#')
                {
                    mCurrentWifiResult.SetWifiResults(mLastMsg.Remove(mLastMsg.Length - 2).Split('\n'));
                    mTvRead.Text += $"Received Packets: {mCurrentWifiResult.GetCorrectnessPercentage()}%\n"
                                  + $"Datarate: {mCurrentWifiResult.GetDataRate()} kB/s\n"
                                  + $"Average time difference: {mCurrentWifiResult.GetAverageTimeDif()} ms\n\n"
                                  + mLastMsg.Remove(mLastMsg.Length - 2).Replace(';', ' ');
                }
            });
        }

        protected override void OnStop()
        {
            base.OnStop();
            try
            {

                mConnectionThread = null;

                if (mSocketWriter != null)
                {
                    mSocketWriter.Close();
                }
                if (mSocketReader != null)
                {
                    mSocketReader.Close();
                }
                if (mSocket != null)
                {
                    mSocket.Close();
                }
            }catch(Java.Lang.Exception ex)
            {
                Log.Debug(TAG, ex.Message);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            try
            {
                mConnectionThread = null;

                if (mSocketWriter != null)
                {
                    mSocketWriter.Close();
                }
                if (mSocketReader != null)
                {
                    mSocketReader.Close();
                }
                if (mSocket != null)
                {
                    mSocket.Close();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                Log.Debug(TAG, ex.Message);
            }
        }
    }
}