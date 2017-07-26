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
        private TextView mTvHelpStartScreen;
        private TextView mTvHelpStartScreenText;
        private TextView mTvHelpControllerSettings;
        private TextView mTvHelpControllerSettingsText;
        private TextView mTvHelpController;
        private TextView mTvHelpControllerText;
        private TextView mTvHelpLogFiles;
        private TextView mTvHelpLogFilesText;
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
            mTvHelpStartScreen = FindViewById<TextView>(Resource.Id.tvHelpStartScreen);
            mTvHelpStartScreenText = FindViewById<TextView>(Resource.Id.tvHelpTextStartScreen);
            mTvHelpControllerSettings = FindViewById<TextView>(Resource.Id.tvHelpControllerSettings);
            mTvHelpControllerSettingsText = FindViewById<TextView>(Resource.Id.tvHelpTextControllerSettings);
            mTvHelpController = FindViewById<TextView>(Resource.Id.tvHelpController);
            mTvHelpControllerText = FindViewById<TextView>(Resource.Id.tvHelpTextController);
            mTvHelpLogFiles = FindViewById<TextView>(Resource.Id.tvHelpLogFiles);
            mTvHelpLogFilesText = FindViewById<TextView>(Resource.Id.tvHelpTextLogFiles);
            mTvHelpAbout = FindViewById<TextView>(Resource.Id.tvHelpAbout);
            mTvVersion = FindViewById<TextView>(Resource.Id.tvVersion);
            mTvCredentials = FindViewById<TextView>(Resource.Id.tvCredentials);
            mTvAboutInfo = FindViewById<TextView>(Resource.Id.tvAboutInfo);
            mTvLinkHomepage = FindViewById<TextView>(Resource.Id.tvLinkHomepage);
            mBtnBackHelp = FindViewById<Button>(Resource.Id.btnBackHelp);

            mTvHeaderHelp.Typeface = font;
            mTvHelpStartScreen.Typeface = font;
            mTvHelpStartScreenText.Typeface = font;
            mTvHelpControllerSettings.Typeface = font;
            mTvHelpControllerSettingsText.Typeface = font;
            mTvHelpController.Typeface = font;
            mTvHelpControllerText.Typeface = font;
            mTvHelpLogFiles.Typeface = font;
            mTvHelpLogFilesText.Typeface = font;
            mTvHelpAbout.Typeface = font;
            mTvVersion.Typeface = font;
            mTvCredentials.Typeface = font;
            mTvAboutInfo.Typeface = font;
            mTvLinkHomepage.Typeface = font;
            mBtnBackHelp.Typeface = font;

            mBtnBackHelp.Click += OnBackToMain;

            PackageManager manager = this.PackageManager;
            PackageInfo info = manager.GetPackageInfo(this.PackageName, 0);
            Date fit = new Date(info.FirstInstallTime);
            Date lut = new Date(info.LastUpdateTime);
            DateTime firstInstall = new DateTime(fit.Year + 1900, fit.Month +1 , fit.Day);
            DateTime lastUpdate = new DateTime(lut.Year + 1900, lut.Month + 1, lut.Day);
            mTvVersion.Text = String.Format(
                "Version: {0}\nFirst install time: {1:yyyy-MM-dd}\nLast Update Time: {2:yyyy-MM-dd}\nPackage Name: {3}", 
                info.VersionName, firstInstall, lastUpdate, info.PackageName);          

        }

        private void OnBackToMain(object sender, EventArgs e)
        {
            Finish();
        }
    }
}