using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps.TestFramework
{
    public interface ITestFrameworkAdapter
    {
        bool IsAssertionException(Exception ex);

        Exception CreateAssertionException(string message);

        Exception CreateInconclusiveException(string message);
    }
}
