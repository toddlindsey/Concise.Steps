using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps.Execution
{
    /// <summary>
    /// Failure options for performance failures.  Configured via <see cref="IStepTestContext"/>.
    /// </summary>
    public enum StepPerformanceFailureAction
    {
        /// <summary>
        /// Fail the test on performance failures
        /// </summary>
        FailTest,

        /// <summary>
        /// Flag the test result as "inconclusive" on performance failures
        /// </summary>
        AssertInconclusive
    }
}
