namespace BTDronection
{
   public class Flight
    {

        private static Flight instance = null;
        private static readonly object padlock = new object();

        private ControllerView mCV;

        public ControllerView CV
        {
            get { return mCV; }
            set { mCV = value; }
        }


        private Flight()
        {
            
        }

        public static Flight Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Flight();
                    }
                    return instance;
                }
            }
        }
    }
}