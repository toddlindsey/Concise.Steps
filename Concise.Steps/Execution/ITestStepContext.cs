using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps.Execution
{
    /// <summary>
    /// Interface for adding and executing BDD topSteps.
    /// Consumers should instantiate an instance of this class during test initialization, and call <see cref="Dispose"/>
    /// on test cleanup, but should not actually use this interface, instead relying on <see cref="BddStringExtensions"/>
    /// </summary>
    public interface ITestStepContext : IDisposable
    {
        /// <summary>
        /// Get or set the action to take when a step takes longer than the designated "maximum duration"
        /// Default is <see cref="StepPerformanceFailureAction.FailTest"/>
        /// </summary>
        StepPerformanceFailureAction PerformanceFailAction { get; set; }

        /// <summary>
        /// Track and immediately execute the specified <see cref="TestStep"/>.
        /// </summary>
        void Execute(TestStep step);
    }
}
