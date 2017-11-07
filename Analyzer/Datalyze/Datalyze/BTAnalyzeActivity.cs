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
    [Activity(Label = "BTAnalyzeActivity")]
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
        private BluetoothDevice mPeer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            mAdapter = BluetoothAdapter.DefaultAdapter;

            Init(HelpClass.Instance.BluetoohtDevice);

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
                mSocketReader = new BTSocketReader(new DataInputStream(mSocket.InputStream));
                mSocketWriter = new BTSocketWriter(new DataOutputStream(mSocket.OutputStream));

            }
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
    }
}