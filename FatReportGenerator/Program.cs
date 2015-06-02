using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FatReportGenerator.PingoEmbeddedAssemblies;

namespace FatReportGenerator
{

    class EmbeddedResourceAssemblyAccumulator : Palmmedia.ReportGenerator.Reporting.IAssemblyAccumulator
    {
        private static List<Assembly> _embeddedAssemblies;
        public IList<Assembly> Assemblies()
        {
            if (_embeddedAssemblies == null)
            {
                _embeddedAssemblies = new List<Assembly>();
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                _embeddedAssemblies.Add(executingAssembly.AssemblyFromManifestResourceStream("ReportGenerator.Reporting.dll"));
            }
            return _embeddedAssemblies;
        }
    }
    class Program
    {

        /// <summary>
        /// The main method.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Return code indicating success/failure.</returns>
        static int Main(string[] args)
        {
            
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(FatReportGenerator.PingoEmbeddedAssemblies.AssemblyResolver.OnResolveAssembly);
            return MainCore(args);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int MainCore(string[] args)
        {
            // important, we have to more our real code here so that our Main gets called before demands are made on our non-existing on disk referenced assemblies.
            // Our assemblies come from the above AssemblyResolve
            // if you move the following code " new EmbeddedResourceAssemblyAccumulator();  
            // to the above Main, the app will crash as it can't resolve the referenced assemblies.
            Palmmedia.ReportGenerator.Reporting.MefReportBuilderFactory.AssemblyAccumulator = new EmbeddedResourceAssemblyAccumulator();
            return Palmmedia.ReportGenerator.ProgramCore.Main(args);
        }
    }
}
