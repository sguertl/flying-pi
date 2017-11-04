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

namespace Datalyze
{
    public class ListAdapter : BaseAdapter<string>
    {
        // Members
        private Activity mContext;
        private List<string> mFileNames;
        //private Typeface mFont;

        /// <summary>
        /// Sets design of ListView, inherited from BaseAdapter.
        /// </summary>
        /// <param name="context">Activity where the list is placed</param>
        /// <param name="names">List of strings which are shown on the list</param>
        public ListAdapter(Activity context, List<string> names)
        {
            mContext = context;
            mFileNames = names;

            // Create font
            //mFont = Typeface.CreateFromAsset(mContext.Assets, "SourceSansPro-Light.ttf");
        }

        /// <summary>
        /// Deletes element from list.
        /// </summary>
        /// <param name="element">Element which should be deleted</param>
        public void DeleteElement(string element)
        {
            mFileNames.Remove(element);
        }

        /// <summary>
        /// Return the file at the position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
		public override string this[int position]
        {
            get { return mFileNames[position]; }
        }

        /// <summary>
        /// Returns the number of files.
        /// </summary>
		public override int Count
        {
            get { return mFileNames.Count; }
        }

        /// <summary>
        /// Returns the item identifier.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
		public override long GetItemId(int position)
        {
            return position;
        }

        /// <summary>
        /// Generates the view and returns it.
        /// </summary>
        /// <returns>Custom View</returns>
        /// <param name="position">Index</param>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            string text = mFileNames[position];
            View customView = convertView;

            // If the view is not created yet, create it
            if (customView == null)
            {
                // ListItem is a custom textview
                LayoutInflater inflater = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
                customView = inflater.Inflate(Resource.Layout.CustomListItem, parent, false);
            }
            int id = Resource.Id.tvListItem;

            // Set text and font
            customView.FindViewById<TextView>(id).Text = text;
            //customView.FindViewById<TextView>(id).Typeface = mFont;
            return customView;
        }
    }
}