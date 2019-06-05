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
        public TestCommand Wrap(TestCommand command)
        {
            return new StepTestCommand(command);
        }

        private class StepTestCommand : DelegatingTestCommand
        {
            public StepTestCommand(TestCommand innerCommand) : base(innerCommand)
            {
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                using (var stepContext = TestStepContext.CreateNew())
                {
                    try
                    {
                        innerCommand.Execute(context);
                        context.CurrentResult.SetResult(context.CurrentResult.ResultState, stepContext.RenderStepResults());
                    }
                    catch (Exception ex)
                    {
                        if (ex is NUnitException)
                            ex = ex.InnerException;

                        ResultState state = (ex as ResultStateException)?.ResultState ?? ResultState.Failure;

                        string stackTrace = null;
                        string message = stepContext.RenderStepResults();
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
                                message += "FAIL> (outside of test steps)";
                                message += Environment.NewLine;
                                message += Environment.NewLine;
                                message += ex.Message;
                            }
                        }

                        context.CurrentResult.SetResult(state, message, stackTrace);
                    }
                }

                return context.CurrentResult;
            }
        }
    }
}
