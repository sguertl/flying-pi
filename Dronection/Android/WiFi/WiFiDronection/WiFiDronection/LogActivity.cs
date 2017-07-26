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

namespace WiFiDronection
{
    [Activity(Label = "LogActivity", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen")]
    public class LogActivity : Activity
    {
        private TextView mTvHeader;
        private ListView mLvFiles;
        private TextView mTvEmpty;
        private Button mBtBack;

        private ListAdapter mAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Log);

            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderLog);
            mLvFiles = FindViewById<ListView>(Resource.Id.lvFiles);
            mTvEmpty = FindViewById<TextView>(Resource.Id.tvEmpty);
            mBtBack = FindViewById<Button>(Resource.Id.btnBackLog);
            mBtBack.Click += OnBackToMain;

            Typeface font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            mTvHeader.Typeface = font;
            mTvEmpty.Typeface = font;
            mBtBack.Typeface = font;

            FillFilesList();
        }

        public override void OnContentChanged()
        {
            base.OnContentChanged();
            View empty = FindViewById<View>(Resource.Id.tvEmpty);
            FindViewById<ListView>(Resource.Id.lvFiles).EmptyView = empty;
        }

        private void FillFilesList()
        {
            var projectDir = new Java.IO.File(MainActivity.ApplicationFolderPath);
            List<string> fileNames = new List<string>();
            string[] fileArray = projectDir.List();
            if(fileArray != null)
            {
                fileNames = fileArray.ToList();
                fileNames.Sort(new MyComparer());
            }
            mAdapter = new ListAdapter(this, fileNames);
            mLvFiles.Adapter = mAdapter;
        }

        private void OnBackToMain(object sender, EventArgs e)
        {
            Finish();
        }
    }

    public class MyComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return y.CompareTo(x);
        }
    }
}