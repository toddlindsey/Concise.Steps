using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;

namespace Concise.Steps.MSTest.UnitTests
{
    [TestClass]
    public class StepTestWrapperExceptionTests
    {
        [StepTest]
        public void StepTestWrapperException_StackTrace_GivenException_WillRecurseInnerExceptions()
        {
            Exception deepException = null;
            "Given an exception with a 2-level deep set of inner exceptions"._(() =>
            {
                try
                { this.DeepThrow1(); }
                catch (Exception ex)
                { deepException = ex; }
            });

            StepTestWrapperException wrapperEx = null;
            "When the exception is passed to StepTestWrapperException"._(() =>
                wrapperEx = new StepTestWrapperException("My message", deepException));

            "Then the StackTrace property will include stack traces for the entire exception chain"._(() =>
            {
                string stackTrace = wrapperEx.StackTrace;

                // sample:

                //   at Concise.Steps.MSTest.UnitTests.Shared.StepTestWrapperExceptionTests.DeepThrow1() in c:\CodeRepos\Concise.Steps\Concise.Steps.MSTest.UnitTests.Shared\StepTestWrapperExceptionTests.cs:line 30
                //   at Concise.Steps.MSTest.UnitTests.Shared.StepTestWrapperExceptionTests.StepTestWrapperException_StackTrace_GivenException_WillRecurseInnerExceptions() in c:\CodeRepos\Concise.Steps\Concise.Steps.MSTest.UnitTests.Shared\StepTestWrapperExceptionTests.cs:line 17
                //---------- Inner ArgumentException StackTrace ----------
                //   at Concise.Steps.MSTest.UnitTests.Shared.StepTestWrapperExceptionTests.DeepThrow2() in c:\CodeRepos\Concise.Steps\Concise.Steps.MSTest.UnitTests.Shared\StepTestWrapperExceptionTests.cs:line 38
                //   at Concise.Steps.MSTest.UnitTests.Shared.StepTestWrapperExceptionTests.DeepThrow1() in c:\CodeRepos\Concise.Steps\Concise.Steps.MSTest.UnitTests.Shared\StepTestWrapperExceptionTests.cs:line 28
                //---------- Innerx2 ApplicationException StackTrace ----------
                //   at Concise.Steps.MSTest.UnitTests.Shared.StepTestWrapperExceptionTests.DeepThrow3() in c:\CodeRepos\Concise.Steps\Concise.Steps.MSTest.UnitTests.Shared\StepTestWrapperExceptionTests.cs:line 44
                //   at Concise.Steps.MSTest.UnitTests.Shared.StepTestWrapperExceptionTests.DeepThrow2() in c:\CodeRepos\Concise.Steps\Concise.Steps.MSTest.UnitTests.Shared\StepTestWrapperExceptionTests.cs:line 35

                stackTrace.Should().Contain("-- Inner ArgumentException StackTrace --");
                stackTrace.Should().Contain("-- Innerx2 ApplicationException StackTrace --");
            });
        }

        private void DeepThrow1()
        {
            try { this.DeepThrow2(); }
            catch (Exception ex)
            { throw new Exception("Level1", ex); }
        }

        private void DeepThrow2()
        {
            try { this.DeepThrow3(); }
            catch (Exception ex)
            {
                throw new ArgumentException("Level2", ex);
            }
        }

        private void DeepThrow3()
        {
            throw new ApplicationException("Level3");
        }
    }
}