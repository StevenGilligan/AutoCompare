namespace AutoCompare
{
    public class Update
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Outdated value of the property in the old model
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// Updated value of the property in the new model
        /// </summary>
        public object NewValue { get; set; }
    }
}
