using System;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    /// <summary>
    /// Configures how the engine handles the specified IEnumerable property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEnumerableConfiguration<T> : IMemberConfiguration where T : class
    {
        /// <summary>
        /// Specifies that the IEnumerable property should be compared by matching child objects with the same ID
        /// </summary>
        /// <typeparam name="TProp">Type of the property used for matching objects</typeparam>
        /// <param name="member">The property used for matching objects</param>
        /// <returns></returns>
        IEnumerableConfiguration<T> MatchUsing<TProp>(Expression<Func<T, TProp>> member);

        /// <summary>
        /// Specifies that the IEnumerable property should be compared by matching child objects with the same ID
        /// </summary>
        /// <typeparam name="TProp">Type of the property used for matching objects</typeparam>
        /// <param name="member">The property used for matching objects</param>
        /// <param name="defaultId">The default value for the match property, to determine if an object is new</param>
        /// <returns></returns>
        IEnumerableConfiguration<T> MatchUsing<TProp>(Expression<Func<T, TProp>> member, TProp defaultId);
    }
}
