using System.Collections.Generic;
using System.Reflection;

namespace AutoCompare.Configuration
{
    internal class ObjectConfigurationBase
    {
        protected readonly HashSet<MemberInfo> _ignored = new HashSet<MemberInfo>();
        protected readonly HashSet<MemberInfo> _deepCompare = new HashSet<MemberInfo>();

        protected readonly Dictionary<MemberInfo, EnumerableConfigurationBase> _lists = new Dictionary<MemberInfo, EnumerableConfigurationBase>();

        public bool IsIgnored(MemberInfo member)
        {
            return _ignored.Contains(member);
        }

        public bool IsDeepCompare(MemberInfo member)
        {
            return _deepCompare.Contains(member);
        }

        public EnumerableConfigurationBase GetListConfiguration(MemberInfo member)
        {
            return _lists.ContainsKey(member) ? _lists[member] : null;
        }
    }
}
