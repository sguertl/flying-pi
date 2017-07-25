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
using Android.Content.PM;
using Java.Util;

namespace WiFiDronection
{
    [Activity(Label = "HelpActivity", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen")]
    public class HelpActivity : Activity
    {

        private TextView mTvHeaderHelp;
        private TextView mTvHelpAbout;
        private TextView mTvVersion;
        private TextView mTvCredentials;
        private TextView mTvAboutInfo;
        private TextView mTvLinkHomepage;
        private Button mBtnBackHelp;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Help);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            mTvHeaderHelp = FindViewById<TextView>(Resource.Id.tvHeaderHelp);
            mTvHelpAbout = FindViewById<TextView>(Resource.Id.tvHelpAbout);
            mTvVersion = FindViewById<TextView>(Resource.Id.tvVersion);
            mTvCredentials = FindViewById<TextView>(Resource.Id.tvCredentials);
            mTvAboutInfo = FindViewById<TextView>(Resource.Id.tvAboutInfo);
            mTvLinkHomepage = FindViewById<TextView>(Resource.Id.tvLinkHomepage);
            mBtnBackHelp = FindViewById<Button>(Resource.Id.btnBackHelp);

            mTvHeaderHelp.Typeface = font;
            mTvHelpAbout.Typeface = font;
            mTvVersion.Typeface = font;
            mTvCredentials.Typeface = font;
            mTvAboutInfo.Typeface = font;
            mTvLinkHomepage.Typeface = font;
            mBtnBackHelp.Typeface = font;

            mBtnBackHelp.Click += OnBackToMain;

            PackageManager manager = this.PackageManager;
            PackageInfo info = manager.GetPackageInfo(this.PackageName, 0);
            mTvVersion.Text = 
                "Version: " + info.VersionName + 
                "\nFirst install time: " + new DateTime(info.FirstInstallTime).ToShortDateString() +
                "\nLast Update Time: " + new DateTime(info.LastUpdateTime).ToShortDateString() + 
                "\nPackage Name: " + info.PackageName;          

        }

        private void OnBackToMain(object sender, EventArgs e)
        {
            Finish();
        }
    }
}