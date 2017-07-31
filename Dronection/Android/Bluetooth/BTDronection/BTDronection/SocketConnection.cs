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
using System.Threading;

namespace BTDronection
{
    public class SocketConnection// : Thread
    {
        private BluetoothSocket mSocket;
        private BluetoothAdapter mAdapter;
        private BluetoothDevice mPeer;
        private DataOutputStream mDataOutputStream;
        private DataInputStream mDataInputStream;
        private string mLogData;
        private long mStartMillis;
        private bool mIsConnected;
        private bool mIsConnectionFailed;

        private readonly byte StartByte = 0x00;
        private readonly int PacketSize = 19;

        public bool IsConnected
        {
            get { return mIsConnected; }
        }

        public bool IsConnectionFailed
        {
            get { return mIsConnectionFailed; }
        }

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

        public SocketConnection()
        {
            mAdapter = BluetoothAdapter.DefaultAdapter;
            mStartMillis = 0;
            mIsConnected = false;
            mIsConnectionFailed = false;
        }

        public void BuildConnection(BluetoothDevice device)
        {
            BluetoothSocket tmp = null;
            tmp = device.CreateInsecureRfcommSocketToServiceRecord(device.GetUuids()[0].Uuid);
            Class helpClass = tmp.RemoteDevice.Class;
            Class[] paramTypes = new Class[] { Integer.Type };
            Method m = helpClass.GetMethod("createRfcommSocket", paramTypes);
            Java.Lang.Object[] param = new Java.Lang.Object[] { Integer.ValueOf(1) };

            mSocket = (BluetoothSocket)m.Invoke(tmp.RemoteDevice, param);

            ThreadPool.QueueUserWorkItem(lol =>
            {
                mAdapter.CancelDiscovery();
                try
                {
                    if (mSocket.IsConnected == false)
                    {
                        mSocket.Connect();
                        mIsConnected = true;
                    }
                }
                catch (System.Exception sex)
                {
                    mIsConnected = false;
                    mIsConnectionFailed = true;
                    Cancel();
                    Log.Debug("SocketConnection", "Connection could not be created");
                }
                mDataOutputStream = new DataOutputStream(mSocket.OutputStream);
                mDataInputStream = new DataInputStream(mSocket.InputStream);
            });
        }

        /*public override void Run()
        {
            BluetoothSocket tmp = null;
            tmp = mPeer.CreateInsecureRfcommSocketToServiceRecord(mPeer.GetUuids()[0].Uuid);
            Class helpClass = tmp.RemoteDevice.Class;
            Class[] paramTypes = new Class[] { Integer.Type };
            Method m = helpClass.GetMethod("createRfcommSocket", paramTypes);
            Java.Lang.Object[] param = new Java.Lang.Object[] { Integer.ValueOf(1) };

            mSocket = (BluetoothSocket)m.Invoke(tmp.RemoteDevice, param);

            mAdapter.CancelDiscovery();
            try
            {
                if (mSocket.IsConnected == false)
                {
                    mSocket.Connect();
                    mIsConnected = true;
                }
            }
            catch (System.Exception sex)
            {
                mIsConnected = false;
                mIsConnectionFailed = true;
                Cancel();
                Log.Debug("SocketConnection", "Connection could not be created");
                return;
            }
            mDataOutputStream = new DataOutputStream(mSocket.OutputStream);
            mDataInputStream = new DataInputStream(mSocket.InputStream);
        }*/

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
            catch(System.Exception sex)
            {
                Log.Debug("SocketConnection", "Error while sending data");
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

            //string str = string.Format("Speed: {0} HeightControl: {1} Azimuth: {2} Pitch: {3} Roll: {4}", speed,
            //        heightcontrol, azimuth, pitch, roll);
            //Console.WriteLine(str);
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
                mSocket.Close();
                if (mDataOutputStream != null)
                {
                    mDataOutputStream.Close();
                }
            }catch(System.Exception ex)
            {

            }
        }
    }
}