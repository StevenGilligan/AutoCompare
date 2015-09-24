using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoCompare.Helpers;
using AutoCompare.Compilation;
using System.Threading.Tasks;
using System.Reflection;

namespace AutoCompare.Configuration
{
    internal class ComparerConfiguration
    {
        protected Dictionary<string, PropertyConfiguration> _propertyConfigs = new Dictionary<string, PropertyConfiguration>();
        public IComparerEngine Engine { get; private set; }

        public ComparerConfiguration(IComparerEngine engine)
        {
            Engine = engine;
        }

        public PropertyConfiguration GetPropertyConfiguration(string property)
        {
            return _propertyConfigs.ContainsKey(property) ? _propertyConfigs[property] : new PropertyConfiguration();
        }
    }

    internal class ComparerConfiguration<T> : ComparerConfiguration, IComparerConfiguration<T>, IPrecompile where T : class
    {
        public ComparerConfiguration(IBuilderEngine engine)
            :base(engine)
        {
        }

        public IComparerConfiguration<T> For<TProp>(Expression<Func<T, TProp>> member, Action<IPropertyConfiguration> configuration)
        {
            var memberInfo = ReflectionHelper.GetPropertyGetterMemberInfo(member);
            if (_propertyConfigs.ContainsKey(memberInfo.Name))
            {
                throw new Exception($"The property {memberInfo.Name} is already configured.");
            }
            var property = new PropertyConfiguration();
            configuration(property);
            _propertyConfigs.Add(memberInfo.Name, property);
            return this;
        }

        private IComparerConfiguration<T> ForEnumerableInternal<TProp>(string memberName, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class
        {
            if (_propertyConfigs.ContainsKey(memberName))
            {
                throw new Exception($"The property {memberName} is already configured.");
            }
            var property = new EnumerableConfiguration<TProp>();
            configuration(property);
            _propertyConfigs.Add(memberName, property);
            return this;
        }

        public IComparerConfiguration<T> For<TProp>(Expression<Func<T, IEnumerable<TProp>>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class
        {
            return ForEnumerableInternal(ReflectionHelper.GetPropertyGetterMemberInfo(member).Name, configuration);
        }

        public IComparerConfiguration<T> For<TProp>(Expression<Func<T, List<TProp>>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class
        {
            return ForEnumerableInternal(ReflectionHelper.GetPropertyGetterMemberInfo(member).Name, configuration);
        }

        public IComparerConfiguration<T> For<TProp>(Expression<Func<T, IList<TProp>>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class
        {
            return ForEnumerableInternal(ReflectionHelper.GetPropertyGetterMemberInfo(member).Name, configuration);
        }

        public IComparerConfiguration<T> For<TProp>(Expression<Func<T, TProp[]>> member, Action<IEnumerableConfiguration<TProp>> configuration) where TProp : class
        {
            return ForEnumerableInternal(ReflectionHelper.GetPropertyGetterMemberInfo(member).Name, configuration);
        }

        public IComparerConfiguration<T> Ignore<TProp>(Expression<Func<T, TProp>> ignoreExpression)
        {
            For(ignoreExpression, x => x.Ignore());
            return this;
        }

        //public IComparerConfiguration<T> Enumerable<TEnumerable>(Expression<Func<T, IEnumerable<TEnumerable>>> listExpression, Action<IEnumerableConfiguration<T, TEnumerable>> configureList) where TEnumerable : class
        //{
        //    var property = ReflectionHelper.GetPropertyGetterMemberInfo(listExpression);
        //    if (_lists.ContainsKey(property))
        //    {
        //        throw new Exception("This list property is already configured.");
        //    }
        //    var propertyConfiguration = new EnumerableConfiguration<T, TEnumerable>(this);
        //    _lists[property] = propertyConfiguration;

        //    configureList?.Invoke(propertyConfiguration);

        //    return this;
        //}

        //public IComparerConfiguration<T> DeepCompare<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> propertyExpression)
        //{
        //    var property = ReflectionHelper.GetPropertyGetterMemberInfo(propertyExpression);
        //    if (_deepCompare.Contains(property))
        //    {
        //        throw new Exception("This dictionary property is already configured.");
        //    }
        //    _deepCompare.Add(property);
        //    return this;
        //}

        public IPrecompile Compile {
            get
            {
                return this;
            }
        }

        void IPrecompile.Now()
        {
            ((IBuilderEngine)Engine).Compile<T>();
        }

        void IPrecompile.Async()
        {
            new Task(() => Compile.Now()).Start();
        }
    }
}
