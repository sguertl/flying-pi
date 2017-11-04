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
using Android.Util;
using Java.Lang;
using Java.IO;
using Android.Bluetooth;
using Java.Lang.Reflect;

namespace Datalyze
{
   public class SocketConnectionBT
    {

        // Debug variable
        private static readonly string TAG = "SocketConnection";

        // Constants
        private readonly byte START_BYTE = 0x00;
        private readonly int PACKET_SIZE = 19;

        // Singleton members
        private static SocketConnectionBT instance = null;
        private static readonly object padlock = new object();

        // Thread for connection
        public Thread mConnectionThread;


        // Input and output members
        private DataInputStream mDataInputStream;
        public DataInputStream InputStream
        {
            get { return mDataInputStream; }
        }

        private DataOutputStream mDataOutputStream;

        private string mLogData;
        public string LogData
        {
            get { return mLogData; }
        }

        private long mStartMillis;


        // Bluetooth members
        private BluetoothAdapter mAdapter;

        private BluetoothDevice mPeer;
        public BluetoothDevice Peer
        {
            get { return mPeer; }
            set { mPeer = value; }
        }

        private BluetoothSocket mSocket;
        public BluetoothSocket Socket
        {
            get { return mSocket; }
        }

        /// <summary>
        /// Returns an instance of SocketConnection.
        /// Calls private constructor if no instance is created yet.
        /// </summary>
        /// <value>Instance of SocketConnection</value>
        public static SocketConnectionBT Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SocketConnectionBT();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// Private singleton constructor
        /// Calls Init().
        /// </summary>
        private SocketConnectionBT()
        {
            Init();
        }

        /// <summary>
        /// Initizalizes the socket connection and connection thread.
        /// Resets wifi socket, connection thread and output streams.
        /// </summary>
        private void Init()
        {
            mAdapter = BluetoothAdapter.DefaultAdapter;
            mStartMillis = 0;

            this.mConnectionThread = new Thread(BuildConnection);
        }

        /// <summary>
        /// Initizalizes the socket connection.
        /// Throws an exception if the device is not available.
        /// </summary>
        public void Init(BluetoothDevice device)
        {
            try
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
            catch (System.Exception ex)
            {
                throw new System.Exception();
            }
        }

        /// <summary>
        /// Starts the socket connection.
        /// Starts the socket thread.
        /// </summary>
        public void OnStartConnection()
        {
            if (mConnectionThread != null)
            {
                // Start the Connection (BuildConnection)
                mConnectionThread.Start();
                // Wait until the thread has finished
                mConnectionThread.Join();
            }

        }

        /// <summary>
        /// Establishes a bluetooth connection.
        /// Socket streams are created for reading and writing
        /// </summary>
        public void BuildConnection()
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
                Cancel();
            }

            if (mSocket.IsConnected)
            {
                mDataOutputStream = new DataOutputStream(mSocket.OutputStream);
                mDataInputStream = new DataInputStream(mSocket.InputStream);
            }

        }

        /// <summary>
        /// Writes controller data from the smartphone to the Raspberry or to the drone
        /// through the socket connection.
        /// Closes the socket if writing fails.
        /// </summary>
        /// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
        public void Write(params Int16[] args)
        {
            mLogData += mStartMillis + "," + args[0] + "," + args[1] + "," + args[2] + "," + args[3] + "," + (0) + "\n";
            Log.Debug(TAG, mLogData);
            mStartMillis += 10;
            byte[] bytes = ConvertToByte(args);
            try
            {
                mDataOutputStream.Write(bytes, 0, bytes.Length);
                mDataOutputStream.Flush();
            }
            catch (Java.Lang.Exception ex)
            {
                Log.Debug(TAG, "Error while sending data (" + ex.Message + ")");
                Cancel();
            }
        }

        /// <summary>
        /// Converts int16 controller parameters to byte stream
        /// </summary>
        /// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
        /// <returns>Byte stream</returns>
        private byte[] ConvertToByte(params Int16[] args)
        {
            byte[] b = new byte[PACKET_SIZE];
            byte speed = (byte)args[0];
            byte heightcontrol = (byte)(0);
            int azimuth = Java.Lang.Float.FloatToIntBits(args[1]);
            int pitch = Java.Lang.Float.FloatToIntBits(args[2]);
            int roll = Java.Lang.Float.FloatToIntBits(args[3]);

            int checksum = START_BYTE;
            checksum ^= (heightcontrol << 8 | speed) & 0xFFFF;
            checksum ^= azimuth;
            checksum ^= pitch;
            checksum ^= roll;

            b[0] = START_BYTE;

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

        /// <summary>
        /// Closes everything related to the socket connection.
        /// </summary>
        public void Cancel()
        {
            try
            {

              /*  if (ControllerView.Settings != null)
                {
                    ControllerView.Settings.AltitudeControlActivated = false;
                }
                */
                this.mConnectionThread = null;

                if (mDataOutputStream != null)
                {
                    mDataOutputStream.Close();
                }

                if (mSocket != null)
                {
                    mSocket.Close();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                Log.Debug(TAG, "Cancel Failed (" + ex.Message + ")");
            }
        }
    }
}