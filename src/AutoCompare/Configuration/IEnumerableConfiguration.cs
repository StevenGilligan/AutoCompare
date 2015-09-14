using System;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    /// <summary>
    /// Lets you configure how a Enumerable is compared with the Comparer
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public interface IEnumerableConfiguration<TParent, TEnumerable>
    {
        /// <summary>
        /// Specifies that the Enumerable should be deeply compared
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="idExpression">The Id property</param>
        /// <returns>parent</returns>
        IObjectConfiguration<TParent> DeepCompare<TKey>(Expression<Func<TEnumerable, TKey>> idExpression);

        /// <summary>
        /// Specifies that the Enumerable should be deeply compared
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="idExpression">The lambda expression to select what property should be used as an ID</param>
        /// <param name="idDefaultValue">Specifies a value for the ID that means the object is new and should be treated as such</param>
        /// <returns>parent</returns>
        IObjectConfiguration<TParent> DeepCompare<TKey>(Expression<Func<TEnumerable, TKey>> idExpression, TKey idDefaultValue);
    }
}
