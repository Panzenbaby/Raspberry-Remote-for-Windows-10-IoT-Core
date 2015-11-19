using System.Diagnostics;

namespace RaspberryRemote
{
    class Delayer
    {
        Stopwatch mStopwatch;
        double mMillisecondDelay;

        public Delayer()
        {
            mStopwatch = Stopwatch.StartNew();
        }

        /**
         * Sets the delay in microseconds
         */
        public void SetMicroseconds(long microseconds)
        {
            mMillisecondDelay = (double)microseconds / 1000.0d;
        }

        public void DelayMicroseconds()
        {
            long initialTick = mStopwatch.ElapsedTicks;
            long initialElapsed = mStopwatch.ElapsedMilliseconds;
            double desiredTicks = mMillisecondDelay / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (mStopwatch.ElapsedTicks < finalTick) { }
        }
    }
}
