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
using Java.Util;
using Java.Lang.Reflect;

namespace BTDronection
{
    public class ConnectedThread : Thread
    {
        // Constants
        //private readonly int SUCCESS_CONNECT = 0;
        private readonly UUID MY_UUID;

        // Members
        private BluetoothAdapter m_BtAdapter;
        public static BluetoothSocket m_Socket;
        private BluetoothDevice m_Device;            
        private string m_UuidString;
        //private Sender m_Sender;
        private PairedDevices m_PairedDevices;
        public static bool m_FailedCon;

        public ConnectedThread(BluetoothDevice device, string UUIDString)
        { 
            // Initializing objects
            m_BtAdapter = BluetoothAdapter.DefaultAdapter;
            m_UuidString = UUIDString;
            m_FailedCon = false;

            // Converting the UUID string into a UUID object
            MY_UUID = UUID.FromString(m_UuidString); // Wandelt den UUID String in ein UUID Objekt um

            // Use a temporary object that is later assigned to m_Socket
            BluetoothSocket tmp = null;
            m_Device = device;

            // Get a BluetoothSocket to connect with the given BluetoothDevice      
            // Workaround to get the Bluetoothsocket
            tmp = device.CreateRfcommSocketToServiceRecord(MY_UUID);
            Class testClass = tmp.RemoteDevice.Class;
            Class[] paramTypes = new Class[] { Integer.Type };

            Method m = testClass.GetMethod("createRfcommSocket", paramTypes);
            Java.Lang.Object[] param = new Java.Lang.Object[] { Integer.ValueOf(1) };

            m_Socket = (BluetoothSocket)m.Invoke(tmp.RemoteDevice, param);
        }

        public ConnectedThread(BluetoothDevice device, string UUIDString, PairedDevices pairedDevices)
            :this(device, UUIDString)
        {
            m_PairedDevices = pairedDevices;
        }

        /// <summary>
        /// Tries to connect to a specific device
        /// </summary>
        //public override void Run()
        public override void Run()
        {
            // Cancels the discovery due to better performance
            m_BtAdapter.CancelDiscovery();
            try
            {
                // Checks if device is already connected
                if (!m_Socket.IsConnected)
                {
                    m_Socket.Connect();
                }
            }
            catch (Java.Lang.Exception connectException)
            {
                m_FailedCon = true;
                // Could not connect to device
                Console.WriteLine(connectException.Message);
                try
                {
                    m_Socket.Close();
                }
                catch (Java.Lang.Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return;
            } 
        }

        /// <summary>
        /// Cancels an in-progress connection and closes the socket
        /// </summary>
        public static void Cancel()
        {
            try { m_Socket.Close(); }
            catch (Java.Lang.Exception e) { Console.WriteLine(e.Message); }
        }
    }
}