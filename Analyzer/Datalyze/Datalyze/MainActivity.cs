using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Util;

namespace Datalyze
{
    [Activity(Label = "Datalyze", MainLauncher = true, Icon = "@drawable/icon",
        Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        // Root path for project folder
        public static string ApplicationFolderPath;

        // Widgets
        private Button mBtnWifi;
        private LinearLayout mLinearLayout;
        private TextView mTvHeader;
        private Button mBtnBluetooth;
        private TextView mTvFooter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView (Resource.Layout.Main);

            // Initialize widgets
            mBtnWifi = FindViewById<Button>(Resource.Id.btnWifi);
            mLinearLayout = FindViewById<LinearLayout>(Resource.Id.linear);
            mBtnBluetooth = FindViewById<Button>(Resource.Id.btnBluetooth);
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeader);
            mTvFooter = FindViewById<TextView>(Resource.Id.tvFooter);

            // Set activity background
            mLinearLayout.SetBackgroundColor(Android.Graphics.Color.White);

            mBtnBluetooth.Click += delegate
            {
                StartActivity(typeof(BTConnectionActivity));
                //Log.Debug("!!!", "Go to BT");
            };
            
            mBtnWifi.Click += delegate
            {
                StartActivity(typeof(WifiConnectionActivity));
            };

            // Create and set font
            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
            mTvHeader.Typeface = font;
            mBtnWifi.Typeface = font;
            mTvFooter.Typeface = font;
            mBtnBluetooth.Typeface = font;

            //CreateApplicationFolder();
        }

        /// <summary>
        /// Creates the application folder for internal mobile storage.
        /// </summary>
        private void CreateApplicationFolder()
        {
            ApplicationFolderPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "airything");
            ApplicationFolderPath += Java.IO.File.Separator + "bluetooth";
            var storageDir = new Java.IO.File(ApplicationFolderPath + Java.IO.File.Separator + "settings");
            storageDir.Mkdirs();
            var settingsFile = new Java.IO.File(ApplicationFolderPath + Java.IO.File.Separator + "settings" + Java.IO.File.Separator + "settings.csv");
            settingsFile.CreateNewFile();
        }
    }
}

