using AutoCompare.Helpers;
using System;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    internal class EnumerableConfiguration : PropertyConfiguration
    {
        public Expression Matcher { get; protected set; }
        public Type MatcherType { get; protected set; }
        public string Match { get; protected set; }
        public object DefaultId { get; protected set; }
    }

    internal class EnumerableConfiguration<T> : EnumerableConfiguration, IEnumerableConfiguration<T> where T : class
    {
        public IEnumerableConfiguration<T> MatchUsing<TProp>(Expression<Func<T, TProp>> member)
        {
            return MatchUsing(member, default(TProp));
        }

        public IEnumerableConfiguration<T> MatchUsing<TProp>(Expression<Func<T, TProp>> member, TProp defaultId)
        {
            DefaultId = defaultId;
            Match = ReflectionHelper.GetPropertyGetterMemberInfo(member).Name;
            Matcher = member;
            MatcherType = typeof(TProp);
            return this;
        }
    }
}
