using Concise.Steps.DemoTests.TestSubject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Concise.Steps;

namespace Concise.Steps.DemoTests
{
    [TestClass]
    public class BasicUsageTests
    {
        private IObjectUnderTest testObject = new ObjectUnderTest();

        [StepTest]
        public void BasicUsage_3FlatSteps()
        {
            int a = 1, b = 2;
            $"Given numbers {a} and {b}"._();

            int? result = null;
            "When Add(a,b) is called"
                ._(() => result = this.testObject.Add(a, b));

            "Expect 3"
                ._(() => result.Value.Should().Be(3));
        }

        [StepTest]
        public void BasicUsage_3FlatSteps_LastFails()
        {
            int a = 1, b = 2;
            $"Given numbers {a} and {b}"._();

            int? result = null;
            "When AddPoorly(a,b) is called"
                ._(() => result = this.testObject.AddPoorly(a, b));

            "Expect 3"
                ._(() => result.Value.Should().Be(3));
        }

        [StepTest]
        public void BasicUsage_StepFails_WithInnerException()
        {
            "Step that fails with an exception containing a chain of inner exceptions"._(() =>
                this.ThrowExceptionWithInners());
        }

        private void ThrowExceptionWithInners()
        {
            try
            {
                this.ThrowExceptionWithInner();
            }
            catch(Exception ex)
            {
                throw new NotSupportedException("Message for exception that contains an inner exception", ex);
            }
        }

        private void ThrowExceptionWithInner()
        {
            try
            {
                this.ThrowException();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("My Argument Exception", ex);
            }
        }

        private void ThrowException()
        {
            throw new ApplicationException("My App Exception");
        }
    }
}
