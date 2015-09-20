namespace AutoCompare
{
    /// <summary>
    /// The results of a comparing a property on two objects of the same type
    /// </summary>
    public class Difference
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the property in the old object
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// Value of the property in the new object
        /// </summary>
        public object NewValue { get; set; }
    }
}
