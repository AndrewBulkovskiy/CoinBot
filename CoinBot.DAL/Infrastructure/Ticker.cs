using System.Timers;

namespace CoinBot.DAL.Infrastructure
{
    // Probably the best solution so far
    public static class Ticker
    {
        public static Timer TickerTimer;

        public delegate void TickerTick();
        public static event TickerTick OnTick;

        static Ticker()
        {
            TickerTimer = new Timer();
            TickerTimer.Elapsed += new ElapsedEventHandler(NotifySubscribers);
        }

        public static void Start(int milliseconds)
        {
            TickerTimer.Interval = milliseconds;
            TickerTimer.Start();
        }

        public static void Stop()
        {
            TickerTimer.Stop();
        }

        private static void NotifySubscribers(object sender, ElapsedEventArgs e)
        {
            OnTick?.Invoke();
        }
    }
}
