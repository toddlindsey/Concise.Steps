using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concise.Steps.MSTest;
using FluentAssertions;

namespace Concise.Steps.UnitTests
{
    [TestClass]
    public class FlatStepTests : UnitTestBase
    {
        [TestMethod]
        public void FlatStep_X_WillExecuteImmediately()
        {
            bool ran = false;
            "Step".x(() => ran = true);
            ran.Should().BeTrue();
        }
    }
}
