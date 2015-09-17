using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCompare.Configuration
{
    public interface IPrecompile
    {
        /// <summary>
        /// Precompile the comparator synchronously
        /// </summary>
        void Now();

        /// <summary>
        /// Start the precompilation asynchronously
        /// </summary>
        void Async();
    }
}
