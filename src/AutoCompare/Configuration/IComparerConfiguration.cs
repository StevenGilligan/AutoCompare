using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    /// <summary>
    /// Configures how the engine handles a specified type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IComparerConfiguration<T>
    {
        /// <summary>
        /// By default, only public properties are compared. Call this to enable comparing public fields for the specified type.
        /// </summary>
        /// <returns></returns>
        IComparerConfiguration<T> ComparePublicFields();

        /// <summary>
        /// Configures how the engine handles the specified member
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IComparerConfiguration<T> For<TProp>(Expression<Func<T, TProp>> member, Action<IMemberConfiguration> configuration);

        /// <summary>
        /// Configures how the engine handles the specified IEnumerable member
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IComparerConfiguration<T> For<TProp>(Expression<Func<T, IEnumerable<TProp>>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class;

        /// <summary>
        /// Configures how the engine handles the specified List member
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IComparerConfiguration<T> For<TProp>(Expression<Func<T, List<TProp>>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class;

        /// <summary>
        /// Configures how the engine handles the specified IList member
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IComparerConfiguration<T> For<TProp>(Expression<Func<T, IList<TProp>>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class;

        /// <summary>
        /// Configures how the engine handles the specified array member
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IComparerConfiguration<T> For<TProp>(Expression<Func<T, TProp[]>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class;

        /// <summary>
        /// Ignores a property. A shortcut for 
        /// .For(x => x.Property, x => x.Ignore())
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member">The member to ignore</param>
        /// <returns>self</returns>
        IComparerConfiguration<T> Ignore<TProp>(Expression<Func<T, TProp>> member);

        /// <summary>
        /// Instructs the IComparerEngine how to precompile this comparer
        /// </summary>
        IPrecompile Compile { get; }
    }
}
