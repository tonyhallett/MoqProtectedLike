using System;

namespace MoqProtectedLike
{
    //[ExcludeFromCodeCoverage]
    internal static class Guard
    {
        /// <summary>
        /// Ensures the given <paramref name="value"/> is not null.
        /// Throws <see cref="ArgumentNullException"/> otherwise.
        /// </summary>
        public static void NotNull(object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
