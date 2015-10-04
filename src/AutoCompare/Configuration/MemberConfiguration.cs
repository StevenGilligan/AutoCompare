namespace AutoCompare.Configuration
{
    internal class MemberConfiguration : IMemberConfiguration
    {
        public bool Ignored { get; private set; } = false;

        public IMemberConfiguration Ignore()
        {
            Ignored = true;
            return this;
        }
    }
}
