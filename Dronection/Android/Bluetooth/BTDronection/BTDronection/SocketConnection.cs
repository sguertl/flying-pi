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
*  Created on: 2017-07-28                                               *
*  Author(s): Christoph Englert                                         *
*             Adrian Klapsch                                            *
*                                                                       *
*  SocketConnection is a Singleton class that establishes a socket      *
*  connection to the Raspberry.                                         *
*                                                                       *
************************************************************************/

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
using Java.IO;
using Java.Util;
using Android.Util;

namespace BTDronection
{
    public class SocketConnection
    {

        // Constants
        private readonly string BLUETOOTH_UUID_STRING = "00001101-0000-1000-8000-00805F9B34FB";

        // Output members
        private BluetoothSocket mSocket;
        private SocketWriter mSocketWriter;

        // Members
        private string mLogData;
        private int mStartMillis;
       
        // Properties
        public bool IsConnected
        {
            get { return mSocket.IsConnected; }
        }

        public string LogData
        {
            get { return mLogData; }
        }

        // Private constructor
        public SocketConnection()
        {
            // Do nothing
        }

        /// <summary>
        /// Connects the controller with bluetooth device.
        /// </summary>
        /// <param name="device">Bluetooth device</param>
        public void Connect(BluetoothDevice device)
        {
            try
            {
                BluetoothSocket tempBTSocket = null;
                try
                {
                    tempBTSocket = device.CreateRfcommSocketToServiceRecord(UUID.FromString(BLUETOOTH_UUID_STRING));
                    tempBTSocket.Connect();
                }
                catch (IOException ex)
                {
                    Log.Debug("SocketConnection", "Connection Error 1");
                }
                mSocket = tempBTSocket;
                mSocketWriter = new SocketWriter(tempBTSocket.OutputStream);
                byte[] bytes = new byte[10] { 10, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                mSocketWriter.Write(bytes);
            }
            catch (IOException ex)
            {
                Log.Debug("SocketConnection", "Connection Error 2");
            }
        }

        /// <summary>
        /// Writes the data to output writer.
        /// </summary>
        /// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
        public void Write(params Int16[] args)
        {
            if(mSocket.IsConnected == true)
            {
                mLogData += mStartMillis + "," + args[0] + "," + args[1] + "," + args[2] + "," + args[3] + "," + (ControllerView.Settings.AltitudeControlActivated ? 1 : 0) + "\n";
                mStartMillis += 10;
                mSocketWriter.Write(args);
            }
        }

        /// <summary>
        /// Closes, if necessary, everything related to the socket connection.
        /// </summary>
        public void Close()
        {
            if(mSocket != null && mSocket.IsConnected == true)
            {
                mSocketWriter.Close();
                mSocket.Close();
            }
        }
    }
}