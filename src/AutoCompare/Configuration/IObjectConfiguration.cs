using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    /// <summary>
    /// Lets you configure how a type should be compared by the AutoCompare engine
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectConfiguration<T>
    {
        /// <summary>
        /// Ignores a property from being compared
        /// </summary>
        /// <param name="ignoreExpression">Lambda expression of the property to ignore</param>
        /// <returns>self</returns>
        IObjectConfiguration<T> Ignore(Expression<Func<T, object>> ignoreExpression);

        /// <summary>
        /// Lets you configure how an IEnumerable property should be handled by the Comparer
        /// </summary>
        /// <typeparam name="TEnumerable"></typeparam>
        /// <param name="listExpression">Lambda expression of the Enumerable property to configure</param>
        /// <param name="configuration">The configuration for this property</param>
        /// <returns></returns>
        IObjectConfiguration<T> Enumerable<TEnumerable>(Expression<Func<T, IEnumerable<TEnumerable>>> listExpression, Action<IEnumerableConfiguration<T, TEnumerable>> configuration);

        /// <summary>
        /// Specify that a dictionary should use deep compare, that is each object should be compared with the Comparer
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="propertyExpression">Lambda expression of the dictionary property to deeply compare</param>
        /// <returns>self</returns>
        IObjectConfiguration<T> DeepCompare<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> propertyExpression);

        /// <summary>
        /// Instructs the IComparerEngine how to precompile this comparer
        /// </summary>
        IPrecompile Compile { get; }
    }
}
