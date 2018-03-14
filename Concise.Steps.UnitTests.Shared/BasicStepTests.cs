using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Concise.Steps;

namespace Concise.Steps.UnitTests
{
    [TestClass]
    public class BasicStepTests
    {
        [StepTest]
        public void BasicStepTest_Step_WillAlways_ExecuteImmediately()
        {
            bool ran = false;
            "Step"._(() => ran = true);
            ran.Should().BeTrue();
        }

        [StepTest]
        public void BasicStepTest_Step_IfFailsWithAnyException_WillPropogateThatException()
        {
            Action step = () =>
            {
                "Step that fails"._(() => throw new Exception("My Exception"));
            };

            step.ShouldThrow<Exception>()
                .And.Message.Should().Be("My Exception");
        }
    }
}
