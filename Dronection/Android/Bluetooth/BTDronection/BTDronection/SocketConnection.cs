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
        private BluetoothSocket mSocket;
        private SocketWriter mSocketWriter;
        private string mLogData;
        private int mStartMillis;

        private readonly string BLUETOOTH_UUID_STRING = "00001101-0000-1000-8000-00805F9B34FB";

        public bool IsConnected
        {
            get { return mSocket.IsConnected; }
        }

        public string LogData
        {
            get { return mLogData; }
        }

        public SocketConnection()
        {
            //
        }

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

        public void Write(params Int16[] args)
        {
            if(mSocket.IsConnected == true)
            {
                mLogData += mStartMillis + "," + args[0] + "," + args[1] + "," + args[2] + "," + args[3] + "," + (ControllerView.Settings.AltitudeControlActivated ? 1 : 0) + "\n";
                mStartMillis += 10;
                mSocketWriter.Write(args);
            }
        }

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