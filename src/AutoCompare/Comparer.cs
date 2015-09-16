using AutoCompare.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoCompare
{
    public static class Comparer
    {
        private static readonly Type _updateType = typeof(Update);
        private static readonly Type _updateListType = typeof(List<Update>);
        private static readonly Type _genericIEnumerableType = typeof(IEnumerable<>);
        private static readonly Type _genericIDictionaryType = typeof(IDictionary<,>);

        private static readonly MemberInfo _setName;
        private static readonly MemberInfo _setOldValue;
        private static readonly MemberInfo _setNewValue;
        private static readonly MethodInfo _listAdd;
        private static readonly MethodInfo _listAddRange;
        
        private static readonly Dictionary<Type, object> _comparerCache = new Dictionary<Type, object>();

        private static readonly Dictionary<Type, ObjectConfigurationBase> _configurations = new Dictionary<Type, ObjectConfigurationBase>();

#if DEBUG
        /// <summary>
        /// Contains the debug version of the compiled comparer lambdas
        /// </summary>
        internal static readonly IDictionary<string, string> DebugInfo = new Dictionary<string, string>();
#endif

        /// <summary>
        /// A method that takes two arguments of type T and returns a
        /// list of updated properties
        /// </summary>
        /// <typeparam name="T">Type of object to compare</typeparam>
        /// <param name="oldModel">The model containing old values</param>
        /// <param name="newModel">The model containing updated values</param>
        /// <returns>A list of values that have been updated between
        /// the old model and the new model</returns>
        public delegate IList<Update> CompiledComparer<T>(T oldModel, T newModel);

        static Comparer()
        {
            _setName = _updateType.GetMember("Name")[0];
            _setOldValue = _updateType.GetMember("OldValue")[0];
            _setNewValue = _updateType.GetMember("NewValue")[0];
            _listAdd = _updateListType.GetMethod("Add");
            _listAddRange = _updateListType.GetMethod("AddRange");
        }

        /// <summary>
        /// Configures how the ObjectComparer should handle comparison of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectConfiguration<T> Configure<T>() where T : class
        {
            var type = typeof(T);
            if (_configurations.ContainsKey(type))
            {
                throw new Exception($"The type {type.Name} is already configured.");
            }
            var typeConfiguration = new ObjectConfiguration<T>();
            _configurations[type] = typeConfiguration;
            return typeConfiguration;
        }

        /// <summary>
        /// Returns a CompiledComparer of type T that can generate a list of 
        /// Updates when comparing two objects of the same type.
        /// Creates a new CompiledComparer and compiles it the first time
        /// this method is called. Returns a cached version afterwards. 
        /// </summary>
        /// <typeparam name="T">Type of object to compare</typeparam>
        /// <returns>CompiledComparer</returns>
        public static CompiledComparer<T> GetComparer<T>() where T : class
        {
            var type = typeof(T);

            object comparer;
            if (_comparerCache.TryGetValue(type, out comparer))
            {
                return (CompiledComparer<T>)comparer;
            }

            var ctx = new CompilerContext()
            {
                A = Expression.Parameter(type, "a"),
                B = Expression.Parameter(type, "b"),
                List = Expression.Variable(_updateListType, "list"),
            };
            var retLabel = Expression.Label(_updateListType);

            // Initialize the List<ModelUpdate> variable
            var blocks = new List<Expression> {
                Expression.Assign(ctx.List, Expression.New(_updateListType)),
            };

            // Expression blocks to compare the objects
            var expression = GetExpressionsForType(type, ctx, new HashSet<Type>());
            if (expression != null)
            {
                blocks.Add(expression);
            }
            // Return the List<ModelUpdate>
            blocks.Add(Expression.Label(retLabel, ctx.List));

            // Compile the expression blocks to a lambda expression
            var body = Expression.Block(_updateListType, new[] { ctx.List }, blocks);
            var comparator = Expression.Lambda<Func<T, T, List<Update>>>(body, (ParameterExpression)ctx.A, (ParameterExpression)ctx.B);
            var newComparer = new CompiledComparer<T>(comparator.Compile());
            _comparerCache[type] = newComparer;
#if DEBUG
            try
            {
                var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
                DebugInfo.Add(type.FullName, propertyInfo.GetValue(comparator) as string);
            }
            catch
            {
                // Do nothing
            }
#endif
            return newComparer;
        }

        /// <summary>
        /// Compares two objects of type T and returns a list
        /// of Update for every property that was updated
        /// </summary>
        /// <typeparam name="T">Type of object to compare</typeparam>
        /// <param name="oldModel">The object containing old values</param>
        /// <param name="newModel">The object containing updated values</param>
        /// <returns>A list of values that have been updated between
        /// the old model and the new model</returns>
        public static IList<Update> Compare<T>(T oldModel, T newModel) where T : class
        {
            return GetComparer<T>()(oldModel, newModel);
        }

        /// <summary>
        /// Creates the list of expressions required to compile a lambda 
        /// to compare two objects of type T
        /// </summary>
        /// <param name="type">Type to compare</param>
        /// <param name="ctx">Compiler Context containing the required expressions</param>
        /// <param name="hierarchy">Parent types to avoid circular references like Parent.Child.Parent</param>
        /// <returns></returns>
        private static Expression GetExpressionsForType(Type type, CompilerContext ctx, HashSet<Type> hierarchy)
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
            var objectConiguration = GetObjectConfiguration(type);

            var expressions = new List<Expression>();

            foreach (var prop in properties)
            {
                var propMethodInfo = prop.GetGetMethod();
                var propType = prop.PropertyType;
                if (objectConiguration.IsIgnored(propMethodInfo))
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
                    var isDeepCompare = objectConiguration.IsDeepCompare(propMethodInfo);
                    expressions.Add(GetIDictionaryPropertyExpression(ctx, isDeepCompare, propType));
                }
                else if (propType.GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == _genericIEnumerableType))
                {
                    var listConfiguration = objectConiguration.GetListConfiguration(propMethodInfo);
                    expressions.Add(GetIEnumerablePropertyExpression(ctx, listConfiguration, propType));
                }
                else
                {
                    // Recursively compare nested types
                    expressions.Add(GetSafeguardedRecursiveExpression(propType, ctx, propMethodInfo, hierarchy));
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
        /// Gets the ObectConfiguration for the specified type or returns a DefaultConfiguration
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ObjectConfigurationBase GetObjectConfiguration(Type type)
        {
            if (_configurations.ContainsKey(type))
            {
                return _configurations[type];
            }
            var configuration = new DefaultConfiguration();
            _configurations[type] = configuration;
            return configuration;
        }

        /// <summary>
        /// Generates the Expression Tree required to test for null and then
        /// recursively test a nested object in the current model
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ctx"></param>
        /// <param name="propMethodInfo"></param>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        private static Expression GetSafeguardedRecursiveExpression(Type type, CompilerContext ctx, MethodInfo propMethodInfo, HashSet<Type> hierarchy)
        {
            var tempA = Expression.Parameter(propMethodInfo.ReturnType, "tempA");
            var tempB = Expression.Parameter(propMethodInfo.ReturnType, "tempB");
            var nullChecked = GetNullCheckedProperties(ctx, propMethodInfo.ReturnType);

            var blockExpressions = new List<Expression>()
            {
                Expression.Assign(tempA, nullChecked.PropA),
                Expression.Assign(tempB, nullChecked.PropB),
            };

            var recursiveCtx = new CompilerContext()
            {
                A = tempA,
                B = tempB,
                Name = ctx.Name,
                List = ctx.List,
            };

            var expression = GetExpressionsForType(type, recursiveCtx, hierarchy);
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
        private static MethodCallExpression GetIDictionaryPropertyExpression(CompilerContext ctx, bool isDeepCompare, Type propType)
        {
            var nullChecked = GetNullCheckedProperties(ctx, propType);

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
        private static Expression GetIEnumerablePropertyExpression(CompilerContext ctx, EnumerableConfigurationBase listConfiguration, Type propType)
        {
            var nullChecked = GetNullCheckedProperties(ctx, propType);

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

        /// <summary>
        /// Generates the Expression Tree required to test, compare a property 
        /// and return a ModelUpdate if needed
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="propMethodInfo"></param>
        /// <returns></returns>
        private static Expression GetPropertyCompareExpression(CompilerContext ctx, PropertyInfo property)
        {
            /* The following expression tree compiles to essentially this : 
             * 
             * if ((a == null || b == null) && a != b || a.x != b.x)
             * {
             *   _list.Add(new ModelUpdate(){
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
        /// Wraps PropA and PropB from the context into ternary operators (ex : a == null ? (PropType)null : a.Prop)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="propType"></param>
        /// <returns></returns>
        private static NullChecked GetNullCheckedProperties(CompilerContext ctx, Type propType)
        {
            var nullConst = Expression.Constant(null);
            var typedNullConst = Expression.Convert(nullConst, propType);
            return new NullChecked
            {
                PropA = Expression.Condition(Expression.Equal(ctx.A, nullConst), typedNullConst, ctx.PropA),
                PropB = Expression.Condition(Expression.Equal(ctx.B, nullConst), typedNullConst, ctx.PropB)
            };
        }

        /// <summary>
        /// Used to pass around a context between methods and recursively
        /// </summary>
        private struct CompilerContext
        {
            /// <summary>
            /// Name of the current property
            /// </summary>
            public string Name;

            /// <summary>
            /// Old model
            /// </summary>
            public Expression A;

            /// <summary>
            /// New model
            /// </summary>
            public Expression B;

            /// <summary>
            /// List of ModelUpdate
            /// </summary>
            public ParameterExpression List;

            /// <summary>
            /// Property accessor on old model
            /// </summary>
            public MemberExpression PropA;

            /// <summary>
            /// Property accessor on new model
            /// </summary>
            public MemberExpression PropB;
        }

        /// <summary>
        /// Only because it's cleaner than using Tuples
        /// </summary>
        private class NullChecked
        {
            public Expression PropA { get; set; }
            public Expression PropB { get; set; }
        }
    }
}