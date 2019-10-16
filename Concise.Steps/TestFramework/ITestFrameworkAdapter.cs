using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Concise.Steps.TestFramework
{
    internal interface ITestFrameworkAdapter
    {
        TextWriter OutputWriter { get; }

        bool IsAssertionException(Exception ex);

        Exception CreateAssertionException(string message, Exception ex = null);

        Exception CreateInconclusiveException(string message, Exception ex = null);
    }
}
