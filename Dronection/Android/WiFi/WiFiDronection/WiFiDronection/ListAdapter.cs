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
    public class ListAdapter : BaseAdapter<string>
    {
        // Members
        private Activity mContext;
        private List<string> mFileNames;
        private Typeface mFont;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Activity where the list is placed</param>
        /// <param name="names">List of strings which are shown on the list</param>
        public ListAdapter(Activity context, List<string> names)
        {
            mContext = context;
            mFileNames = names;
            // Standard Infineon font
            mFont = Typeface.CreateFromAsset(mContext.Assets, "SourceSansPro-Light.ttf");
        }

        /// <summary>
        /// Deletes element of list
        /// </summary>
        /// <param name="element">Element which should be deleted</param>
        public void DeleteElement(string element)
        {
            mFileNames.Remove(element);
        }

        public override string this[int position]
        {
            get { return mFileNames[position]; }
        }

        public override int Count
        {
            get { return mFileNames.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            string text = mFileNames[position];
            View costumView = convertView;
            // If the view is not created yet, create it
            if(costumView == null)
            {
                // Listitem is a custom textview
                LayoutInflater inflater = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
                costumView = inflater.Inflate(Resource.Layout.CustomListItem, parent, false);
            }
            int id = Resource.Id.tvListItem;
            // Set text and font
            costumView.FindViewById<TextView>(id).Text = text;
            costumView.FindViewById<TextView>(id).Typeface = mFont;
            return costumView;
        }
    }
}