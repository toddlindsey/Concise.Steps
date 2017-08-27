using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Concise.Steps
{
    /// <summary>
    /// Static methods to assist with argument precondition testing
    /// </summary>
    [DebuggerStepThrough]
    public static class Guard
    {
        /// <summary>
        /// Method to protect against null argument values by throwing an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="argumentName">The argument name.</param>
        public static void AgainstNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Method to protect against null or empty string argument values by throwing an <see cref="ArgumentNullException"/> 
        /// for a null parameter value or an <see cref="ArgumentException"/> for a value of empty string.
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="argumentName">The argument name.</param>
        public static void AgainstNullOrEmpty(string argumentValue, string argumentName)
        {
            AgainstNull(argumentValue, argumentName);

            if (argumentValue.Length == 0)
            {
                throw new ArgumentException("The argument value cannot be null or empty string.", argumentName);
            }
        }

        /// <summary>
        /// Method to protect against null or empty collection argument values by throwing an <see cref="ArgumentNullException"/> 
        /// for a null parameter value or an <see cref="ArgumentException"/> for an empty collection.
        /// </summary>
        /// <typeparam name="T">The type of the generic enumerable.</typeparam>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="argumentName">The argument name.</param>
        public static void AgainstNullOrEmpty<T>(IEnumerable<T> argumentValue, string argumentName)
        {
            AgainstNull(argumentValue, argumentName);

            if (!argumentValue.Any())
            {
                throw new ArgumentException("The argument value cannot be an empty collection.", argumentName);
            }
        }

        /// <summary>
        /// Method to protect against null or empty or whitespace argument values by throwing an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="argumentName">The argument name.</param>
        [DebuggerStepThrough]
        public static void AgainstNullOrWhitespace(string argumentValue, string argumentName)
        {
            AgainstNull(argumentValue, argumentName);

            if (string.IsNullOrWhiteSpace(argumentValue))
            {
                throw new ArgumentException("The argument value cannot be null or whitespace.", argumentName);
            }
        }
    }
}
