namespace AutoCompare.Configuration
{
    internal class PropertyConfiguration : IPropertyConfiguration
    {
        public bool Ignored { get; private set; } = false;

        public IPropertyConfiguration Ignore()
        {
            Ignored = true;
            return this;
        }
    }
}
