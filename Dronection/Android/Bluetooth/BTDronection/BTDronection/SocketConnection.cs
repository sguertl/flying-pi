/************************************************************************
*                                                                       *
*  Copyright (C) 2017 Infineon Technologies Austria AG.                 *
*                                                                       *
*  Licensed under the Apache License, Version 2.0 (the "License");      *
*  you may not use this file except in compliance with the License.     *
*  You may obtain a copy of the License at                              *
*                                                                       *
*    http://www.apache.org/licenses/LICENSE-2.0                         *
*                                                                       *
*  Unless required by applicable law or agreed to in writing, software  *
*  distributed under the License is distributed on an "AS IS" BASIS,    *
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or      *
*  implied.                                                             *
*  See the License for the specific language governing                  *
*  permissions and limitations under the License.                       *
*                                                                       *
*                                                                       *
*  File: SocketConnection.cs                                            *
*  Created on: 2017-07-19                                               *
*  Author(s): Klapsch Adrian Vasile (IFAT PMM TI COP)                   *
*             Englert Christoph (IFAT PMM TI COP)                       *
*                                                                       *
*  SocketConnection is a Singleton class that establishes a socket      *
*  connection to the Raspberry.                                         *
*                                                                       *
************************************************************************/

using System;

using Java.Lang;
using Android.Bluetooth;
using Java.Lang.Reflect;
using Android.Util;
using Java.IO;


namespace BTDronection
{
    public class SocketConnection
    {

		// Debug variable
		private static readonly string TAG = "SocketConnection";

        // Constants
        private readonly byte START_BYTE = 0x00;
        private readonly int PACKET_SIZE = 19;

		// Singleton members
		private static SocketConnection instance = null;
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
		/// Returns instance of SocketConnection.
		/// </summary>
		/// <value>Instance of SocketConnection</value>
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

		/// <summary>
		/// Initizalizes the socket connection.
		/// </summary>
		private void Init()
        {
            mAdapter = BluetoothAdapter.DefaultAdapter;
            mStartMillis = 0;

            this.mConnectionThread = new Thread(BuildConnection);
        }

		/// <summary>
		/// Initizalizes the socket connection.
		/// </summary>
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
		/// Writes controller data to smartphone through socket connection
		/// </summary>
		/// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
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

		/// <summary>
		/// Converts int16 controller parameters to byte stream
		/// </summary>
		/// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
		/// <returns>Byte stream</returns>
		private byte[] ConvertToByte(params Int16[] args)
        {
            byte[] b = new byte[PACKET_SIZE];
            byte speed = (byte)args[0];
            byte heightcontrol = (byte)(ControllerView.Settings.AltitudeControlActivated == true ? 1 : 0);
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
		/// Closes connections.
		/// </summary>
		public void Cancel()
        {
            try
            {

                if (ControllerView.Settings != null)
                {
                    ControllerView.Settings.AltitudeControlActivated = false;
                }

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