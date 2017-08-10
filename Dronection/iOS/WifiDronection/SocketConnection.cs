﻿using System;
using Sockets.Plugin;

namespace WiFiDronection
{
    public class SocketConnection
    {

		// Debug variable
		private static readonly string TAG = "SocketConnection";

		// Constants
		private readonly string SERVER_ADDRESS = "172.24.1.1";
		private readonly int SERVER_PORT = 5050;
		private readonly byte START_BYTE = 0x00;
		private readonly int PACKET_SIZE = 19;

		// Singleton members
		private static SocketConnection instance = null;
		private static readonly object padlock = new object();

		// Output stream members
		private string mLogData;
		private long mStartMillis;

        public TcpSocketClient mSocket;

		// Thread for connecting
		//public Thread mConnectionThread;

		// Boolean to check if connected
		public bool IsConnected
        {
            get { return mSocket.Socket.Connected; }
		}

		/// <summary>
		/// Private Singleton constructor.
		/// </summary>
		private SocketConnection()
		{
			Init();
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

        private void Init()
        {
            try
            {
                mSocket.ConnectAsync(SERVER_ADDRESS, SERVER_PORT);
            }
            catch(Exception ex)
            {
				mSocket.DisconnectAsync();
			}

			// we're connected!
        }

		/// <summary>
		/// Writes controller data to smartphone through socket connection
		/// </summary>
		/// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
		public void Write(params Int16[] args)
		{
			// Save controls for log file
            mLogData += mStartMillis + "," + args[0] + "," + args[1] + "," + args[2] + "," + args[3] + "," + (ControllerSettings.Instance.AltitudeControlActivated ? 1 : 0) + "\n";

			mStartMillis += 10;

			// Convert int16 controller parameters to byte stream
			byte[] bytes = ConvertToByte(args);

			try
			{
                mSocket.WriteStream.Write(bytes, 0, 19);
                mSocket.WriteStream.Flush();
			}
            catch (Exception ex)
			{
				Console.WriteLine("Error sending data");
                mSocket.DisconnectAsync();
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
            byte heightcontrol = (byte)(ControllerSettings.Instance.AltitudeControlActivated == true ? 1 : 0);
			// int azimuth = Java.Lang.Float.FloatToIntBits(args[1]);
			// int pitch = Java.Lang.Float.FloatToIntBits(args[2]);
			// int roll = Java.Lang.Float.FloatToIntBits(args[3]);

            int azimuth = BitConverter.ToInt32(BitConverter.GetBytes(args[1]), 0);
            int pitch = BitConverter.ToInt32(BitConverter.GetBytes(args[2]), 0);
            int roll = BitConverter.ToInt32(BitConverter.GetBytes(args[3]), 0);

            Console.WriteLine("{0}, {1}, {2}, {3}", args[0], args[1], args[2], args[3]);

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

        public void Cancel()
        {
            if(mSocket != null)
            {
				mSocket.DisconnectAsync();
			}
		}
    }
}
