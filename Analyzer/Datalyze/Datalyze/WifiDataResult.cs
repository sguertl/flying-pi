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
using Android.Media;

namespace Datalyze
{
    public class WifiDataResult
    {
        private int mBytes;
        private int mRepetitions;
        private int mDelay;
        private List<WifiData> mWifiResults;

        public List<WifiData> WifiResults
        {
            get { return mWifiResults; }
        }


        public WifiDataResult(int bytes, int repetitions, int delay)
        {
            mBytes = bytes;
            mRepetitions = repetitions;
            mDelay = delay;
        }

        public void SetWifiResults(string[] parts)
        {
            mWifiResults = parts.ToList().ConvertAll(new Converter<string, WifiData>(ConvertToWifiData));
        }

        private WifiData ConvertToWifiData(string str)
        {
            string[] parts = str.Split(';');
            return new WifiData
            {
                IsCorrect = Convert.ToByte(parts[0]),
                TimeDif = Convert.ToInt16(parts[1]),
                DataRate = Convert.ToSingle(parts[2]) / 100
            };
        }

        public int GetCorrectnessPercentage()
        {
            return mWifiResults.Count(wd => wd.IsCorrect == 1) * 100 / mWifiResults.Count;
        }

        public double GetDataRate()
        {
            float time = mWifiResults.Sum(wd => wd.TimeDif) - mWifiResults[0].TimeDif;
            return Math.Round((float)((mRepetitions * mBytes) / time), 2);
        }

        public double GetAverageTimeDif()
        {
            return Math.Round(mWifiResults.Average(wd => (wd.TimeDif - mDelay)), 2);
        }

        public void Write()
        {
            DateTime time = DateTime.Now;
            string fileName = string.Format("{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
            var logWriter = new Java.IO.FileWriter(new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "wifi", fileName + ".csv"));
            string title = $"Log from {fileName}\nBytes: {mBytes}\nRepetitions: {mRepetitions}\nDelay: {mDelay}\n\n";
            logWriter.Write(title);
            logWriter.Flush();
            foreach (WifiData wd in mWifiResults)
            {
                logWriter.Write(wd.ToString());
                logWriter.Flush();
            }
            logWriter.Close();
            MediaScannerConnection.ScanFile(Application.Context, new string[] { MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "wifi", fileName + ".csv" }, null, null);
        }
    }

    public class WifiData
    {
        public byte IsCorrect { get; set; }
        public int TimeDif { get; set; }
        public float DataRate { get; set; }

        public override string ToString()
        {
            return $"{IsCorrect};{TimeDif};{DataRate}\n";
        }
    }
}