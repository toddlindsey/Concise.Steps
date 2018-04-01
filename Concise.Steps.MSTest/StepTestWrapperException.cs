using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps
{
    public class StepTestWrapperException : Exception
    {
        public StepTestWrapperException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public override string StackTrace => this.StackTraceRecurseInners(this.InnerException, 1);

        private string StackTraceRecurseInners(Exception exception, int level)
        {
            var sb = new StringBuilder();

            if(level >= 2)
            {
                sb.AppendLine();
                sb.AppendLine($"---------- Inner{(level == 2 ? "" : $"x{level-1}")} {exception.GetType().Name} StackTrace ----------");
            }

            sb.Append(exception.StackTrace);

            if (exception.InnerException != null)
                sb.Append(this.StackTraceRecurseInners(exception.InnerException, level + 1));

            return sb.ToString();
        }
    }
}
