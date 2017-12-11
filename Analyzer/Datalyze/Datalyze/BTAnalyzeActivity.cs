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
        private readonly int PACKET_SIZE = 19;
        private readonly byte START_BYTE = 0x00;

        private static BluetoothSocket mSocket;
        private Thread mConnectionThread;
        private BTSocketWriter mSocketWriter;
        private BTSocketReader mSocketReader;
        private BluetoothAdapter mAdapter;
        private string mLastMessage;

        public string LastMessage
        {
            get { return mLastMessage; }
            set { mLastMessage = value; }
        }

        private Button mBtSend;
        private EditText mEtText;
        private EditText mRepText;
        private EditText mDelayText;
        private TextView mTvRead;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BTAnalyzeLayout);

            mBtSend = FindViewById<Button>(Resource.Id.btSend);
            mBtSend.Click += OnSend;
            mBtSend.Enabled = false;
            mEtText = FindViewById<EditText>(Resource.Id.etText);
            mRepText = FindViewById<EditText>(Resource.Id.etRepetitions);
            mDelayText = FindViewById<EditText>(Resource.Id.etDelay);
            mTvRead = FindViewById<TextView>(Resource.Id.tvRead);

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
            string text = mEtText.Text;

            byte[] bytes = new byte[text.Length + 4];
            int checksum = 0;

            for (int i = 0; i < text.Length; i++)
            {
                checksum ^= (byte)text[i];
                bytes[i] = (byte)text[i];
            }

            bytes[bytes.Length - 4] = (byte)((checksum >> 24) & 0xFF);
            bytes[bytes.Length - 3] = (byte)((checksum >> 16) & 0xFF);
            bytes[bytes.Length - 2] = (byte)((checksum >> 8 ) &0xFF);
            bytes[bytes.Length - 1] = (byte)(checksum & 0xFF);

            int repetitions = 1;
            int delay = 0;

            try
            {
                repetitions = Integer.ParseInt(mRepText.Text);
                delay = Integer.ParseInt(mDelayText.Text);
            }catch(NumberFormatException ex)
            {
                Log.Debug("BTAnalyzeActivity", "Numberformat exception");
            }

            string str = "";
            for (int i = 0; i < repetitions; i++)
            {
                try
                {
                    mSocketWriter.Write(bytes);              
                    str += "Current Package  = " + (i + 1) + " Time = " + DateTime.Now + "\n";              
                    Thread.Sleep(delay);
                }catch(System.Exception ex)
                {
                    Log.Debug("BTAnalyzeActivity", "Fehler beim senden");
                }
            }

            RunOnUiThread(() =>
            {
                mTvRead.Text = str;
            });
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
                //mSocketReader = new BTSocketReader(new DataInputStream(mSocket.InputStream), PrintLastMsg);
                mSocketWriter = new BTSocketWriter(new DataOutputStream(mSocket.OutputStream));
              
               RunOnUiThread(() =>
                {
                    mBtSend.Text = "Send";
                    mBtSend.Enabled = true;
                });
            }
        }

        private void PrintLastMsg(string msg)
        {
            RunOnUiThread(() =>
            {
                mLastMessage += msg + "\n";
                mTvRead.Text = mLastMessage;
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

            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            try { 
            mConnectionThread = null;

            if (mSocketWriter != null)
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
            catch (Java.Lang.Exception ex)
            {

            }
        }
    }
}