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

namespace BTDronection
{
   public class CurrentVisualizationData
    {

        private Dictionary<string, List<DataPoint>> mPoints;

        public Dictionary<string, List<DataPoint>> Points
        {
            get { return mPoints; }
            set { mPoints = value; }
        }

        private List<float> mAltControlTime;

        public List<float> AltControlTime
        {
            get { return mAltControlTime; }
            set { mAltControlTime = value; }
        }


        private static CurrentVisualizationData instance = null;
        private static readonly object padlock = new object();

        private CurrentVisualizationData()
        {
            this.mPoints = new Dictionary<string, List<DataPoint>>();
            this.mAltControlTime = new List<float>();
        }

        public static CurrentVisualizationData Instance
        {
            get
            {
                lock (padlock)
                {
                    if(instance == null)
                    {
                        instance = new CurrentVisualizationData();
                    }
                    return instance;
                }
            }
        }

    }
}