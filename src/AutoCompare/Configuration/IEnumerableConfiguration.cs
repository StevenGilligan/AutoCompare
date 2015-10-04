using System;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    /// <summary>
    /// Configures how the engine handles the specified IEnumerable member
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEnumerableConfiguration<T> : IMemberConfiguration where T : class
    {
        /// <summary>
        /// Specifies that the IEnumerable member should be compared by matching child objects with the same ID
        /// </summary>
        /// <typeparam name="TMember">Type of the member used for matching objects</typeparam>
        /// <param name="member">The member used for matching objects</param>
        /// <returns></returns>
        IEnumerableConfiguration<T> MatchUsing<TMember>(Expression<Func<T, TMember>> member);

        /// <summary>
        /// Specifies that the IEnumerable member should be compared by matching child objects with the same ID
        /// </summary>
        /// <typeparam name="TMember">Type of the member used for matching objects</typeparam>
        /// <param name="member">The member used for matching objects</param>
        /// <param name="defaultId">The default value for the match type, to determine if an object is new</param>
        /// <returns></returns>
        IEnumerableConfiguration<T> MatchUsing<TMember>(Expression<Func<T, TMember>> member, TMember defaultId);
    }
}
