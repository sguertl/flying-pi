using System.Collections.Generic;

namespace WiFiDronection
{
    public class CurrentVisualisatonData
    {
        private string m_Title;
        private List<DataPoint> m_Points;

        public List<DataPoint> Points
        {
            get { return m_Points; }
            set { m_Points = value; }
        }

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        /// <summary>
        /// Singleton
        /// </summary>
        private static CurrentVisualisatonData instance = null;
        private static readonly object padlock = new object();

        private CurrentVisualisatonData() {
            this.m_Title = "";
            this.m_Points = new List<DataPoint>();
        }

        public static CurrentVisualisatonData Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new CurrentVisualisatonData();
                    }
                    return instance;
                }
            }
        }


    }
}