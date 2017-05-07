using Concise.Steps.DemoTests.TestSubject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace Concise.Steps.DemoTests
{
    [TestClass]
    public class BasicUsageTests
    {
        private IObjectUnderTest testObject = new ObjectUnderTest();

        [StepTest]
        public void ObjectUnderTest_Add_GivenTwoNumbers_AddsCorrectly()
        {
            int a = 1, b = 2;
            $"Given numbers {a} and {b}"
                ._(() => { });

            int? result = null;
            "When Add(a,b) is called"
                ._(() => result = this.testObject.Add(a, b));

            "Expect 3"
                ._(() => result.Value.Should().Be(3));
        }

        [StepTest]
        public void ObjectUnderTest_AddPoorly_GivenTwoNumbers_AddsCorrectly_FAILS()
        {
            int a = 1, b = 2;
            $"Given numbers {a} and {b}"
                ._(() => { });

            int? result = null;
            "When AddPoorly(a,b) is called"
                ._(() => result = this.testObject.AddPoorly(a, b));

            "Expect 3"
                ._(() => result.Value.Should().Be(3));
        }

    }
}
