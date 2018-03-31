using Concise.Steps.IoC;
using Concise.Steps.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps
{
    /// <summary>
    /// Helper methods that create assertion failures which contain the full context established by use of <see cref="AssertContext"/>.
    /// (If you use any of the Assert.XXX() methods, you will NOT get the benefit of this context in the error message reported.
    /// 
    /// Normally, FluentAssertions should be used to generate the assertions because this WILL use this context, but there are certain times when 
    /// you need to generate asserts that cannot be generated with this library. 
    /// (for example, in occasions where your assert would need OR conditions, FluentAssertions does not support this)
    /// </summary>
    public static class Fail
    {
        /// <summary>
        /// Throw an assertion failure with the provided message, as well as any context.  See <see cref="Fail"/> class notes.
        /// </summary>
        public static void With(string message)
        {
            Guard.AgainstNullOrEmpty(message, nameof(message));

            ITestFrameworkAdapter adapter = (ITestFrameworkAdapter)Bootstrapper.Locator.GetService(typeof(ITestFrameworkAdapter));
            Exception ex = adapter.CreateAssertionException(message);
            throw ex;
        }

        /// <summary>
        /// Throw an inconclusive assertion failure with the provided message, as well as any context.  See <see cref="Fail"/> class notes.
        /// </summary>
        public static void Inconclusive(string message)
        {
            Guard.AgainstNullOrEmpty(message, nameof(message));

            ITestFrameworkAdapter adapter = (ITestFrameworkAdapter)Bootstrapper.Locator.GetService(typeof(ITestFrameworkAdapter));
            Exception ex = adapter.CreateInconclusiveException(message);
            throw ex;
        }
    }
}
