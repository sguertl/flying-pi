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
using Java.IO;
using System.IO;
using Android.Util;

namespace BTDronection
{
    public class SocketWriter
    {
        private DataOutputStream mDataOutputStream;

        private readonly int PACKET_SIZE = 19;
        private readonly byte START_BYTE = 0x00;

        public SocketWriter(Stream outputStream)
        {
            mDataOutputStream = new DataOutputStream(outputStream);
        }

        public void Write(params Int16[] args)
        {
            byte[] bytes = ConvertToByte(args);

            try
            {
                mDataOutputStream.Write(bytes);
                mDataOutputStream.Flush();
            }
            catch (Exception ex)
            {
                Log.Debug("BTSocketWriter", "Error while sending data");
            }
        }

        public void Write(params byte[] args)
        {
            try
            {
                mDataOutputStream.Write(args);
                mDataOutputStream.Flush();
            }
            catch (Exception ex)
            {
                Log.Debug("BTSocketWriter", "Error while sending data");
            }
        }

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

        public void Close()
        {
            if(mDataOutputStream != null)
            {
                mDataOutputStream.Close();
            }
        }
    }
}