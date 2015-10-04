using AutoCompare.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoCompare.Compilation
{
    internal static class Builder
    {
        private static readonly Type _updateType = typeof(Difference);
        private static readonly Type _updateListType = typeof(List<Difference>);
        private static readonly Type _genericIEnumerableType = typeof(IEnumerable<>);
        private static readonly Type _genericIDictionaryType = typeof(IDictionary<,>);

        private static readonly MemberInfo _setName;
        private static readonly MemberInfo _setOldValue;
        private static readonly MemberInfo _setNewValue;
        private static readonly MethodInfo _listAdd;
        private static readonly MethodInfo _listAddRange;
        
#if DEBUG
        /// <summary>
        /// Contains the debug version of the compiled comparer lambdas
        /// </summary>
        internal static readonly IDictionary<string, string> DebugInfo = new Dictionary<string, string>();
#endif

        static Builder()
        {
            _setName = _updateType.GetMember("Name")[0];
            _setOldValue = _updateType.GetMember("OldValue")[0];
            _setNewValue = _updateType.GetMember("NewValue")[0];
            _listAdd = _updateListType.GetMethod("Add");
            _listAddRange = _updateListType.GetMethod("AddRange");
        }

        public static CompiledComparer<T> Build<T>(ComparerConfiguration configuration) where T : class
        {
            var type = typeof(T);
            
            var ctx = new Context()
            {
                ObjectA = Expression.Parameter(type, "a"),
                ObjectB = Expression.Parameter(type, "b"),
                List = Expression.Variable(_updateListType, "list"),
            };
            var retLabel = Expression.Label(_updateListType);

            // Initialize the List<Difference> variable
            var blocks = new List<Expression> {
                Expression.Assign(ctx.List, Expression.New(_updateListType)),
            };

            // Expression blocks to compare the objects
            var expression = GetExpressionsForType(type, ctx, configuration, new HashSet<Type>());
            if (expression != null)
            {
                blocks.Add(expression);
            }
            // Return the List<Difference>
            blocks.Add(Expression.Label(retLabel, ctx.List));

            // Compile the expression blocks to a lambda expression
            var body = Expression.Block(_updateListType, new[] { ctx.List }, blocks);
            var comparator = Expression.Lambda<Func<T, T, List<Difference>>>(body, (ParameterExpression)ctx.ObjectA, (ParameterExpression)ctx.ObjectB);
            var newComparer = new CompiledComparer<T>(comparator.Compile());
#if DEBUG
            try
            {
                var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
                DebugInfo.Add(type.FullName, propertyInfo.GetValue(comparator) as string);
            }
            catch
            {
                // Do nothing, this is debug only
            }
#endif
            return newComparer;
        }

        /// <summary>
        /// Determines if the type should be compared with a simple equality check
        /// </summary>
        /// <param name="type">The type to test</param>
        /// <returns>If we consider this type a value type</returns>
        public static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum ||
                    type.FullName.StartsWith("System") &&
                   (!type.IsGenericType || type.Name.StartsWith("Nullable"));
        }

        /// <summary>
        /// Determines if the type is IEnumerable or implements IEnumerable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIEnumerableType(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == _genericIEnumerableType)
                || type.GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == _genericIEnumerableType);
        }

        /// <summary>
        /// Determines if the type is IDicionary or implements IDictionary
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIDictionaryType(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == _genericIDictionaryType)
                || type.GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == _genericIDictionaryType);
        }

        /// <summary>
        /// Creates the list of expressions required to compile a lambda 
        /// to compare two objects of type T
        /// </summary>
        /// <param name="type">Type to compare</param>
        /// <param name="ctx">Compiler Context containing the required expressions</param>
        /// <param name="configuration">The configuration of the current type</param>
        /// <param name="hierarchy">Parent types to avoid circular references like Parent.Child.Parent</param>
        /// <returns></returns>
        private static Expression GetExpressionsForType(Type type, Context ctx, ComparerConfiguration configuration, HashSet<Type> hierarchy)
        {
            if (hierarchy.Contains(type))
            {
                // Break out of circular reference
                return null;
            }

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                              .Where(x => FilterMember(configuration, x));
            
            // Keep track of types in the hierarchy to avoid circular references
            hierarchy.Add(type);
            var prefix = ctx.Name;

            var expressions = new List<Expression>();

            foreach (var member in members)
            {
                string memberName = member.Name;
                Type memberType = null;

                var property = member as PropertyInfo;
                if (property != null)
                {
                    memberType = property.PropertyType;
                }

                var field = member as FieldInfo;
                if (field != null)
                {
                    memberType = field.FieldType;
                }

                var memberConfiguration = configuration.GetMemberConfiguration(memberName);
                var enumerableConfiguration = memberConfiguration as EnumerableConfiguration;

                ctx.MemberA = Expression.PropertyOrField(ctx.ObjectA, memberName);
                ctx.MemberB = Expression.PropertyOrField(ctx.ObjectB, memberName);
                ctx.Name = string.IsNullOrEmpty(prefix) ? memberName : $"{prefix}.{memberName}";
                if (IsSimpleType(memberType))
                {
                    // ValueType, simply compare value with an if (a.X != b.X) 
                    expressions.Add(GetMemberCompareExpression(ctx, memberName));
                }
                else if (IsIDictionaryType(memberType))
                {
                    // Static call to CollectionComparer.CompareIDictionary<K,V> to compare IDictionary properties
                    expressions.Add(GetIDictionaryMemberExpression(ctx, configuration.Engine, memberType));
                }
                else if (IsIEnumerableType(memberType))
                {
                    expressions.Add(GetIEnumerableMemberExpression(ctx, configuration.Engine, enumerableConfiguration, memberType));
                }
                else
                {
                    // Recursively compare nested types
                    expressions.Add(GetSafeguardedRecursiveExpression(memberType, ctx, memberType, configuration, hierarchy));
                }
            }

            // Pop the current type from the Hierarchy stack
            hierarchy.Remove(type);

            if (!expressions.Any())
            {
                return null; // Object has no properties
            }
            // Check if both objects are null
            return Expression.IfThen(
                Expression.Not(
                    Expression.AndAlso(
                        Expression.Equal(ctx.ObjectA, Expression.Constant(null)),
                        Expression.Equal(ctx.ObjectB, Expression.Constant(null)))),
                Expression.Block(expressions));
        }

        /// <summary>
        /// Filters members based on the configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        private static bool FilterMember(ComparerConfiguration configuration, MemberInfo member)
        {
            var isField = member is FieldInfo;
            var isProperty = member is PropertyInfo;

            if (!isField && !isProperty)
                return false;

            if (isField && !configuration.CompareFields)
                return false;

            var memberConfiguration = configuration.GetMemberConfiguration(member.Name);
            return !memberConfiguration.Ignored;
        }

        /// <summary>
        /// Generates the Expression Tree required to test, compare a member
        /// and return a Difference if needed
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        private static Expression GetMemberCompareExpression(Context ctx, string memberName)
        {
            /* The following expression tree compiles to essentially this : 
             * 
             * if ((a == null || b == null) && a != b || a.x != b.x)
             * {
             *   _list.Add(new Difference(){
             *      Name = name,
             *      OldValue = a == null ? null : a.x,
             *      NewValue = b == null ? null : b.x
             *   });
             * }
             * 
             * */
            var nullConst = Expression.Constant(null);
            return Expression.IfThen(
                        Expression.OrElse(
                            Expression.AndAlso(
                                Expression.OrElse(
                                    Expression.Equal(ctx.ObjectA, nullConst),
                                    Expression.Equal(ctx.ObjectB, nullConst)),
                                Expression.NotEqual(ctx.ObjectA, ctx.ObjectB)),
                            Expression.NotEqual(
                                Expression.PropertyOrField(ctx.ObjectA, memberName),
                                Expression.PropertyOrField(ctx.ObjectB, memberName))),
                        Expression.Call(ctx.List, _listAdd,
                            Expression.MemberInit(
                                Expression.New(_updateType),
                                Expression.Bind(_setName, Expression.Constant(ctx.Name)),
                                Expression.Bind(_setOldValue,
                                     Expression.Condition(
                                        Expression.Equal(ctx.ObjectA, nullConst),
                                        nullConst,
                                        Expression.Convert(ctx.MemberA, typeof(object)))),
                                Expression.Bind(_setNewValue,
                                     Expression.Condition(
                                        Expression.Equal(ctx.ObjectB, nullConst),
                                        nullConst,
                                        Expression.Convert(ctx.MemberB, typeof(object)))))));
        }

        /// <summary>
        /// Generates the Expression Tree required to test for null and then
        /// recursively test a nested object in the current object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ctx"></param>
        /// <param name="memberType"></param>
        /// <param name="configuration"></param>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        private static Expression GetSafeguardedRecursiveExpression(Type type, Context ctx, Type memberType, ComparerConfiguration configuration, HashSet<Type> hierarchy)
        {
            var tempA = Expression.Parameter(memberType, "tempA");
            var tempB = Expression.Parameter(memberType, "tempB");
            var nullChecked = new NullChecked(ctx, memberType);

            var blockExpressions = new List<Expression>()
            {
                Expression.Assign(tempA, nullChecked.PropA),
                Expression.Assign(tempB, nullChecked.PropB),
            };

            var recursiveCtx = new Context()
            {
                ObjectA = tempA,
                ObjectB = tempB,
                Name = ctx.Name,
                List = ctx.List,
            };

            var expression = GetExpressionsForType(type, recursiveCtx, configuration, hierarchy);
            if (expression != null)
            {
                blockExpressions.Add(expression);
            }

            return Expression.Block(
                new[] { tempA, tempB },
                blockExpressions
            );
        }

        /// <summary>
        /// Returns the expression that compares two IDictionary members
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="engine"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        private static MethodCallExpression GetIDictionaryMemberExpression(Context ctx, IComparerEngine engine, Type memberType)
        {
            var nullChecked = new NullChecked(ctx, memberType);

            var genericPropTypes = memberType.GetGenericArguments();

            var methodInfo = CollectionComparer.GetCompareIDictionaryMethodInfo(genericPropTypes);

            if (IsSimpleType(genericPropTypes[1]))
            {
                return Expression.Call(ctx.List,
                    _listAddRange,
                    Expression.Call(
                        methodInfo,
                        Expression.Constant(ctx.Name),
                        nullChecked.PropA,
                        nullChecked.PropB));
            }
            return Expression.Call(ctx.List,
                _listAddRange,
                Expression.Call(
                    methodInfo,
                    Expression.Constant(engine),
                    Expression.Constant(ctx.Name),
                    nullChecked.PropA,
                    nullChecked.PropB));
        }

        /// <summary>
        /// Returns the expression that compares two IEnumerable members
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="engine"></param>
        /// <param name="configuration"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        private static Expression GetIEnumerableMemberExpression(Context ctx, IComparerEngine engine, EnumerableConfiguration configuration, Type memberType)
        {
            var nullChecked = new NullChecked(ctx, memberType);

            if (configuration != null && !string.IsNullOrEmpty(configuration.Match))
            {
                var itemType = memberType.IsArray ? memberType.GetElementType() : memberType.GetGenericArguments().First();
                var types = new[] { itemType, configuration.MatcherType };
                
                // Static call to CollectionComparer.CompareIEnumerableWithKeyAndDefault<T, TKey> to compare IEnumerable properties
                return Expression.Call(ctx.List,
                    _listAddRange,
                    Expression.Call(CollectionComparer.GetCompareIEnumerableWithKeyAndDefaultMethodInfo(types),
                        Expression.Constant(engine),
                        Expression.Constant(ctx.Name),
                        nullChecked.PropA,
                        nullChecked.PropB,
                        configuration.Matcher,
                        Expression.Convert(
                            Expression.Constant(configuration.DefaultId),
                            configuration.MatcherType)));
            }
            // Static call to CollectionComparer.CompareIEnumerable<T> to compare IEnumerable properties
            return Expression.Call(ctx.List,
                _listAddRange,
                Expression.Call(CollectionComparer.GetCompareIEnumerableMethodInfo(memberType.GetGenericArguments()),
                    Expression.Constant(ctx.Name), nullChecked.PropA, nullChecked.PropB));
        }
    }
}
