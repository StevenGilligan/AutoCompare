using System.Linq.Expressions;

namespace AutoCompare.Compilation
{
    /// <summary>
    /// Used to pass around a context between methods and recursively
    /// </summary>
    internal struct Context
    {
        /// <summary>
        /// Name of the current property
        /// </summary>
        public string Name;

        /// <summary>
        /// Old object
        /// </summary>
        public Expression A;

        /// <summary>
        /// New object
        /// </summary>
        public Expression B;

        /// <summary>
        /// List of Difference
        /// </summary>
        public ParameterExpression List;

        /// <summary>
        /// Property accessor on old object
        /// </summary>
        public MemberExpression PropA;

        /// <summary>
        /// Property accessor on new object
        /// </summary>
        public MemberExpression PropB;
    }
}
