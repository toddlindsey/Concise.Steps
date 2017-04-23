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
            "Invalid attempt to define a test step without first creating a TestStepContext.";

        /// <summary>
        /// Same as <see cref="x(string,Action,TimeSpan)"/>, but where max duration is <see cref="TimeSpan.MaxValue"/>.
        /// </summary>
        public static void x(this string stepDescription, Action action)
        {
            stepDescription.x(action, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Create a fail-fast step definition, meaning a failure in this step will immediately fail the entire test.
        /// </summary>
        /// <param name="stepDescription">The plain-english description of this step</param>
        /// <param name="action">The action to perform</param>
        public static void x(this string stepDescription, Action action, TimeSpan maxDuration)
        {
            if (TestStepContext.Current == null)
                throw new InvalidOperationException(NoContextMessage);

            var step = new TestStep(stepDescription, action, maxDuration, true);
            TestStepContext.Current.Execute(step);
        }

        /// <summary>
        /// Same as <see cref="continueOnFail(string,Action,TimeSpan)"/>, but where max duration is <see cref="TimeSpan.MaxValue"/>.
        /// </summary>
        public static void continueOnFail(this string stepDescription, Action action)
        {
            stepDescription.continueOnFail(action, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Create a step definition that, if it fails, will still allow the next defined step to execute.
        /// </summary>
        /// <param name="stepDescription">The plain-english description of this step</param>
        /// <param name="action">The action to perform</param>
        public static void continueOnFail(this string stepDescription, Action action, TimeSpan maxDuration)
        {
            if (TestStepContext.Current == null)
                throw new InvalidOperationException(NoContextMessage);

            var step = new TestStep(stepDescription, action, maxDuration, false);
            TestStepContext.Current.Execute(step);
        }
    }
}
