using System;
using System.Collections.Generic;
using AutoCompare.Configuration;
using AutoCompare.Compilation;

namespace AutoCompare
{
    public class Engine : IComparerEngine, IBuilderEngine
    {
        private readonly Dictionary<Type, object> _comparerCache = new Dictionary<Type, object>();

        private readonly object _buildLock = new object();
        private readonly Dictionary<Type, object> _compilationLocks = new Dictionary<Type, object>();

        private readonly Dictionary<Type, ObjectConfigurationBase> _configurations = new Dictionary<Type, ObjectConfigurationBase>();
        
        public IObjectConfiguration<T> Configure<T>() where T : class
        {
            var type = typeof(T);
            if (_configurations.ContainsKey(type))
            {
                throw new Exception($"The type {type.Name} is already configured.");
            }
            var typeConfiguration = new ObjectConfiguration<T>(this);
            _configurations[type] = typeConfiguration;
            return typeConfiguration;
        }

        public IList<Difference> Compare<T>(T oldObject, T newObject) where T : class
        {
            return Get<T>()(oldObject, newObject);
        }

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
        ObjectConfigurationBase IBuilderEngine.GetObjectConfiguration(Type type)
        {
            if (_configurations.ContainsKey(type))
            {
                return _configurations[type];
            }
            var configuration = new DefaultConfiguration();
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
