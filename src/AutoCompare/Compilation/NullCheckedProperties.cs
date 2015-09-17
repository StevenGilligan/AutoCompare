using System;
using System.Linq.Expressions;

namespace AutoCompare.Compilation
{
    /// <summary>
    /// Only because it's cleaner than using Tuples
    /// </summary>
    internal class NullChecked
    {
        public Expression PropA { get; set; }
        public Expression PropB { get; set; }

        /// <summary>
        /// Wraps PropA and PropB from the context into ternary operators (ex : a == null ? (PropType)null : a.Prop)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="propType"></param>
        /// <returns></returns>
        public NullChecked(Context ctx, Type propType)
        {
            var nullConst = Expression.Constant(null);
            var typedNullConst = Expression.Convert(nullConst, propType);

            PropA = Expression.Condition(Expression.Equal(ctx.A, nullConst), typedNullConst, ctx.PropA);
            PropB = Expression.Condition(Expression.Equal(ctx.B, nullConst), typedNullConst, ctx.PropB);
        }
    }
}
