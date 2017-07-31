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
    public class SocketConnection 
    {
        // Debug variable
        private static readonly string TAG = "SocketConnection";

        // Constants
        private readonly string SERVER_ADDRESS = "172.24.1.1";
        private readonly int SERVERPORT = 5050;
        private readonly byte STARTBYTE = 10;
        private readonly int PACKET_SIZE = 19;

        // Singleton variables
        private static SocketConnection instance = null;
        private static readonly object padlock = new object();

        // Members
        private DataOutputStream mDataOutputStream;
        private Socket m_Socket;
        private string mLogData;
        private long mStartMillis;

        // Public Members
        public Thread m_ConnectionThread;



        /// <summary>
        /// Saves the flying parameters
        /// </summary>
        public string LogData
        {
            get { return mLogData; }
            set { mLogData = value; }
        }

        public bool isConnected { get; set; }

        public Socket WifiSocket
        {
            get { return m_Socket; }
        }

        private DataInputStream mDataInputStream;

        public DataInputStream InputStream
        {
            get { return mDataInputStream; }
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        private SocketConnection()
        {
            Init();
        }

        public static SocketConnection Instance
        {
            get
            {
                lock (padlock)
                {
                    if(instance == null)
                    {
                        instance = new SocketConnection();
                    }
                    return instance;
                }
            }
        }

        public bool IsSocketConnected
        {
            get { return m_Socket.IsConnected; }
        }


        public void Init()
        {
            try
            {
                this.m_ConnectionThread = new Thread(OnConnecting);
                mStartMillis = 0;
                this.OnCancel();
                this.m_Socket = new Socket();
            }
            catch (Java.Lang.Exception ex)
            {
                Log.Debug(TAG, ex.Message);
            }
        }

        public void OnStartConnection()
        {
            Init();

            m_ConnectionThread.Start();
            m_ConnectionThread.Join();

        }

        /// <summary>
        /// Try to connect the Socket to a specific SSID and HOST-PORT
        /// </summary>
        public void OnConnecting()
        {
            if (m_Socket.IsConnected == false)
            {
                try
                {
                    // Connect with socket
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
                        // if first connection attempt fails try again
                        SocketAddress socketAdr = new InetSocketAddress(SERVER_ADDRESS, SERVERPORT);
                        Thread.Sleep(5000);
                        m_Socket.Connect(socketAdr, 2000);
                    }
                }
                catch (Java.Lang.Exception ex)
                {
                    Log.Debug(TAG, ex.Message);
              
                }
                finally
                {
                    if (m_Socket.IsConnected)
                    {
                        // Create socket reading and writing streams
                        mDataOutputStream = new DataOutputStream(m_Socket.OutputStream);
                        mDataInputStream = new DataInputStream(m_Socket.InputStream);
                    }

                }
            }
           // return;
        }

        /// <summary>
        /// Connection thread
        /// </summary>
       /* public override void Run()
        {
            FLAG = true;
            if (m_Socket.IsConnected == false)
            {
                try
                {
                    // Connect with socket
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
                        // if first connection attempt fails try again
                        SocketAddress socketAdr = new InetSocketAddress(SERVER_ADDRESS, SERVERPORT);
                        Thread.Sleep(5000);
                        m_Socket.Connect(socketAdr, 2000);
                    }
                }
                catch (Java.Lang.Exception ex)
                {
                    FLAG = false;
                    Log.Debug(TAG, ex.Message);
                    return;
                }
                finally
                {
                    if (FLAG)
                    {
                        // Create socket reading and writing streams
                        mDataOutputStream = new DataOutputStream(m_Socket.OutputStream);
                        mDataInputStream = new DataInputStream(m_Socket.InputStream);
                    }
                   
                }
            }

        }*/

        /// <summary>
        /// Writes controller data to smartphone through socket connection
        /// </summary>
        /// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
        public void Write(params Int16[] args)
        {
            // Save controlls for log file
            mLogData += mStartMillis + "," + args[0] + "," + args[1] + "," + args[2] + "," + args[3] + "," + (ControllerView.Settings.AltitudeControlActivated ? 1 : 0) + "\n";

            mStartMillis += 10;
            // Convert int16 controller parameters to byte stream
            byte[] bytes = ConvertToByte(args);
            try
            {
                mDataOutputStream.Write(bytes);
                mDataOutputStream.Flush();
            }
            catch(Java.Lang.Exception ex)
            {
                Log.Debug(TAG, "Error sending data");
                mDataOutputStream.Close();
                m_Socket.Close();
            }
        }

        /// <summary>
        /// Convert int16 controller parameters to byte stream
        /// </summary>
        /// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
        /// <returns>byte stream</returns>
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

        /// <summary>
        /// Close connections
        /// </summary>
        public void OnCancel()
        {
            try
            {
                this.m_ConnectionThread = null;

                if (mDataOutputStream != null)
                {
                    mDataOutputStream.Close();
                }
                if (m_Socket != null)
                {
                    m_Socket.Close();
                }
            }catch(Java.Lang.Exception ex)
            {
                Log.Debug(TAG, "Failed closing");
            }
        }
    }
}