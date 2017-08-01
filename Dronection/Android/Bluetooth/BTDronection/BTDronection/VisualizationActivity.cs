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
    [Activity(Label = "VisualizationActivity", Theme = "@android:style/Theme.NoTitleBar.Fullscreen")]
    public class VisualizationActivity : Activity
    {

        private ListView mLvVisualizationData;

        private CurrentVisualizationData mCurVisData;

        private ListAdapter mAdapter;

        private String mFilename;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.VisualizationLayout);
            this.mFilename = Intent.GetStringExtra("filename");
            Init();
        }

        public void Init()
        {
            this.mLvVisualizationData = FindViewById<ListView>(Resource.Id.lvData);

            FillRawDataList();

            this.mLvVisualizationData.Adapter = mAdapter;
            this.mLvVisualizationData.DividerHeight = 14;
            this.mLvVisualizationData.ItemClick += OnListViewItemClick;

            this.mCurVisData = CurrentVisualizationData.Instance;
            this.mCurVisData.Points = new Dictionary<string, List<DataPoint>>();
            this.mCurVisData.AltControlTime = new List<float>();
        }

        private void FillRawDataList()
        {
            var projectDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mFilename);
            List<string> fileNames = new List<string>();
            string[] fileArray = projectDir.List();

            for (int i = 0; i < fileArray.Length; i++)
            {
                fileArray[i] = fileArray[i].Replace(".csv", "");
            }

            if(fileArray != null)
            {
                fileNames = fileArray.ToList();
            }

            mAdapter = new ListAdapter(this, fileNames);
        }

        private void OnListViewItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string title = mAdapter[e.Position];
            string path = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mFilename + Java.IO.File.Separator + title + ".csv";
            var reader = new Java.IO.BufferedReader(new Java.IO.FileReader(path));
            string line = "";

            if (title.Equals("controls"))
            {
                mCurVisData.Points.Add("throttle", new List<DataPoint>());
                mCurVisData.Points.Add("yaw", new List<DataPoint>());
                mCurVisData.Points.Add("pitch", new List<DataPoint>());
                mCurVisData.Points.Add("roll", new List<DataPoint>());
            }
            else
            {
                mCurVisData.Points.Add(title, new List<DataPoint>());
            }

            while ((line = reader.ReadLine()) != null)
            {
                String[] p = line.Split(',');
                if (title.Equals("controls"))
                {
                    float x = Convert.ToSingle(p[0]);
                    float t = Convert.ToSingle(p[1]);
                    float y = Convert.ToSingle(p[2]);
                    float p2 = Convert.ToSingle(p[3]);
                    float r = Convert.ToSingle(p[4]);
                    int h = Convert.ToInt32(p[5]);
                    mCurVisData.Points["throttle"].Add(new DataPoint(x, t));
                    mCurVisData.Points["yaw"].Add(new DataPoint(x, y));
                    mCurVisData.Points["pitch"].Add(new DataPoint(x, p2));
                    mCurVisData.Points["roll"].Add(new DataPoint(x, r));
                    if(h == 1)
                    {
                        mCurVisData.AltControlTime.Add(x);
                    }
                }
                else
                {
                    float x = Convert.ToSingle(p[0]);
                    float y = Convert.ToSingle(p[1]);
                    int h = Convert.ToInt32(p[2]);

                    mCurVisData.Points[title].Add(new DataPoint(x, y));

                    if(h == 1)
                    {
                        mCurVisData.AltControlTime.Add(x);
                    }
                }
            }
            reader.Close();
            StartActivity(typeof(ShowVisualizationDataActivity));
        }
    }
}