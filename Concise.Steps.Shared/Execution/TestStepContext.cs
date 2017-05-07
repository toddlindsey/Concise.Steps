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
    /// <summary>
    /// Context for adding and executing test steps.
    /// Consumers should instantiate an instance of this class during test initialization, and call <see cref="Dispose"/>
    /// on test cleanup, but should not actually use this interface, instead relying on <see cref="BddStringExtensions"/>
    /// </summary>
    internal class TestStepContext : IDisposable
    {
        private List<TestStep> topSteps = new List<TestStep>();
        //private bool stepResultsReported = false;

        /// <summary>
        /// Contains the current step context on the call context
        /// </summary>
        private static AsyncLocal<TestStepContext> CurrentStepContextLocal = new AsyncLocal<TestStepContext>();

        private ITestFrameworkAdapter adapter;

        private TestStepContext()
        {
            if (TestStepContext.Current != null)
            {
                throw new InvalidOperationException(
                    "Invalid attempt to create a TestStepContext when one is already defined for this thread context.  " +
                    "This either means an instance was not wrapped with a using() block, or that a new context is directly being created inside of another.");
            }

            TestStepContext.Current = this;
            //this.PerformanceFailAction = StepPerformanceFailureAction.FailTest;
            this.adapter = (ITestFrameworkAdapter)Bootstrapper.Locator.GetService(typeof(ITestFrameworkAdapter));
        }

        public static TestStepContext CreateNew()
        {
            return new TestStepContext();
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

        ///// <summary>
        ///// Get or set the action to take when a step takes longer than the designated "maximum duration"
        ///// Default is <see cref="StepPerformanceFailureAction.FailTest"/>
        ///// </summary>
        //public StepPerformanceFailureAction PerformanceFailAction { get; set; }

        /// <summary>
        /// Track and immediately execute the specified <see cref="TestStep"/>.
        /// </summary>
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

                // If in fail-fast, allow original exception to propogate, else we capture and swallow for now
                if (newStep.FailFast)
                    throw;

                //if (newStep.FailFast)
                //{
                //    // If we are in a continue on fail context on an *ancestor* BDD step, we do NOT want to 
                //    // render the BDD steps at this time - just throw the exception
                //    // Also true if there are any parent steps
                //    if (newStep.InContinueOnFailContext || newStep.Parent != null)
                //        throw;
                //    else
                //        this.RenderStepResultsAndFail();
                //}
            }
            finally
            {
                this.CurrentStep = newStep.Parent;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CurrentStepContextLocal.Value = null;

            //lock (this.topSteps)
            //{
            //    if (this.topSteps.Any() && !stepResultsReported)
            //    {
            //        string output = this.RenderStepResults();

            //        // If *any* step in the given tree failed functionally, return false
            //        Func<TestStep, TestStep> findFirstFunctionalFailStep = null;
            //        findFirstFunctionalFailStep = (TestStep step) =>
            //        {
            //            return !step.FunctionalPassed ? step : step.Children.FirstOrDefault(c => findFirstFunctionalFailStep(c) != null);
            //        };

            //        // If *any* step in the tree failed on performance, return false
            //        Func<TestStep, bool> stepTreePerformancePassed = null;
            //        stepTreePerformancePassed = (TestStep step) =>
            //        {
            //            return step.PerformancePassed && step.Children.All(c => stepTreePerformancePassed(c));
            //        };

            //        TestStep functionalFailStep = this.topSteps.FirstOrDefault(step => findFirstFunctionalFailStep(step) != null);

            //        if (functionalFailStep != null)
            //        {
            //            throw this.adapter.CreateAssertionException(output, functionalFailStep.Exception);
            //        }
            //        else
            //        {
            //            Console.WriteLine(output);

            //            bool performancePassed = this.topSteps.All(step => stepTreePerformancePassed(step));
            //            if (!performancePassed)
            //            {
            //                if (this.PerformanceFailAction == StepPerformanceFailureAction.FailTest)
            //                    Fail.With(output);
            //                else if (this.PerformanceFailAction == StepPerformanceFailureAction.AssertInconclusive)
            //                    Fail.Inconclusive(output);
            //                else
            //                    throw new InvalidOperationException("Unrecognized action: " + this.PerformanceFailAction.ToString());
            //            }
            //        }
            //    }
            //}
        }

        public TestStep FirstFunctionalFailStepOrNull()
        {
            // If *any* step in the given tree failed functionally, return false
            Func<TestStep, TestStep> findFirstFunctionalFailStep = null;
            findFirstFunctionalFailStep = (TestStep step) =>
            {
                return !step.FunctionalPassed ? step : step.Children.FirstOrDefault(c => findFirstFunctionalFailStep(c) != null);
            };

            return this.topSteps.FirstOrDefault(step => findFirstFunctionalFailStep(step) != null);
        }

        public bool HasPerformanceFailure()
        {
            // If *any* step in the tree failed on performance, return false
            Func<TestStep, bool> stepTreePerformancePassed = null;
            stepTreePerformancePassed = (TestStep step) =>
            {
                return step.PerformancePassed && step.Children.All(c => stepTreePerformancePassed(c));
            };

            return !this.topSteps.All(step => stepTreePerformancePassed(step));
        }

        /// <summary>
        /// Return a string of the test results across ALL topSteps
        /// </summary>
        public string RenderStepResults(bool renderCallstacks = false)
        {
            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine();
            this.RenderStepResultsAtLevel(builder, this.topSteps, 0, renderCallstacks);
            return builder.ToString();
        }

        /// <summary>
        /// Render all provided steps, and all of their children (recursively)
        /// </summary>
        /// <param name="builder">String builder to write output to</param>
        /// <param name="steps">The steps to render</param>
        /// <param name="level">The current step level (starting with 0, incrementing for recursive calls)</param>
        private void RenderStepResultsAtLevel(StringBuilder builder, IEnumerable<TestStep> steps, int level, bool renderCallstacks)
        {
            foreach (var step in steps)
            {
                builder.AppendLine(
                    (step.FunctionalPassed ? (step.PerformancePassed ? "PASS" : "TOO LONG") : "FAIL") + ">  ".Repeat(level + 1) +
                    step.Description +
                    $" ({step.Duration.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture)}s)");

                // If there is an exception that isn't already reported by a child step, report it
                if (step.Exception != null && !step.Children.Any(child => child.Exception == step.Exception))
                {
                    builder.AppendLine();

                    Exception ex = step.Exception;

                    // TargetInvocationExceptions are just noise, just render the inner exception.
                    if (ex is TargetInvocationException)
                        ex = ex.InnerException;

                    if (renderCallstacks)
                    {
                        if (adapter.IsAssertionException(ex))
                        {
                            builder.AppendLine(ex.Message);
                            builder.AppendLine();
                            builder.AppendLine(ex.StackTrace.ToString());
                        }
                        else
                            builder.AppendLine(ex.ToString());
                    }
                    else
                    {
                        if( adapter.IsAssertionException(ex))
                            builder.AppendLine(ex.Message);
                        else
                            builder.AppendLine($"{ex.GetType().Name}: {ex.Message}");
                    }

                    builder.AppendLine();
                }

                if (step.Children.Any())
                    this.RenderStepResultsAtLevel(builder, step.Children, level + 1, renderCallstacks);
            }
        }

        /// <summary>
        /// Will call <see cref="RenderStepResults"/> to compose the test results, and will then 
        /// throw an <see cref="AssertFailedException"/> (MSTest assertion failure) with those results.
        /// </summary>
        protected void RenderStepResultsAndFail()
        {
            string output = this.RenderStepResults();
            //stepResultsReported = true;

            throw this.adapter.CreateAssertionException(output, this.CurrentStep.Exception);
        }
    }
}
