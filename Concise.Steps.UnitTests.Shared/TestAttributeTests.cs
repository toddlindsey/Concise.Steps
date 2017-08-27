using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Steps.UnitTests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MyTestAttribute : TestMethodAttribute
    {
        public override TestResult[] Execute(ITestMethod testMethod)
        {
            Exception exception;
            try
            {
                throw new AssertFailedException("Exception Message");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var result = new TestResult
            {
                Outcome = UnitTestOutcome.Passed,
                //DebugTrace = "Debug Trace Line 1\nDebug Trace Line 2\nDebug Trace Line 3",
                //LogOutput = "Log Output Line 1\nLog Output Line 2\nLog Output Line 3",
                //LogError = "Log Error Line 1\nLog Error Line 2\nLog Error Line 3",
                TestFailureException = exception
                //TestContextMessages = "Test Context Line 1\nTest Context Line 2\nTest Context Line 3"
            };

            return new TestResult[] { result };
        }
    }

    [TestClass]
    public class TestAttributeTests
    {
        [MyTest]
        [Ignore]
        public void TestAttributeTest()
        {
        }
    }
}
