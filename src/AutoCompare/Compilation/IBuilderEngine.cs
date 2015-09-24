using AutoCompare.Configuration;
using System;

namespace AutoCompare.Compilation
{
    internal interface IBuilderEngine : IComparerEngine
    {
        ComparerConfiguration GetObjectConfiguration(Type type);

        void Compile<T>() where T : class;
    }
}
