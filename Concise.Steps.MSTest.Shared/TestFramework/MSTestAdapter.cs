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

        public Exception CreateAssertionException(string message, Exception ex = null)
        {
            return new AssertFailedException(message, ex);
        }

        public Exception CreateInconclusiveException(string message, Exception ex = null)
        {
            return new AssertInconclusiveException(message, ex);
        }
    }
}
