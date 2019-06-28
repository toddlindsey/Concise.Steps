using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps.Execution
{
    /// <summary>
    /// Options that can be passed to <see cref="TestStepContext"/> to control behaviors.
    /// </summary>
    internal class TestContextOptions
    {
        /// <summary>
        /// True to show timestamps for when each step completed (in addition to the duration)
        /// </summary>
        public bool? ShowDoneTimestamps { get; set; }

        /// <summary>
        /// The <see cref="DateTime"/> format string to use if step timestamps were requested
        /// </summary>
        public string TimeFormatString = "HH:mm:ss.fff";
    }
}
