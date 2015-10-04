using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoCompare.Helpers
{
    internal class ReflectionHelper
    {
        /// <summary>
        /// Gets the MemberInfo for this Property or Field
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MemberInfo GetMemberInfo<T1, T2>(Expression<Func<T1, T2>> expression)
        {
            MemberExpression member = null;
            switch (expression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    member = (MemberExpression)expression.Body;
                    break;
            }
            if (member == null) throw new Exception("Expected a member");

            var memberInfo = member.Member;
            if (memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property) throw new Exception("Expected a field or property getter");

            return memberInfo;
        }
    }
}
