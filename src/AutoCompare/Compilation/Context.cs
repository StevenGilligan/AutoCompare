using System.Linq.Expressions;

namespace AutoCompare.Compilation
{
    /// <summary>
    /// Used to pass around a context between methods and recursively
    /// </summary>
    internal struct Context
    {
        /// <summary>
        /// Name of the current member
        /// </summary>
        public string Name;

        /// <summary>
        /// Old object
        /// </summary>
        public Expression ObjectA;

        /// <summary>
        /// New object
        /// </summary>
        public Expression ObjectB;

        /// <summary>
        /// List of Difference
        /// </summary>
        public ParameterExpression List;

        /// <summary>
        /// Member accessor on old object
        /// </summary>
        public MemberExpression MemberA;

        /// <summary>
        /// Member accessor on new object
        /// </summary>
        public MemberExpression MemberB;
    }
}
