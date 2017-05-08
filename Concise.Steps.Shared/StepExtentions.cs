using Concise.Steps.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps
{
    /// <summary>
    /// Extension methods to <see cref="string"/> for the purpose of defining and executing test steps
    /// </summary>
    public static class BddStringExtensions
    {
        private const string NoContextMessage =
            "You defined a test step without any active test context.  Be sure you decorated the test using [StepTest], not [TestMethod]";

        /// <summary>
        /// Create a fail-fast step definition, meaning a failure in this step will immediately fail the entire test.
        /// </summary>
        /// <param name="stepDescription">The plain-english description of this step</param>
        /// <param name="action">The action to perform</param>
        public static void _(this string stepDescription, Action action)
        {
            if (TestStepContext.Current == null)
                throw new InvalidOperationException(NoContextMessage);

            var step = new TestStep(stepDescription, action, TimeSpan.MaxValue, true);
            TestStepContext.Current.Execute(step);
        }

        /// <summary>
        /// Create an empty step definition (for test documentation purposes only).
        /// </summary>
        /// <param name="stepDescription">The plain-english description of this step</param>
        public static void _(this string stepDescription)
        {
            stepDescription._(() => { });
        }
    }
}
