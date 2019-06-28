using Concise.Steps.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Concise.Steps
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StepTestAttribute : TestMethodAttribute
    {
        /// <summary>
        /// True to include step completed timestamps in each step message
        /// </summary>
        public bool DoneTimestamps { get; set; }

        /// <summary>
        /// Specify to override the default time format string used if <see cref="ShowTimestamps"/> is true.
        /// (See <see cref="DateTimeOffset.ToString(String)"/> for format strings)
        /// </summary>
        public string TimeFormatString { get; set; }


        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var contextOptions = new TestContextOptions
            {
                ShowDoneTimestamps = this.DoneTimestamps
            };

            if (!string.IsNullOrEmpty(this.TimeFormatString))
                contextOptions.TimeFormatString = this.TimeFormatString;

            using (var stepContext = Execution.TestStepContext.CreateNew(contextOptions))
            {
                if (testMethod.GetAttributes<DataRowAttribute>(false).Any())
                    throw new NotSupportedException("[Test] methods do not currently support the use of the [DataRow] attribute.");

                TestResult result = testMethod.Invoke(null);

                if (result.Outcome == UnitTestOutcome.Failed)
                {
                    TestStep functionalFailStep = stepContext.FirstFunctionalFailStepOrNull();
                    if (functionalFailStep != null)
                    {
                        // Provide a "fake" exception so the step output is clearly visible in the primary results window
                        result.TestFailureException = new StepTestWrapperException(stepContext.RenderStepResults(), functionalFailStep.Exception);
                        //result.DebugTrace = functionalFailStep.Exception.StackTrace;
                    }
                }
                else if(result.Outcome == UnitTestOutcome.Passed)
                {
                    if (stepContext.HasPerformanceFailure())
                    {
                        // Provide a "fake" exception so the step output is clearly visible in the primary results window
                        result.TestFailureException = new AssertFailedException(stepContext.RenderStepResults());
                        result.DebugTrace = null;
                        result.Outcome = UnitTestOutcome.Failed;
                    }
                    else
                    {
                        // Yes this is odd to register an exception for a Passed test, but it seems the only
                        // way to show test results directly as the primary message, which is a much better experience
                        // than having to click again into a separate "Output" window.
                        result.TestFailureException = new AssertFailedException(stepContext.RenderStepResults());
                    }
                }

                return new TestResult[] { result };
            }
        }
    }
}
