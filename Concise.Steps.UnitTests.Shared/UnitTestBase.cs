using Concise.Steps.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps.UnitTests
{
    ///// <summary>
    ///// A recommended base class for all MSTest-based unit tests.
    ///// NOTE!! MSTest v2 v1.1.17 has a bug that doesn't allow inheritence across assemblies(!!)
    /////     Therefore this base class will need to be copied into the consuming assembly
    /////     until the next version of MSTest is released (see: https://github.com/Microsoft/testfx/pull/147)
    ///// </summary>
    //public abstract class UnitTestBase
    //{
    //    /// <summary>
    //    /// The active <see cref="TestContext"/> for the running test.
    //    /// </summary>
    //    protected TestStepContext StepContext { get; private set; }

    //    [TestInitialize]
    //    public virtual void TestInitialize()
    //    {
    //        StepContext = TestStepContext.CreateNew();
    //    }

    //    [TestCleanup]
    //    public virtual void TestCleanup()
    //    {
    //        if (StepContext != null)
    //        {
    //            StepContext.Dispose();
    //            StepContext = null;
    //        }
    //    }
    //}
}
