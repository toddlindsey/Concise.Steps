using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Concise.Steps.Extensions
{
    /// <summary>
    /// Extension methods for strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns true if the string is non-null and non-empty
        /// </summary>
        [DebuggerStepThrough]
        public static bool Exists(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Build a string with the specified string repeated the specified number of times.
        /// </summary>
        [DebuggerStepThrough]
        public static string Repeat(this string input, int count)
        {
            if (!string.IsNullOrEmpty(input))
            {
                StringBuilder builder = new StringBuilder(input.Length * count);
                for (int i = 0; i < count; i++) builder.Append(input);
                return builder.ToString();
            }

            return string.Empty;
        }
    }
}
