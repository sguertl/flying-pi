﻿using System;
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
        private string mSelectedItem;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Log);

            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderLog);
            mLvFiles = FindViewById<ListView>(Resource.Id.lvFiles);
            mLvFiles.ItemClick += OnShowListItemContextMenu;
            RegisterForContextMenu(mLvFiles);
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

        private void OnShowListItemContextMenu(object sender, AdapterView.ItemClickEventArgs e)
        {
            mSelectedItem = e.View.FindViewById<TextView>(Resource.Id.tvListItem).Text;
            mLvFiles.ShowContextMenu();
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);
            menu.SetHeaderIcon(Resource.Drawable.ifx_logo_small);
            menu.SetHeaderTitle("Options");
            menu.Add(0, v.Id, 0, "Raw data");
            menu.Add(0, v.Id, 0, "Visualize");
            menu.Add(0, v.Id, 0, "Delete");
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            string title = item.ToString().ToLower();
            switch (title)
            {
                case "raw data": ShowRawData(); break;
                case "visualize": ShowGraph(); break;
                case "delete": DeleteFolder(); break;
                default: return false;
            }
            return true;
        }

        private void ShowRawData()
        {
            Intent intent = new Intent(BaseContext, typeof(RawDataActivity));
            intent.PutExtra("filename", mSelectedItem);
            StartActivity(intent);
        }

        private void ShowGraph()
        {

        }

        private void DeleteFolder()
        {
            string dir = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mSelectedItem;
            var storageDir = new Java.IO.File(dir);
            string[] children = storageDir.List();
            for(int i = 0; i < children.Length; i++)
            {
                new Java.IO.File(dir, children[i]).Delete();
            }
            storageDir.Delete();
            mAdapter.DeleteElement(mSelectedItem);
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