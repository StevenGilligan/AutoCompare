using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoCompare.Helpers
{
    internal class ReflectionHelper
    {
        /// <summary>
        /// Gets the MemberInfo that corresponds to the Getter method on a property from an expression
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MemberInfo GetPropertyGetterMemberInfo<T1, T2>(Expression<Func<T1, T2>> expression)
        {
            MemberExpression member = null;
            switch (expression.Body.NodeType)
            {
                case ExpressionType.Convert:
                    var method = (UnaryExpression)expression.Body;
                    member = (MemberExpression)method.Operand;
                    break;
                case ExpressionType.MemberAccess:
                    member = (MemberExpression)expression.Body;
                    break;
            }
            if (member == null) throw new Exception("Expected a property getter");

            var propertyInfo = member.Member as PropertyInfo;
            return propertyInfo != null ? propertyInfo.GetMethod : member.Member;
        }
    }
}
