# Concise.Steps
A simple, non-opinionated C# Framework for authoring Self-Describing Tests.
_Great for taming integration, endpoint/e2e tests or even unit tests_

#### Dependencies

* Currently supports [MSTest (v2)](https://github.com/microsoft/testfx) and [NUnit (v3)](https://github.com/nunit/nunit); feel free to submit a pull request for other frameworks.
* Supports .NET Frameworks compatible with [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

#### Quick Start

1. From Nuget, add **Concise.Steps.MSTest** if using MSTestv2, or **Concise.Steps.NUnit** if using NUnit v3
2. Start authoring tests using the **[StepTest]** attribute as shown below.  _Our examples will use the highly-recommended [FluentAssertions](https://github.com/fluentassertions/fluentassertions) library, but this is not required._
3. If using NUnit, you must reference attribute directly from the NUnit assembly within your test project in order for things to work.
    The easiest way to do this is to simply place an NUnit [Description] attribute on a test class.
4. Nested steps (to any level) are also supported!  This is particularly helpful in taming more complex E2E tests.

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
5. async/await is also supported with the _\"Step description\"\.\_Async()_ method

#### Visual Studio Experience

##### Passed Test

![](images/passedTest.png?raw=true)

##### Failed Test

![](images/failedTest.png?raw=true)