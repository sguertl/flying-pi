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
using Android.Text;

namespace WiFiDronection
{
    [Activity(Label = "HelpActivity", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen")]
    public class HelpActivity : Activity
    {
        // Members
        private TextView mTvHeaderHelp;
        private TextView mTvHelpText;
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
        private TextView mTvLinkGithub;
        private TextView mTvAboutInfo;
        private TextView mTvLinkHomepage;
        private TextView mTvThirdParty;
        private Button mBtnBackHelp;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Help);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            // Initialize widgets
            mTvHeaderHelp = FindViewById<TextView>(Resource.Id.tvHeaderHelp);
            mTvHelpText = FindViewById<TextView>(Resource.Id.tvHelpText);
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
            mTvLinkGithub = FindViewById<TextView>(Resource.Id.tvLinkGitHub);
            mTvAboutInfo = FindViewById<TextView>(Resource.Id.tvAboutInfo);
            mTvLinkHomepage = FindViewById<TextView>(Resource.Id.tvLinkHomepage);
            mTvThirdParty = FindViewById<TextView>(Resource.Id.tvThirdParty);
            mBtnBackHelp = FindViewById<Button>(Resource.Id.btnBackHelp);

            mTvHeaderHelp.Typeface = font;
            mTvHelpText.Typeface = font;
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
            mTvLinkGithub.Typeface = font;
            mTvAboutInfo.Typeface = font;
            mTvLinkHomepage.Typeface = font;
            mTvThirdParty.Typeface = font;
            mBtnBackHelp.Typeface = font;

            mBtnBackHelp.Click += OnBackToMain;


            mTvHelpStartScreenText.TextFormatted = Html.FromHtml(
                "If the user opens the app, a start screen with three buttons appears. The app searches automatically for WiFi hotspots containing the strings <i>Raspberry</i>, <i>RPI</i> or <i>Pi</i>. If the <i>Connect</i> button remains red, the app can’t find a matching WiFi hotspot containing the strings mentioned above. If an appropriate network is found, the <i>Connect</i> button turns blue and the SSID and MAC of the Raspberry Pi are displayed.<br/>" +
                "<br/><b>Connect</b><br/>Asks the user to enter the password (usually 87654321 or 00000000) and confirm it. After that, the app navigates to <i>Controller Settings.</i><br/>" +
                "<br/><b>Log Files</b><br/>Navigates to the <i>Log File Menu</i>, where the user can either see raw log files or visualizations of previous flights.<br/>" +
                "<br/><b>Help</b><br/>Navigates to the help menu, where general information is displayed.");

            mTvHelpControllerSettingsText.TextFormatted = Html.FromHtml(
                "In this menu the user can choose between the positions of the functionality during the flight.<br/>" +
                "<br/><b>Mode 1</b><br/>The left joystick is used to control Throttle (vertical) and Yaw (horizontal). The right joystick is used to control Pitch (vertical) and Roll (horizontal).<br/>" +
                "<br/><b>Mode 2</b><br/>The left joystick is used to control Pitch (vertical) and Yaw (horizontal). The right joystick is used to control Throttle (vertical) and Roll (horizontal).<br/>" +
                "<br/>A click on <i>Start</i> opens the controller.");

            mTvHelpControllerText.TextFormatted = Html.FromHtml("With the controller view you can control a multicopter using the two joysticks. Depending on the previous selection the user can start the multicopter by pushing up the throttle joystick. At the top of the screen the user can activate altitude control and adjust trims for yaw, pitch and roll. The trim can be changed either by moving the bar or by clicking the two volume buttons (+ and -) on the smartphone. Once a flight is finished, the user can go back to the <i>Controller Settings</i> menu by clicking the smartphone’s back button.");

            mTvHelpLogFilesText.TextFormatted = Html.FromHtml(
                "In this menu a list of all log files is displayed. After a flight, sent and received data is logged and saved in a file. With a click on a file, the user can choose between these three options:<br/>" +
                "<br/><b>Raw Data</b><br/>This option shows the raw data from the log file.<br/>" +
                "<br/><b>Visualize</b><br/>This option visualizes the logged data and generates one or more graphs.<br/>" +
                "<br/><b>Delete file</b><br/>Deletes the file permanently.");

            mTvThirdParty.TextFormatted = Html.FromHtml(
                "Wifi Dronection includes third-party components and we are very thankful to their authors.<br/><br/>" +
                "&#9679; <a href='https://github.com/PhilJay/MPAndroidChart'>MPAndroidChart</a> by PhilJay"
                );

            PackageManager manager = this.PackageManager;
            PackageInfo info = manager.GetPackageInfo(this.PackageName, 0);
            /*Date fit = new Date(info.FirstInstallTime);
            Date lut = new Date(info.LastUpdateTime);
            DateTime firstInstall = new DateTime(fit.Year + 1900, fit.Month +1 , fit.Day);
            DateTime lastUpdate = new DateTime(lut.Year + 1900, lut.Month + 1, lut.Day);*/
            mTvVersion.Text = String.Format(
                "Version: {0}",//\nFirst install time: {1:yyyy-MM-dd}\nLast Update Time: {2:yyyy-MM-dd}\nPackage Name: {3}",
                info.VersionName);//, firstInstall, lastUpdate, info.PackageName);          

        }

        /// <summary>
        /// Go back to MainActivity
        /// </summary>
        private void OnBackToMain(object sender, EventArgs e)
        {
            Finish();
        }
    }
}