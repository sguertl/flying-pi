using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace WiFiDronection
{
    [Activity(Label = "LogActivity", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen")]
    public class LogActivity : Activity
    {
        // Widgets
        private TextView mTvHeader;
        private ListView mLvFiles;
        private TextView mTvEmpty;
        private Button mBtBack;

        // Customized list adapter
        private ListAdapter mAdapter;
        // Selected list item
        private string mSelectedItem;

        /// <summary>
        /// Creates the activity and initializes the widgets.
        /// </summary>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Log);

            // Initialize widgets
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderLog);
            mLvFiles = FindViewById<ListView>(Resource.Id.lvFiles);
            mLvFiles.ItemClick += OnShowListItemContextMenu;
            // Set context menu on listview
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

        /// <summary>
        /// Displays messsage if list is empty.
        /// </summary>
        public override void OnContentChanged()
        {
            base.OnContentChanged();
            View empty = FindViewById<View>(Resource.Id.tvEmpty);
            FindViewById<ListView>(Resource.Id.lvFiles).EmptyView = empty;
        }

        /// <summary>
        /// Reads files in project folder and displays them on the list.
        /// </summary>
        private void FillFilesList()
        {
            // Get all file names in project folder
            var projectDir = new Java.IO.File(MainActivity.ApplicationFolderPath);
            List<string> fileNames = new List<string>();
            string[] fileArray = projectDir.List();
            if(fileArray != null)
            {
                fileNames = fileArray.ToList();
                // Sort files by date
                fileNames.Sort(new MyComparer());
            }
            // Display on list
            mAdapter = new ListAdapter(this, fileNames);
            mLvFiles.Adapter = mAdapter;
        }

        /// <summary>
        /// Shows context menu after click on list item.
        /// </summary>
        private void OnShowListItemContextMenu(object sender, AdapterView.ItemClickEventArgs e)
        {
            mSelectedItem = e.View.FindViewById<TextView>(Resource.Id.tvListItem).Text;
            mLvFiles.ShowContextMenu();
        }

        /// <summary>
        /// Creates a context menu with options.
        /// </summary>
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);
            menu.SetHeaderIcon(Resource.Drawable.ifx_logo_small);
            menu.SetHeaderTitle("Options");
            menu.Add(0, v.Id, 0, "Raw data");
            menu.Add(0, v.Id, 0, "Visualize");
            menu.Add(0, v.Id, 0, "Delete");
        }

        /// <summary>
        /// Handles OnClick event for context menu.
        /// </summary>
        public override bool OnContextItemSelected(IMenuItem item)
        {
            string title = item.ToString().ToLower().Trim();
            switch (title)
            {
                case "raw data": ShowRawData(); break;
                case "visualize": ShowGraph(); break;
                case "delete": DeleteFolder(); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Displays the raw data activity.
        /// </summary>
        private void ShowRawData()
        {
            Intent intent = new Intent(BaseContext, typeof(RawDataActivity));
            intent.PutExtra("filename", mSelectedItem);
            StartActivity(intent);
        }

        /// <summary>
        /// Displays the graphical data visualization activity.
        /// </summary>
        private void ShowGraph()
        {
            Intent intent = new Intent(BaseContext, typeof(VisualizationActivity));
            intent.PutExtra("filename", mSelectedItem);
            StartActivity(intent);
        }

        /// <summary>
        /// Deletes selected folder.
        /// </summary>
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

        /// <summary>
        /// Handles OnClick event for Back button.
        /// </summary>
        private void OnBackToMain(object sender, EventArgs e)
        {
            Finish();
        }
    }

    /// <summary>
    /// Comparer for sorting the list by date.
    /// </summary>
    public class MyComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return y.CompareTo(x);
        }
    }
}