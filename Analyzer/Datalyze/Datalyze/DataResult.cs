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
    public class DataResult
    {
        private int mBytes;
        private int mRepetitions;
        private int mDelay;
        private List<Data> mResults;

        public List<Data> Results
        {
            get { return mResults; }
        }

        public DataResult(int bytes, int repetitions, int delay)
        {
            mBytes = bytes;
            mRepetitions = repetitions;
            mDelay = delay;
        }

        public void SetWifiResults(string[] parts)
        {
            mResults = parts.ToList().ConvertAll(new Converter<string, Data>(ConvertToData));
        }

        private Data ConvertToData(string str)
        {
            string[] parts = str.Split(';');
            return new Data
            {
                IsCorrect = Convert.ToByte(parts[0]),
                TimeDif = Convert.ToInt16(parts[1]),
                DataRate = Convert.ToSingle(parts[2]) / 100
            };
        }

        public int GetCorrectnessPercentage()
        {
            return mResults.Count(wd => wd.IsCorrect == 1) * 100 / mRepetitions;
        }

        public double GetDataRate()
        {
<<<<<<< HEAD:Analyzer/Datalyze/Datalyze/WifiDataResult.cs
            float time = mWifiResults.Sum(wd => wd.TimeDif) - mWifiResults[0].TimeDif;
            return Math.Round((((mRepetitions * mBytes) * GetCorrectnessPercentage() / 100) / time * 1000), 2);
=======
            float time = mResults.Sum(wd => wd.TimeDif) - mResults[0].TimeDif;
            return Math.Round((((mRepetitions * mBytes) * GetCorrectnessPercentage() / 100) / time), 2);
>>>>>>> 9cf43c3218877adb9dec7311eb9ad0775f703a0e:Analyzer/Datalyze/Datalyze/DataResult.cs
        }

        public double GetAverageTimeDif()
        {
            return Math.Round(mResults.GetRange(0, mResults.Count - 1).Average(wd => Math.Abs(mDelay - wd.TimeDif)), 2);
        }

        public void Write()
        {
            DateTime time = DateTime.Now;
            string fileName = string.Format("{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
            var logWriter = new Java.IO.FileWriter(new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "wifi", fileName + ".csv"));
            string title = $"Log from {fileName}\nBytes: {mBytes}\nRepetitions: {mRepetitions}\nDelay: {mDelay}\n\n";
            logWriter.Write(title);
            logWriter.Flush();
            foreach (Data wd in mResults)
            {
                logWriter.Write(wd.ToString());
                logWriter.Flush();
            }
            logWriter.Close();
        }
    }

    public class Data
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