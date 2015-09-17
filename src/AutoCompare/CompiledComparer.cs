using System.Collections.Generic;

namespace AutoCompare
{
    /// <summary>
    /// A method that takes two arguments of type T and returns the list of differences between them
    /// </summary>
    /// <typeparam name="T">Type of objects to compare</typeparam>
    /// <param name="oldObject">The object containing old values</param>
    /// <param name="newObject">The object containing new values</param>
    /// <returns>The list of differences found between the two objects</returns>
    public delegate IList<Difference> CompiledComparer<T>(T oldObject, T newObject);
}
