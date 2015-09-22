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
                A = Expression.Parameter(type, "a"),
                B = Expression.Parameter(type, "b"),
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
            var comparator = Expression.Lambda<Func<T, T, List<Difference>>>(body, (ParameterExpression)ctx.A, (ParameterExpression)ctx.B);
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

            PropertyInfo[] properties = type.GetProperties();

            // Keep track of types in the hierarchy to avoid circular references
            hierarchy.Add(type);
            var prefix = ctx.Name;

            var expressions = new List<Expression>();

            foreach (var prop in properties)
            {
                var propMethodInfo = prop.GetGetMethod();
                var propType = prop.PropertyType;
                var propertyConfiguration = configuration.GetPropertyConfiguration(propMethodInfo.Name);
                if (propertyConfiguration.Ignored)
                {
                    continue;
                }
                ctx.PropA = Expression.Property(ctx.A, prop);
                ctx.PropB = Expression.Property(ctx.B, prop);
                ctx.Name = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                if (propType.IsPrimitive || propType.IsEnum || IsSystemValueType(propType))
                {
                    // ValueType, simply compare value with an if (a.X != b.X) 
                    expressions.Add(GetPropertyCompareExpression(ctx, prop));
                }
                else if (propType.GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == _genericIDictionaryType))
                {
                    // Static call to CollectionComparer.CompareIDictionary<K,V> to compare IDictionary properties
                    var isDeepCompare = configuration.IsDeepCompare(propMethodInfo);
                    expressions.Add(GetIDictionaryPropertyExpression(ctx, isDeepCompare, propType));
                }
                else if (propType.GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == _genericIEnumerableType))
                {
                    var listConfiguration = configuration.GetListConfiguration(propMethodInfo);
                    expressions.Add(GetIEnumerablePropertyExpression(ctx, listConfiguration, propType));
                }
                else
                {
                    // Recursively compare nested types
                    expressions.Add(GetSafeguardedRecursiveExpression(propType, ctx, propMethodInfo, configuration, hierarchy));
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
                        Expression.Equal(ctx.A, Expression.Constant(null)),
                        Expression.Equal(ctx.B, Expression.Constant(null)))),
                Expression.Block(expressions));
        }

        /// <summary>
        /// Generates the Expression Tree required to test, compare a property 
        /// and return a Difference if needed
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static Expression GetPropertyCompareExpression(Context ctx, PropertyInfo property)
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
                                    Expression.Equal(ctx.A, nullConst),
                                    Expression.Equal(ctx.B, nullConst)),
                                Expression.NotEqual(ctx.A, ctx.B)),
                            Expression.NotEqual(
                                Expression.Property(ctx.A, property),
                                Expression.Property(ctx.B, property))),
                        Expression.Call(ctx.List, _listAdd,
                            Expression.MemberInit(
                                Expression.New(_updateType),
                                Expression.Bind(_setName, Expression.Constant(ctx.Name)),
                                Expression.Bind(_setOldValue,
                                     Expression.Condition(
                                        Expression.Equal(ctx.A, nullConst),
                                        nullConst,
                                        Expression.Convert(ctx.PropA, typeof(object)))),
                                Expression.Bind(_setNewValue,
                                     Expression.Condition(
                                        Expression.Equal(ctx.B, nullConst),
                                        nullConst,
                                        Expression.Convert(ctx.PropB, typeof(object)))))));
        }

        /// <summary>
        /// Determines if the property type is a System object type that we should 
        /// compare as if it was a normal value type
        /// </summary>
        /// <param name="propType">The property type to test</param>
        /// <returns>If we consider this property a value type</returns>
        private static bool IsSystemValueType(Type propType)
        {
            return propType.FullName.StartsWith("System") &&
                   (!propType.IsGenericType || propType.Name.StartsWith("Nullable"));
        }

        /// <summary>
        /// Generates the Expression Tree required to test for null and then
        /// recursively test a nested object in the current object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ctx"></param>
        /// <param name="propMethodInfo"></param>
        /// <param name="configuration"></param>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        private static Expression GetSafeguardedRecursiveExpression(Type type, Context ctx, MethodInfo propMethodInfo, ComparerConfiguration configuration, HashSet<Type> hierarchy)
        {
            var tempA = Expression.Parameter(propMethodInfo.ReturnType, "tempA");
            var tempB = Expression.Parameter(propMethodInfo.ReturnType, "tempB");
            var nullChecked = new NullChecked(ctx, propMethodInfo.ReturnType);

            var blockExpressions = new List<Expression>()
            {
                Expression.Assign(tempA, nullChecked.PropA),
                Expression.Assign(tempB, nullChecked.PropB),
            };

            var recursiveCtx = new Context()
            {
                A = tempA,
                B = tempB,
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
        /// Returns the expression that compares two IDictionary properties
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="isDeepCompare"></param>
        /// <param name="propType"></param>
        /// <returns></returns>
        private static MethodCallExpression GetIDictionaryPropertyExpression(Context ctx, bool isDeepCompare, Type propType)
        {
            var nullChecked = new NullChecked(ctx, propType);

            return Expression.Call(ctx.List,
                _listAddRange,
                Expression.Call(
                    CollectionComparer.GetCompareIDictionaryMethodInfo(isDeepCompare, propType.GetGenericArguments()),
                    Expression.Constant(ctx.Name),
                    nullChecked.PropA,
                    nullChecked.PropB));
        }

        /// <summary>
        /// Returns the expression that compares two IEnumerable properties
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="listConfiguration"></param>
        /// <param name="propType"></param>
        /// <returns></returns>
        private static Expression GetIEnumerablePropertyExpression(Context ctx, EnumerableConfigurationBase listConfiguration, Type propType)
        {
            var nullChecked = new NullChecked(ctx, propType);

            if (listConfiguration != null && listConfiguration.IsDeepCompare)
            {
                var types = new[] { propType.GetGenericArguments().First(), listConfiguration.KeyType };

                if (listConfiguration.KeyDefaultValue != null)
                {
                    // Static call to CollectionComparer.CompareIEnumerableWithKeyAndDefault<T, TKey> to compare IEnumerable properties
                    return Expression.Call(ctx.List,
                        _listAddRange,
                        Expression.Call(CollectionComparer.GetCompareIEnumerableWithKeyAndDefaultMethodInfo(types),
                            Expression.Constant(ctx.Name),
                            nullChecked.PropA,
                            nullChecked.PropB,
                            listConfiguration.KeySelector,
                            Expression.Convert(
                                Expression.Constant(listConfiguration.KeyDefaultValue),
                                listConfiguration.KeyType)));
                }
                // Static call to CollectionComparer.CompareIEnumerableWithKey<T, TKey> to compare IEnumerable properties
                return Expression.Call(ctx.List,
                    _listAddRange,
                    Expression.Call(CollectionComparer.GetCompareIEnumerableWithKeyMethodInfo(types),
                        Expression.Constant(ctx.Name),
                        nullChecked.PropA,
                        nullChecked.PropB,
                        listConfiguration.KeySelector));
            }
            // Static call to CollectionComparer.CompareIEnumerable<T> to compare IEnumerable properties
            return Expression.Call(ctx.List,
                _listAddRange,
                Expression.Call(CollectionComparer.GetCompareIEnumerableMethodInfo(propType.GetGenericArguments()),
                    Expression.Constant(ctx.Name), nullChecked.PropA, nullChecked.PropB));
        }
    }
}
