using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoCompare.Helpers;
using AutoCompare.Compilation;
using System.Threading.Tasks;

namespace AutoCompare.Configuration
{
    internal class ObjectConfiguration<T> : ObjectConfigurationBase, IObjectConfiguration<T>, IPrecompile where T : class
    {
        private IBuilderEngine _engine;

        public ObjectConfiguration(IBuilderEngine engine)
        {
            _engine = engine;
        }

        public IObjectConfiguration<T> Ignore(Expression<Func<T, object>> ignoreExpression)
        {
            _ignored.Add(ReflectionHelper.GetPropertyGetterMemberInfo(ignoreExpression));

            return this;
        }

        public IObjectConfiguration<T> Enumerable<TEnumerable>(Expression<Func<T, IEnumerable<TEnumerable>>> listExpression, Action<IEnumerableConfiguration<T, TEnumerable>> configureList)
        {
            var property = ReflectionHelper.GetPropertyGetterMemberInfo(listExpression);
            if (_lists.ContainsKey(property))
            {
                throw new Exception("This list property is already configured.");
            }
            var propertyConfiguration = new EnumerableConfiguration<T, TEnumerable>(this);
            _lists[property] = propertyConfiguration;

            configureList?.Invoke(propertyConfiguration);

            return this;
        }

        public IObjectConfiguration<T> DeepCompare<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> propertyExpression)
        {
            var property = ReflectionHelper.GetPropertyGetterMemberInfo(propertyExpression);
            if (_deepCompare.Contains(property))
            {
                throw new Exception("This dictionary property is already configured.");
            }
            _deepCompare.Add(property);
            return this;
        }

        public IPrecompile Compile {
            get
            {
                return this;
            }
        }

        void IPrecompile.Now()
        {
            _engine.Compile<T>();
        }

        void IPrecompile.Async()
        {
            new Task(() => Compile.Now()).Start();
        }
    }
}
