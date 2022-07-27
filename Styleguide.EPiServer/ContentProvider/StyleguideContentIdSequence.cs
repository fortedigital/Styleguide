namespace Forte.Styleguide.EPiServer.ContentProvider
{
    public static class StyleguideContentIdSequence
    {
        private static readonly object SyncLock = new object();

        private const int Seed = 100000;
        private const int Capacity = 1000;

        private static int Current = Seed;

        public static int Next()
        {
            lock (SyncLock)
            {
                if (Current == Seed + Capacity)
                {
                    Current = Seed;
                }
                else
                {
                    Current++;
                }

                return Current;
            }
        }
    }
}
