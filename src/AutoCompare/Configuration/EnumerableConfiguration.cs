using System;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    internal class EnumerableConfiguration<TParent, TEnumerable> : EnumerableConfigurationBase, IEnumerableConfiguration<TParent, TEnumerable>
    {
        private readonly IObjectConfiguration<TParent> _parent;

        public EnumerableConfiguration(IObjectConfiguration<TParent> parent)
        {
            _parent = parent;
        }

        public IObjectConfiguration<TParent> DeepCompare<TKey>(Expression<Func<TEnumerable, TKey>> keyExpression)
        {
            KeySelector = keyExpression;
            KeyType = typeof(TKey);
            return _parent;
        }

        public IObjectConfiguration<TParent> DeepCompare<TKey>(Expression<Func<TEnumerable, TKey>> keyExpression, TKey keyDefaultValue)
        {
            KeyDefaultValue = keyDefaultValue;
            return DeepCompare(keyExpression);
        }
    }
}
