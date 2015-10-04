namespace AutoCompare.Configuration
{
    /// <summary>
    /// Configures how the engine handles a specified member
    /// </summary>
    public interface IMemberConfiguration
    {
        /// <summary>
        /// Ignores the specified member when comparing this type
        /// </summary>
        /// <returns></returns>
        IMemberConfiguration Ignore();
    }
}
