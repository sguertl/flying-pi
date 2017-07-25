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
    [Activity(Label = "HelpActivity", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen")]
    public class HelpActivity : Activity
    {
        private TextView mTvHeaderHelp;
        private Button mBtnBackHelp;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Help);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            mTvHeaderHelp = FindViewById<TextView>(Resource.Id.tvHeaderHelp);
            mBtnBackHelp = FindViewById<Button>(Resource.Id.btnBackHelp);

            mTvHeaderHelp.Typeface = font;
            mBtnBackHelp.Typeface = font;

            mBtnBackHelp.Click += OnBack;
        }

        private void OnBack(object sender, EventArgs e)
        {
            Finish();
        }
    }
}