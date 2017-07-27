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
using Android.Graphics;

namespace BTDronection
{
    [Activity(Label = "RawDataActivity", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen")]
    public class RawDataActivity : Activity
    {
        // Members
        private TextView mTvHeader;
        private ListView mLvRawData;
        private TextView mTvDisplayRawData;
        private Button mBtBack;

        private ListAdapter mAdapter;
        private string mSelectedFile;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RawDataLayout);

            // Initialize widgets
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderRawData);
            mLvRawData = FindViewById<ListView>(Resource.Id.lvRawData);
            mLvRawData.ItemClick += OnListItemClick;
            mTvDisplayRawData = FindViewById<TextView>(Resource.Id.tvDisplayRawData);
            mBtBack = FindViewById<Button>(Resource.Id.btnBackRawData);
            mBtBack.Click += OnBack;

            Typeface font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
            mTvHeader.Typeface = font;
            mBtBack.Typeface = font;

            // Get selected filename from 
            mSelectedFile = Intent.GetStringExtra("filename");

            FillRawDataList();
        }

        /// <summary>
        /// Fills list with the control types
        /// </summary>
        private void FillRawDataList()
        {
            var projectDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mSelectedFile);
            List<string> fileNames = new List<string>();
            string[] fileArray = projectDir.List();
            if (fileArray != null)
            {
                fileNames = fileArray.ToList();
            }
            mAdapter = new ListAdapter(this, fileNames);
            mLvRawData.Adapter = mAdapter;
        }

        /// <summary>
        /// Onclick event for list item
        /// Reads the raw data from a .csv file and diplays it on textview
        /// </summary>
        private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string fileName = e.View.FindViewById<TextView>(Resource.Id.tvListItem).Text;
            string path = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mSelectedFile + Java.IO.File.Separator + fileName;
            var reader = new Java.IO.BufferedReader(new Java.IO.FileReader(path));
            string line = "";
            string finalText = "";
            while((line = reader.ReadLine()) != null)
            {
                line = line.Replace(',', ' ');
                finalText += line + "\n";
            }
            mTvDisplayRawData.Text = finalText;
            reader.Close();
        }

        /// <summary>
        /// Onclick event for back button
        /// </summary>
        private void OnBack(object sender, EventArgs e)
        {
            StartActivity(typeof(LogActivity));
        }
    }
}