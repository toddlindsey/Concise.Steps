using System;
using System.Collections.Generic;
using System.Text;
using Concise.Steps;
using Concise.Steps.TestFramework;
using NUnit.Framework;

namespace Concise.Steps.NUnit.TestFramework
{
    public class NUnitAdapter : ITestFrameworkAdapter
    {
        public bool IsAssertionException(Exception ex)
        {
            return ex is AssertionException;
        }

        public Exception CreateAssertionException(string message, Exception ex = null)
        {
            return new AssertionException(message, ex);
        }

        public Exception CreateInconclusiveException(string message, Exception ex = null)
        {
            return new InconclusiveException(message, ex);
        }
    }
}
