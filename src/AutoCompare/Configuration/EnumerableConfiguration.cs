using System;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    internal class EnumerableConfiguration<TParent, TEnumerable> : EnumerableConfigurationBase, IEnumerableConfiguration<TParent, TEnumerable>
    {
        private readonly IComparerConfiguration<TParent> _parent;

        public EnumerableConfiguration(IComparerConfiguration<TParent> parent)
        {
            _parent = parent;
        }

        public IComparerConfiguration<TParent> DeepCompare<TKey>(Expression<Func<TEnumerable, TKey>> keyExpression)
        {
            KeySelector = keyExpression;
            KeyType = typeof(TKey);
            return _parent;
        }

        public IComparerConfiguration<TParent> DeepCompare<TKey>(Expression<Func<TEnumerable, TKey>> keyExpression, TKey keyDefaultValue)
        {
            KeyDefaultValue = keyDefaultValue;
            return DeepCompare(keyExpression);
        }
    }
}
