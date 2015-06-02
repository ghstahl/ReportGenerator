using System.Collections.Generic;
using System.Reflection;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Interface used to abstract where Assemblies come from.  i.e. FileSystem, Embedded, etc.
    /// </summary>
    public interface IAssemblyAccumulator
    {
        /// <summary>
        /// The List of loaded assemblies
        /// </summary>
        /// <returns>A list of Assemblies</returns>
        IList<Assembly> Assemblies();
    }
}