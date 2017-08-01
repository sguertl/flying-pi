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
using Android.Bluetooth;
using Java.Lang.Reflect;
using Android.Util;
using Java.IO;


namespace BTDronection
{
    public class SocketConnection
    {
        private BluetoothSocket mSocket;
        private BluetoothAdapter mAdapter;
        private BluetoothDevice mPeer;
        private DataOutputStream mDataOutputStream;
        private DataInputStream mDataInputStream;
        private string mLogData;
        private long mStartMillis;

        // Public Member
        public Thread mConnectionThread;

        private readonly byte StartByte = 0x00;
        private readonly int PacketSize = 19;

        public BluetoothDevice Peer
        {
            get { return mPeer; }
            set { mPeer = value; }
        }

        public DataInputStream InputStream
        {
            get { return mDataInputStream; }
        }

        public BluetoothSocket Socket
        {
            get { return mSocket; }
        }

        public string LogData
        {
            get { return mLogData; }
        }

        private static SocketConnection instance = null;
        private static readonly object padlock = new object();
        private static readonly string TAG = "SocketConnection";

        public static SocketConnection Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SocketConnection();
                    }
                    return instance;
                }
            }
        }

        private SocketConnection()
        {
            Init();
        }

        private void Init()
        {
            mAdapter = BluetoothAdapter.DefaultAdapter;
            mStartMillis = 0;

            this.mConnectionThread = new Thread(BuildConnection);
        }

        public void Init(BluetoothDevice device)
        {
            Init();
            BluetoothSocket tmp = null;
            tmp = device.CreateInsecureRfcommSocketToServiceRecord(device.GetUuids()[0].Uuid);
            Class helpClass = tmp.RemoteDevice.Class;
            Class[] paramTypes = new Class[] { Integer.Type };
            Method m = helpClass.GetMethod("createRfcommSocket", paramTypes);
            Java.Lang.Object[] param = new Java.Lang.Object[] { Integer.ValueOf(1) };

            mSocket = (BluetoothSocket)m.Invoke(tmp.RemoteDevice, param);
        }

        public void OnStartConnection()
        {
            if (mConnectionThread != null)
            {
                mConnectionThread.Start();
                mConnectionThread.Join();
            }

        }
        public void BuildConnection()
        {
                mAdapter.CancelDiscovery();

                try
                {
                    if (mSocket.IsConnected == false)
                    {
                        Thread.Sleep(2000);
                        mSocket.Connect();
                    }
                }
                catch (Java.Lang.Exception ex)
                {
                    Log.Debug(TAG, "Connection could not be created (" + ex.Message + ")");
                    Cancel();
                }

            if (mSocket.IsConnected)
            {
                mDataOutputStream = new DataOutputStream(mSocket.OutputStream);
                mDataInputStream = new DataInputStream(mSocket.InputStream);
            }

        }

        public void Write(params Int16[] args)
        {
            mLogData += mStartMillis + "," + args[0] + "," + args[1] + "," + args[2] + "," + args[3] + "," + (ControllerView.Settings.AltitudeControlActivated ? 1 : 0) + "\n";

            mStartMillis += 10;
            byte[] bytes = ConvertToByte(args);
            try
            {
                mDataOutputStream.Write(bytes);
                mDataOutputStream.Flush();
            }
            catch(Java.Lang.Exception ex)
            {
                Log.Debug(TAG, "Error while sending data (" + ex.Message + ")");
                Cancel();
            }
        }

        private byte[] ConvertToByte(params Int16[] args)
        {
            byte[] b = new byte[PacketSize];
            byte speed = (byte)args[0];
            byte heightcontrol = 0;
            int azimuth = Java.Lang.Float.FloatToIntBits(args[1]);
            int pitch = Java.Lang.Float.FloatToIntBits(args[2]);
            int roll = Java.Lang.Float.FloatToIntBits(args[3]);

            int checksum = StartByte;
            checksum ^= (heightcontrol << 8 | speed) & 0xFFFF;
            checksum ^= azimuth;
            checksum ^= pitch;
            checksum ^= roll;

            b[0] = StartByte;

            b[1] = (byte)(heightcontrol & 0xFF);
            b[2] = (byte)(speed & 0xFF);

            b[3] = (byte)((azimuth >> 24) & 0xFF);
            b[4] = (byte)((azimuth >> 16) & 0xFF);
            b[5] = (byte)((azimuth >> 8) & 0xFF);
            b[6] = (byte)(azimuth & 0xFF);

            b[7] = (byte)((pitch >> 24) & 0xFF);
            b[8] = (byte)((pitch >> 16) & 0xFF);
            b[9] = (byte)((pitch >> 8) & 0xFF);
            b[10] = (byte)(pitch & 0xFF);

            b[11] = (byte)((roll >> 24) & 0xFF);
            b[12] = (byte)((roll >> 16) & 0xFF);
            b[13] = (byte)((roll >> 8) & 0xFF);
            b[14] = (byte)(roll & 0xFF);

            b[15] = (byte)((checksum >> 24) & 0xFF);
            b[16] = (byte)((checksum >> 16) & 0xFF);
            b[17] = (byte)((checksum >> 8) & 0xFF);
            b[18] = (byte)(checksum & 0xFF);
            return b;
        }

        public void Cancel()
        {
            try
            {
                this.mConnectionThread = null;
                
                if (mDataOutputStream != null)
                {
                    mDataOutputStream.Close();
                }

                if(mSocket != null)
                {
                    mSocket.Close();
                }
            }catch(Java.Lang.Exception ex)
            {
                Log.Debug(TAG, "Cancel Failed (" + ex.Message + ")");
            }
        }
    }
}