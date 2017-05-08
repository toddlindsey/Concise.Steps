# Concise.Steps
A Non-Opinionated C# Framework for Authoring Self-Describing Tests.
_great for taming unit, integration, AND long-running mutithreaded e2e tests - come as you are, stay as you please_

#### Dependencies
_Dependencies automatically installed via NuGet.  The flexibility to use alternate test frameworks and assertion libraries is planned._
* [MSTest (v2)](https://github.com/microsoft/testfx)
* [FluentAssertions](https://github.com/fluentassertions/fluentassertions)

#### Quick Start

1. Ensure your class library is setup to run [MSTest (v2)](https://github.com/microsoft/testfx) tests by installing **MSTest.TestAdapter** and **MSTest.TestFramework** packages.
2. Add **Concise.Steps.MSTest** library using NuGet
3. Start authoring tests using the **[StepTest]** attribute as shown below:

```C#
        [StepTest]
        public void ObjectUnderTest_Add_GivenTwoNumbers_AddsCorrectly()
        {
            int a = 1, b = 2;
            $"Given numbers {a} and {b}"._();

            int? result = null;
            "When Add(a,b) is called"
                ._(() => result = this.testObject.Add(a, b));

            "Expect 3"
                ._(() => result.Value.Should().Be(3));
        }
```
