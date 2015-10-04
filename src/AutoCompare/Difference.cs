using System;

namespace AutoCompare
{
    /// <summary>
    /// The results of a comparing a member on two objects of the same type
    /// </summary>
    public class Difference : IEquatable<Difference>
    {
        /// <summary>
        /// Name of the member
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the member in the old object
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// Value of the member in the new object
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Difference other)
        {
            if (other == null)
            {
                return false;
            }
            return Name == other.Name &&
                   Equals(OldValue, other.OldValue) &&
                   Equals(NewValue, other.NewValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Difference);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Name.GetHashCode();
                hash = hash * 23 + OldValue.GetHashCode();
                hash = hash * 23 + NewValue.GetHashCode();
                return hash;
            }
        }
    }
}
