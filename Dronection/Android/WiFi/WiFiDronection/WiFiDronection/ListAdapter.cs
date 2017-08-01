/************************************************************************
*																		*
*  Copyright (C) 2017 Infineon Technologies Austria AG.					*
*																		*
*  Licensed under the Apache License, Version 2.0 (the "License");		*
*  you may not use this file except in compliance with the License.		*
*  You may obtain a copy of the License at								*
*																		*
*    http://www.apache.org/licenses/LICENSE-2.0							*
*																		*
*  Unless required by applicable law or agreed to in writing, software	*
*  distributed under the License is distributed on an "AS IS" BASIS,	*
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or		*
*  implied.																*
*  See the License for the specific language governing					*
*  permissions and limitations under the License.						*
*																		*
*																		*
*  File: ListAdapter.cs													*
*  Created on: 2017-07-27                                  				*
*  Author(s): Klapsch Adrian Vasile (IFAT PMM TI COP)					*
*																		*
*  ListAdapter provides a customized design for listviews.				*
*																		*
************************************************************************/

using System.Collections.Generic;

using Android.App;
using Android.Content;
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
        /// Sets design of ListView, inherited from BaseAdapter.
        /// </summary>
        /// <param name="context">Activity where the list is placed</param>
        /// <param name="names">List of strings which are shown on the list</param>
        public ListAdapter(Activity context, List<string> names)
        {
            mContext = context;
            mFileNames = names;

            // Create font
            mFont = Typeface.CreateFromAsset(mContext.Assets, "SourceSansPro-Light.ttf");
        }

        /// <summary>
        /// Deletes element from list.
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

        /// <summary>
        /// Generates the view and returns it.
        /// </summary>
        /// <returns>Custom View</returns>
        /// <param name="position">Index</param>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            string text = mFileNames[position];
            View costumView = convertView;

            // If the view is not created yet, create it
            if(costumView == null)
            {
                // ListItem is a custom textview
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