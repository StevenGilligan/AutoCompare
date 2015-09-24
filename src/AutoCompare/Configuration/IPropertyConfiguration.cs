using System;

namespace AutoCompare.Configuration
{
    /// <summary>
    /// Configures how the engine handles a specified property
    /// </summary>
    public interface IPropertyConfiguration
    {
        /// <summary>
        /// Ignores the specified property when comparing this type
        /// </summary>
        /// <returns></returns>
        IPropertyConfiguration Ignore();
    }
}
