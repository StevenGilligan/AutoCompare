using System;
using System.Linq.Expressions;

namespace AutoCompare.Configuration
{
    internal abstract class EnumerableConfigurationBase
    {
        public Expression KeySelector { get; protected set; }
        public object KeyDefaultValue { get; protected set; }
        public Type KeyType { get; protected set; }

        public bool IsDeepCompare { get { return KeySelector != null; } }
    }
}
