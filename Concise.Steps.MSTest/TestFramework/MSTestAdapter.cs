using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Concise.Steps;
using Concise.Steps.TestFramework;

namespace Concise.Steps.MSTest.TestFramework
{
    public class MSTestAdapter : ITestFrameworkAdapter
    {
        public bool IsAssertionException(Exception ex)
        {
            return ex is UnitTestAssertException;
        }

        public Exception CreateAssertionException(string message)
        {
            return new AssertFailedException(message);
        }

        public Exception CreateInconclusiveException(string message)
        {
            return new AssertInconclusiveException(message);
        }
    }
}
