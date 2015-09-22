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

        protected readonly HashSet<MemberInfo> _deepCompare = new HashSet<MemberInfo>();

        protected readonly Dictionary<MemberInfo, EnumerableConfigurationBase> _lists = new Dictionary<MemberInfo, EnumerableConfigurationBase>();

        public bool IsDeepCompare(MemberInfo member)
        {
            return _deepCompare.Contains(member);
        }

        public EnumerableConfigurationBase GetListConfiguration(MemberInfo member)
        {
            return _lists.ContainsKey(member) ? _lists[member] : null;
        }

        public PropertyConfiguration GetPropertyConfiguration(string property)
        {
            return _propertyConfigs.ContainsKey(property) ? _propertyConfigs[property] : new PropertyConfiguration();
        }
    }

    internal class ComparerConfiguration<T> : ComparerConfiguration, IComparerConfiguration<T>, IPrecompile where T : class
    {
        private IBuilderEngine _engine;

        public ComparerConfiguration(IBuilderEngine engine)
        {
            _engine = engine;
        }

        public IComparerConfiguration<T> For<TProp>(Expression<Func<T, TProp>> member, Action<IPropertyConfiguration> configuration)
        {
            var memberInfo = ReflectionHelper.GetPropertyGetterMemberInfo(member);
            if (_propertyConfigs.ContainsKey(memberInfo.Name))
            {
                throw new Exception("This property is already configured.");
            }
            var property = new PropertyConfiguration();
            configuration(property);
            _propertyConfigs.Add(memberInfo.Name, property);
            return this;
        }

        public IComparerConfiguration<T> Ignore(Expression<Func<T, object>> ignoreExpression)
        {
            For(ignoreExpression, x => x.Ignore());
            return this;
        }

        public IComparerConfiguration<T> Enumerable<TEnumerable>(Expression<Func<T, IEnumerable<TEnumerable>>> listExpression, Action<IEnumerableConfiguration<T, TEnumerable>> configureList)
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

        public IComparerConfiguration<T> DeepCompare<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> propertyExpression)
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
