using System;
using System.Collections.Generic;
using AutoCompare.Configuration;
using AutoCompare.Compilation;

namespace AutoCompare
{
    /// <summary>
    /// AutoCompare engine. Handles type configuration, compilation and cache
    /// </summary>
    public class Engine : IComparerEngine, IBuilderEngine
    {
        private readonly Dictionary<Type, object> _comparerCache = new Dictionary<Type, object>();

        private readonly object _buildLock = new object();
        private readonly Dictionary<Type, object> _compilationLocks = new Dictionary<Type, object>();

        private readonly Dictionary<Type, ComparerConfiguration> _configurations = new Dictionary<Type, ComparerConfiguration>();

        /// <summary>
        /// Configures how this engine should handle comparison of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IComparerConfiguration<T> Configure<T>() where T : class
        {
            var type = typeof(T);
            if (_configurations.ContainsKey(type))
            {
                throw new Exception($"The type {type.Name} is already configured.");
            }
            var typeConfiguration = new ComparerConfiguration<T>(this);
            _configurations[type] = typeConfiguration;
            return typeConfiguration;
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
        public IList<Difference> Compare<T>(T oldObject, T newObject) where T : class
        {
            return Get<T>()(oldObject, newObject);
        }

        /// <summary>
        /// Returns the compiled comparer for the specified type. Should only be used when calling 
        /// Compare in a loop to avoid checking the cache on every call (for a minuscule, almost negligible performance gain)
        /// </summary>
        /// <typeparam name="T">Type of object to compare</typeparam>
        /// <returns>CompiledComparer</returns>
        public CompiledComparer<T> Get<T>() where T : class
        {
            var type = typeof(T);
            object comparer;
            if (_comparerCache.TryGetValue(type, out comparer))
            {
                return (CompiledComparer<T>)comparer;
            }
            EnsureTypeIsCompiled<T>();
            return (CompiledComparer<T>)_comparerCache[type];
        }

        /// <summary>
        /// Returns if the type is already compiled by this engine
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsTypeCompiled(Type type)
        {
            return _comparerCache.ContainsKey(type);
        }

        /// <summary>
        /// Returns if the type is already configured by this engine
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsTypeConfigured(Type type)
        {
            return _configurations.ContainsKey(type);
        }

        private void EnsureTypeIsCompiled<T>() where T : class
        {
            var type = typeof(T);
            object typeLock;
            lock (_buildLock)
            {
                _compilationLocks.TryGetValue(type, out typeLock);
                if (typeLock == null)
                {
                    typeLock = new object();
                    _compilationLocks.Add(type, typeLock);
                }
            }
            lock (typeLock)
            {
                if (_comparerCache.ContainsKey(type)) {
                    return;
                }
                InternalCompile<T>();
            }
        }

        /// <summary>
        /// Gets the ObectConfiguration for the specified type or returns a DefaultConfiguration
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ComparerConfiguration IBuilderEngine.GetObjectConfiguration(Type type)
        {
            if (_configurations.ContainsKey(type))
            {
                return _configurations[type];
            }
            var configuration = new ComparerConfiguration();
            _configurations[type] = configuration;
            return configuration;
        }

        void IBuilderEngine.Compile<T>()
        {
            EnsureTypeIsCompiled<T>();
        }
        
        private void InternalCompile<T>() where T : class
        {
            var builder = (IBuilderEngine)this;
            var config = builder.GetObjectConfiguration(typeof(T));
            var comparer = Builder.Build<T>(config);
            _comparerCache.Add(typeof(T), comparer);
        }
    }
}
