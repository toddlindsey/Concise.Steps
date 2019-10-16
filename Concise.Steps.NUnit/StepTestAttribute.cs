using Concise.Steps.Execution;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;

namespace Concise.Steps
{
    public class StepTestAttribute : TestAttribute, IWrapTestMethod
    {
        public StepTestAttribute() { }

        /// <summary>
        /// True to include step completed timestamps in each step message
        /// </summary>
        public bool DoneTimestamps { get; set; }

        /// <summary>
        /// Specify to override the default time format string used if <see cref="DoneTimestamps"/> is true.
        /// (See <see cref="DateTimeOffset.ToString(String)"/> for format strings)
        /// </summary>
        public string TimeFormatString { get; set; }

        public TestCommand Wrap(TestCommand command)
        {
            return new StepTestCommand(command, this);
        }

        private class StepTestCommand : DelegatingTestCommand
        {
            private StepTestAttribute attribute;

            public StepTestCommand(TestCommand innerCommand, StepTestAttribute attribute) : base(innerCommand)
            {
                Guard.AgainstNull(attribute, nameof(attribute));
                this.attribute = attribute;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                var contextOptions = new TestContextOptions
                {
                    ShowDoneTimestamps = this.attribute.DoneTimestamps,
                };

                if (!string.IsNullOrEmpty(this.attribute.TimeFormatString))
                    contextOptions.TimeFormatString = this.attribute.TimeFormatString;

                using (var stepContext = Execution.TestStepContext.CreateNew(contextOptions))
                {
                    try
                    {
                        innerCommand.Execute(context);

                        string stepResults = stepContext.RenderStepResults();
                        TestContext.Out.WriteLine(stepResults);
                        context.CurrentResult.SetResult(context.CurrentResult.ResultState, stepResults);
                    }
                    catch (Exception ex)
                    {
                        if (ex is NUnitException)
                            ex = ex.InnerException;

                        ResultState state = (ex as ResultStateException)?.ResultState ?? ResultState.Failure;

                        string stackTrace = null;
                        string stepResults = stepContext.RenderStepResults();
                        if (state == ResultState.Failure)
                        {
                            Exception failedStepException = stepContext.FirstFunctionalFailStepOrNull()?.Exception;
                            if (failedStepException != null)
                            {
                                stackTrace = ExceptionHelper.BuildStackTrace(failedStepException);
                            }
                            else // It appears an exception was thrown outside of a step
                            {
                                stackTrace = ExceptionHelper.BuildStackTrace(ex);
                                stepResults += "FAIL> (outside of test steps)";
                                stepResults += Environment.NewLine;
                                stepResults += Environment.NewLine;
                                stepResults += ex.Message;
                            }
                        }

                        TestContext.Out.WriteLine(stepResults);
                        context.CurrentResult.SetResult(state, stepResults, stackTrace);
                    }
                }

                return context.CurrentResult;
            }
        }
    }
}
