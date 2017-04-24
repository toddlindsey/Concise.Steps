using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Concise.Steps.TestFramework;
using Concise.Steps.Performance;
using System.Threading;
using Concise.Steps.IoC;
using Concise.Steps.Extensions;
using System.Globalization;
using System.Reflection;

namespace Concise.Steps.Execution
{
    /// <inheritdoc/>
    public class TestStepContext : ITestStepContext
    {
        private List<TestStep> topSteps = new List<TestStep>();
        private bool stepResultsReported = false;

        /// <summary>
        /// Contains the current step context on the call context
        /// </summary>
        private static AsyncLocal<TestStepContext> CurrentStepContextLocal = new AsyncLocal<TestStepContext>();

        private ITestFrameworkAdapter adapter;

        private TestStepContext()
        {
            this.PerformanceFailAction = StepPerformanceFailureAction.FailTest;
            this.adapter = (ITestFrameworkAdapter)Bootstrapper.Locator.GetService(typeof(ITestFrameworkAdapter));
        }

        public static TestStepContext CreateNew()
        {
            var context = new TestStepContext();
            TestStepContext.Current = context;
            return context;
        }

        /// <summary>
        /// Return the current/active test context
        /// </summary>
        public static TestStepContext Current
        {
            get { return CurrentStepContextLocal.Value; }
            private set { CurrentStepContextLocal.Value = value; }
        }

        /// <summary>
        /// Get or set the currently running BDD Step
        /// </summary>
        public TestStep CurrentStep { get; private set; }

        /// <inheritdoc/>
        public StepPerformanceFailureAction PerformanceFailAction { get; set; }

        /// <inheritdoc/>
        public void Execute(TestStep newStep)
        {
            newStep.Parent = this.CurrentStep;

            List<TestStep> stepList = newStep.Parent == null ? this.topSteps : newStep.Parent.Children;
            lock (stepList)
                stepList.Add(newStep);

            TimeSpan duration = TimeSpan.Zero;
            try
            {
                this.CurrentStep = newStep;
                Collect.TimeOf(newStep.Action, out duration);
                newStep.Duration = duration;
                newStep.FunctionalPassed = true;
            }
            catch (Exception ex)
            {
                newStep.FunctionalPassed = false;
                newStep.Exception = ex;
                newStep.Duration = duration;
                if (newStep.FailFast)
                {
                    // If we are in a continue on fail context on an *ancestor* BDD step, we do NOT want to 
                    // render the BDD steps at this time - just throw the exception
                    // Also true if there are any parent steps
                    if (newStep.InContinueOnFailContext || newStep.Parent != null)
                        throw;
                    else
                        this.RenderStepResultsAndFail();
                }
            }
            finally
            {
                this.CurrentStep = newStep.Parent;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (this.topSteps)
            {
                if (this.topSteps.Any() && !stepResultsReported)
                {
                    string output = this.ComposeTestResults();

                    // If *any* step in the given tree failed functionally, return false
                    Func<TestStep, bool> stepTreePassed = null;
                    stepTreePassed = (TestStep step) =>
                    {
                        return step.FunctionalPassed && step.Children.All(c => stepTreePassed(c));
                    };

                    // If *any* step in the tree failed on performance, return false
                    Func<TestStep, bool> stepTreePerformancePassed = null;
                    stepTreePerformancePassed = (TestStep step) =>
                    {
                        return step.PerformancePassed && step.Children.All(c => stepTreePerformancePassed(c));
                    };

                    bool passedFuntionally = this.topSteps.All(step => stepTreePassed(step));

                    if (passedFuntionally)
                    {
                        Console.WriteLine(output);

                        bool performancePassed = this.topSteps.All(step => stepTreePerformancePassed(step));
                        if (!performancePassed)
                        {
                            if (this.PerformanceFailAction == StepPerformanceFailureAction.FailTest)
                                Fail.With(output);
                            else if (this.PerformanceFailAction == StepPerformanceFailureAction.AssertInconclusive)
                                Fail.Inconclusive(output);
                            else
                                throw new InvalidOperationException("Unrecognized action: " + this.PerformanceFailAction.ToString());
                        }

                    }
                    else
                        Fail.With(output);
                }
            }
        }

        /// <summary>
        /// Return a string of the test results across ALL topSteps
        /// </summary>
        protected string ComposeTestResults()
        {
            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine();
            this.RenderStepResults(builder, this.topSteps, 0);
            return builder.ToString();
        }

        /// <summary>
        /// Render all provided steps, and all of their children (recursively)
        /// </summary>
        /// <param name="builder">String builder to write output to</param>
        /// <param name="steps">The steps to render</param>
        /// <param name="level">The current step level (starting with 0, incrementing for recursive calls)</param>
        private void RenderStepResults(StringBuilder builder, IEnumerable<TestStep> steps, int level)
        {
            foreach (var step in steps)
            {
                builder.AppendLine(
                    (step.FunctionalPassed ? (step.PerformancePassed ? "PASS" : "PERF") : "FAIL") + ">  ".Repeat(level + 1) +
                    step.Description +
                    " (" +
                    step.Duration.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture) + "s" +
                    ")");

                // If there is an exception that isn't already reported by a child step, report it
                if (step.Exception != null && !step.Children.Any(child => child.Exception == step.Exception))
                {
                    builder.AppendLine();

                    Exception ex = step.Exception;

                    // TargetInvocationExceptions are just noise, just render the inner exception.
                    if (ex is TargetInvocationException)
                        ex = ex.InnerException;

                    if (adapter.IsAssertionException(ex))
                    {
                        builder.AppendLine(ex.Message);
                        builder.AppendLine();
                        builder.AppendLine(ex.StackTrace.ToString());
                    }
                    else
                        builder.AppendLine(ex.ToString());

                    builder.AppendLine();
                }

                if (step.Children.Any())
                    this.RenderStepResults(builder, step.Children, level + 1);
            }
        }

        /// <summary>
        /// Will call <see cref="ComposeTestResults"/> to compose the test results, and will then 
        /// throw an <see cref="AssertFailedException"/> (MSTest assertion failure) with those results.
        /// </summary>
        protected void RenderStepResultsAndFail()
        {
            string output = this.ComposeTestResults();
            stepResultsReported = true;
            Fail.With(output);
        }
    }
}
