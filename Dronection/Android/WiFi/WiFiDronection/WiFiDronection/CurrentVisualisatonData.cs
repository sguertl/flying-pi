using System.Collections.Generic;

namespace WiFiDronection
{
    public class CurrentVisualisatonData
    {
        private Dictionary<string,List<DataPoint>> m_Points;

        private List<float> m_HighContTime;

        public List<float> HighContTime
        {
            get { return m_HighContTime; }
            set { m_HighContTime = value; }
        }

        public Dictionary<string, List<DataPoint>> Points
        {
            get { return m_Points; }
            set { m_Points = value; }
        }

        /// <summary>
        /// Singleton
        /// </summary>
        private static CurrentVisualisatonData instance = null;
        private static readonly object padlock = new object();

        private CurrentVisualisatonData() {
            this.m_Points = new Dictionary<string, List<DataPoint>>();
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