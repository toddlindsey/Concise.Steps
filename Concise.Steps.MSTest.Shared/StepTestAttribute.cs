using Concise.Steps.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Concise.Steps
{
    public class StepTestWrapperException : Exception
    {
        public StepTestWrapperException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public override string StackTrace
        {
            get
            {
                var sb = new StringBuilder();

                sb.Append(this.InnerException.StackTrace);
                if( this.InnerException.InnerException != null )
                {
                    sb.AppendLine();
                    sb.AppendLine("---------- Inner StackTrace ----------");
                    sb.AppendLine(this.InnerException.InnerException.StackTrace);
                }
                return sb.ToString();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    //[DebuggerStepThrough]
    public class StepTestAttribute : TestMethodAttribute
    {
        public override TestResult[] Execute(ITestMethod testMethod)
        {
            using (var stepContext = TestStepContext.CreateNew())
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
