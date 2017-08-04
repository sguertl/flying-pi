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
   public class Flight
    {
        ControllerView cv;


        private static Flight instance = null;
        private static readonly object padlock = new object();

        private ControllerView mCV;

        public ControllerView CV
        {
            get { return mCV; }
            set { mCV = value; }
        }


        private Flight()
        {
        }

        public static Flight Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Flight();
                    }
                    return instance;
                }
            }
        }
    }
}