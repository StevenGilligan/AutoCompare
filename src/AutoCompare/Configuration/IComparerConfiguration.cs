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
        /// Configures how the engine handles the specified property
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IComparerConfiguration<T> For<TProp>(Expression<Func<T, TProp>> member, Action<IPropertyConfiguration> configuration);

        /// <summary>
        /// Configures how the engine handles the specified IEnumerable property
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IComparerConfiguration<T> For<TProp>(Expression<Func<T, IEnumerable<TProp>>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class;

        /// <summary>
        /// Configures how the engine handles the specified List property
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IComparerConfiguration<T> For<TProp>(Expression<Func<T, List<TProp>>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class;

        /// <summary>
        /// Configures how the engine handles the specified IList property
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="member"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IComparerConfiguration<T> For<TProp>(Expression<Func<T, IList<TProp>>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class;

        /// <summary>
        /// Configures how the engine handles the specified array property
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
        /// <param name="member">The property to ignore</param>
        /// <returns>self</returns>
        IComparerConfiguration<T> Ignore<TProp>(Expression<Func<T, TProp>> member);

        /// <summary>
        /// Instructs the IComparerEngine how to precompile this comparer
        /// </summary>
        IPrecompile Compile { get; }
    }
}
