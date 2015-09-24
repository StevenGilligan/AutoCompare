using AutoCompare.Configuration;
using System.Collections.Generic;

namespace AutoCompare
{
    /// <summary>
    /// Wraps a default IComparerEngine
    /// </summary>
    public static class Comparer
    {
        /// <summary>
        /// The default IComparerEngine created by AutoCompare
        /// </summary>
        public static IComparerEngine Engine { get; } = new Engine();
        
        /// <summary>
        /// Configures how the AutoCompare engine should handle comparison of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IComparerConfiguration<T> Configure<T>() where T : class
        {
            return Engine.Configure<T>();
        }

        /// <summary>
        /// Returns the compiled comparer for the specified type. Should only be used when calling 
        /// Compare in a loop to avoid checking the cache on every call (for a minuscule, almost negligible performance gain)
        /// </summary>
        /// <typeparam name="T">Type of object to compare</typeparam>
        /// <returns>CompiledComparer</returns>
        public static CompiledComparer<T> Get<T>() where T : class
        {
            return Engine.Get<T>();
        }

        /// <summary>
        /// Compares two objects of type T and returns a list
        /// of Update for every property that was updated
        /// </summary>
        /// <typeparam name="T">Type of object to compare</typeparam>
        /// <param name="oldObject">The object containing old values</param>
        /// <param name="newObject">The object containing updated values</param>
        /// <returns>A list of values that have been updated between
        /// the old object and the new object</returns>
        public static IList<Difference> Compare<T>(T oldObject, T newObject) where T : class
        {
            return Engine.Compare<T>(oldObject, newObject);
        }
    }
}