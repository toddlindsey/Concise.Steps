using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps.Execution
{
    /// <summary>
    /// Represents a BDD Step definition.  This class is for internal use by <see cref="TestStepContext"/>
    /// </summary>
    public class TestStep
    {
        public TestStep(string description, Action action, TimeSpan maxDuration, bool failFast = true) : this()
        {
            this.Description = description;
            this.Action = action;
            this.MaxDuration = maxDuration;
            this.FailFast = failFast;
        }

        /// <summary>
        /// A reference to the parent BDD step, or null if this is already a top step
        /// </summary>
        public TestStep Parent { get; internal set; }

        /// <summary>
        /// Contains a list of child BDD steps
        /// </summary>
        public List<TestStep> Children { get; private set; }

        /// <summary>
        /// The description of this BDD step
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The action for this BDD step
        /// </summary>
        public Action Action { get; private set; }

        /// <summary>
        /// True if a failure in this step should fail all topSteps.
        /// If false, failure will still ultimately fail the test, but the following topSteps will still get the chance to complete.
        /// </summary>
        public bool FailFast { get; private set; }

        /// <summary>
        /// Return true if the current, or ANY ancestor BDD step is a "continue on fail" step, meaning <see cref="FailFast"/> is false.
        /// </summary>
        public bool InContinueOnFailContext
        {
            get
            {
                if (!this.FailFast)
                    return true;
                else if (this.Parent != null)
                    return this.Parent.InContinueOnFailContext;
                else
                    return false;
            }
        }

        /// <summary>
        /// The duration of the step, once completed
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        /// <summary>
        /// True if the step passed functionally, false if it failed
        /// </summary>
        public bool FunctionalPassed { get; internal set; }

        /// <summary>
        /// True if the step passed the specified performance bar (also true if no performance bar specified).
        /// </summary>
        public bool PerformancePassed
        {
            get { return this.Duration < this.MaxDuration; }
        }

        /// <summary>
        /// If the step failed, this is the exception
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// The maximum length of time this step should be allowed to take, before flagging it as a performance failure.
        /// </summary>
        public TimeSpan MaxDuration { get; set; }

        private TestStep()
        {
            this.Children = new List<TestStep>();
        }
    }

}
