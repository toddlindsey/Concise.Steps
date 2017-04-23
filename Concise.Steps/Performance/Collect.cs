using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps.Performance
{
    /// <summary>
    /// Helper methods to collect data about a passed-in routine
    /// </summary>
    public static class Collect
    {
        /// <summary>
        /// Collect the duration of the specified routine.
        /// Any exception from the routine will propogate out.
        /// </summary>
        public static TimeSpan TimeOf(Action action)
        {
            var start = DateTime.Now;
            action();
            return DateTime.Now - start;
        }

        /// <summary>
        /// Collect the duration of the specified routine.
        /// Any exception from the routine will propogate, but the duration will always be captured.
        /// </summary>
        public static void TimeOf(Action action, out TimeSpan duration)
        {
            var start = DateTime.Now;
            try
            {
                action();
            }
            finally
            {
                duration = DateTime.Now - start;
            }
        }
    }

}
