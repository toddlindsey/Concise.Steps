# Concise.Steps
A Non-Opinionated C# Framework for Authoring Self-Describing Tests.
_great for taming unit, integration, AND long-running mutithreaded e2e tests - come as you are, stay as you please_

#### Quick Start

1. Add **Concise.Steps.MSTest** library using NuGet (this will add the required [MSTest (v2)](https://github.com/microsoft/testfx) assemblies as needed)
2. Start authoring tests using the **[StepTest]** attribute as shown below.  _Our examples will use the highly-recommended [FluentAssertions](https://github.com/fluentassertions/fluentassertions) library, but this is not required._

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
