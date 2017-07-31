using System;

namespace WiFiDronection
{
   public class DataPoint
    {

        // X coordinate
		private float mX;
		public float X
        {
            get { return mX; }
            set { mX = value; }
        }

        // Y coordinate
		private float mY;
		public float Y
		{
			get { return mY; }
			set { mY = value; }
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WiFiDronection.DataPoint"/> class.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public DataPoint(float x, float y)
        {
            this.mX = x;
            this.mY = y;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:WiFiDronection.DataPoint"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:WiFiDronection.DataPoint"/>.</returns>
        public override string ToString()
        {
            return String.Format("[{0}/{1}]", mX, mY);
        }
    }
}