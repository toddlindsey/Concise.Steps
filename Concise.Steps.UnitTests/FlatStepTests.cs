using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Concise.Steps;

namespace Concise.Steps.UnitTests
{
    [TestClass]
    public class FlatStepTests
    {
        [StepTest]
        public void FlatStep_X_WillAlways_ExecuteImmediately()
        {
            bool ran = false;
            "Step"._(() => ran = true);
            ran.Should().BeTrue();
        }

        [StepTest]
        public void FlatStep_X_IfFailsWithAnyException_WillPropogateThatException()
        {
            Action step = () =>
            {
                "Step that fails"._(() => throw new Exception("My Exception"));
            };

            step.ShouldThrow<Exception>()
                .And.Message.Should().Be("My Exception");
        }

        //[TestMethod]
        //public void FlatStep_WithTimeout()
        //{
        //      "Step 1"._(() => {
        //             // line 1
        //             // line 2
        //             // line 3
        //      }, o => o
        //          .ShouldFinishWithin(TimeSpan.FromSeconds(3)) // Fail
        //          .ShouldFinishWithin(TimeSpan.FromSeconds(3), FailMode.Inconclusive) // Inconclusive
        //          .ContinueOnFail()
        //          .InconclusiveOnFail()
        //          .Finally() // always
        //          .OnCleanup() // at end, but only upon successful Step?  probably mirror Teardown() logic
        //          .ShouldThrow<InvalidOperationException>()
        //              .And.Message.Should().Contain("whatever");
        //}
    }
}
