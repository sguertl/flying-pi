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
using Android.Util;
using Java.Net;
using Java.IO;

namespace WiFiDronection
{
    public class SocketConnection : Thread
    {
        private readonly string TAG = "SocketConnection";

        public static bool FLAG = true;
        public static Socket m_Socket;

        private static readonly string SERVER_ADDRESS = "172.24.1.1";
        private static readonly int SERVERPORT = 5050;
        private static readonly byte STARTBYTE = 10;
        private static readonly int PACKET_SIZE = 19;

        private DataOutputStream mDataOutputStream;

        public SocketConnection()
        {
            m_Socket = new Socket();
        }

        public override void Run()
        {
            try
            {
                m_Socket = new Socket(SERVER_ADDRESS, SERVERPORT);
            }
            catch (UnknownHostException uhe)
            {
                Log.Debug(TAG, uhe.Message + " if the IP address of the host could not be determined.");
            }
            catch (IOException uhe)
            {
                Log.Debug(TAG, uhe.Message + " if an I/O error occurs when creating the socket.");
            }
            catch (SecurityException uhe)
            {
                Log.Debug(TAG, uhe.Message + " if a security manager exists and its checkConnect method doesn't allow the operation.");
            }
            catch (IllegalAccessException uhe)
            {
                Log.Debug(TAG, uhe.Message + " if the port parameter is outside the specified range of valid port values, which is between 0 and 65535, inclusive.");
            }

            try
            {
                if (!m_Socket.IsConnected)
                {
                    SocketAddress socketAdr = new InetSocketAddress(SERVER_ADDRESS, SERVERPORT);
                    Thread.Sleep(5000);
                    m_Socket.Connect(socketAdr, 2000);
                    mDataOutputStream = new DataOutputStream(m_Socket.OutputStream);
                }
            }
            catch (Java.Lang.Exception ex)
            {
                FLAG = false;
                Log.Debug(TAG, ex.Message);
                return;
            }
        }

        public void Write(params Int16[] args)
        {
            byte[] bytes = ConvertToByte(args);
            try
            {
                mDataOutputStream.Write(bytes);
                mDataOutputStream.Flush();
            }
            catch(Java.Lang.Exception ex)
            {
                Log.Debug(TAG, "Fehler bei der Übertragung");
                mDataOutputStream.Close();
                m_Socket.Close();
            }
        }

        private byte[] ConvertToByte(params Int16[] args)
        {
            byte[] b = new byte[PACKET_SIZE];

            byte speed = (byte) args[0];
            byte heightcontrol = 0;
            int azimuth = Java.Lang.Float.FloatToIntBits(args[1]);
            int pitch = Java.Lang.Float.FloatToIntBits(args[2]);
            int roll = Java.Lang.Float.FloatToIntBits(args[3]);

            int checksum = STARTBYTE;
            checksum ^= (heightcontrol << 8 | speed) & 0xFFFF;
            checksum ^= azimuth;
            checksum ^= pitch;
            checksum ^= roll;

            b[0] = STARTBYTE;

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

    }
}