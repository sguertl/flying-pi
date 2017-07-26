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
        private Activity mContext;
        private List<string> mFileNames;
        private Typeface mFont;

        public ListAdapter(Activity context, List<string> names)
        {
            mContext = context;
            mFileNames = names;
            mFont = Typeface.CreateFromAsset(mContext.Assets, "SourceSansPro-Light.ttf");
        }

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
            if(costumView == null)
            {
                LayoutInflater inflater = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
                costumView = inflater.Inflate(Resource.Layout.CostumListItem, parent, false);
            }
            int id = Resource.Id.tvListItem;
            costumView.FindViewById<TextView>(id).Text = text;
            costumView.FindViewById<TextView>(id).Typeface = mFont;
            return costumView;
        }
    }
}