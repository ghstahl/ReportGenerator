using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Implementation of <see cref="IReportBuilderFactory"/> based on MEF.
    /// </summary>
    public class MefReportBuilderFactory : IReportBuilderFactory
    {
        private static IAssemblyAccumulator _assemblyAccumulator;
        public static IAssemblyAccumulator AssemblyAccumulator
        {
            get
            {
                if (_assemblyAccumulator == null)
                {
                    var directoryName = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
                    _assemblyAccumulator = new FileSystemLocalAssemblyAccumulator(directoryName);
                    
                }
                return _assemblyAccumulator;
                
            }
            set { _assemblyAccumulator = value; }
        }
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MefReportBuilderFactory));

        /// <summary>
        /// Gets the available report types.
        /// </summary>
        /// <returns>
        /// The available report types.
        /// </returns>
        public IEnumerable<string> GetAvailableReportTypes()
        {
            var reportBuilders = LoadReportBuilders();

            return reportBuilders
                .Select(r => r.ReportType)
                .Distinct()
                .OrderBy(r => r)
                .ToArray();
        }

        /// <summary>
        /// Gets the report builders that correspond to the given <paramref name="reportTypes" />.
        /// </summary>
        /// <param name="targetDirectory">The target directory where reports are stored.</param>
        /// <param name="reportTypes">The report types.</param>
        /// <returns>
        /// The report builders.
        /// </returns>
        public IEnumerable<IReportBuilder> GetReportBuilders(string targetDirectory, IEnumerable<string> reportTypes)
        {
            Logger.InfoFormat(Resources.InitializingReportBuilders, string.Join(", ", reportTypes));

            var reportBuilders = LoadReportBuilders()
                .Where(r => reportTypes.Contains(r.ReportType, StringComparer.OrdinalIgnoreCase))
                .OrderBy(r => r.ReportType)
                .ToArray();

            var result = new List<IReportBuilder>();

            foreach (var reportBuilderGroup in reportBuilders.GroupBy(r => r.ReportType))
            {
                if (reportBuilderGroup.Count() == 1)
                {
                    result.Add(reportBuilderGroup.First());
                }
                else
                {
                    var nonDefaultParsers = reportBuilderGroup
                        .Where(r => r.GetType().Assembly.GetName().Name != "ReportGenerator.Reporting")
                        .ToArray();

                    foreach (var reportBuilder in nonDefaultParsers)
                    {
                        result.Add(reportBuilder);
                    }

                    if (nonDefaultParsers.Length > 1)
                    {
                        Logger.WarnFormat(" " + Resources.SeveralCustomReportBuildersWithSameReportType, reportBuilderGroup.Key);
                    }

                    if (nonDefaultParsers.Length < reportBuilderGroup.Count())
                    {
                        Logger.WarnFormat(" " + Resources.DefaultReportBuilderReplaced, reportBuilderGroup.Key);
                    }
                }
            }

            foreach (var reportBuilder in result)
            {
                reportBuilder.TargetDirectory = targetDirectory;
            }

            return result;
        }

        /// <summary>
        /// Loads the report builders.
        /// </summary>
        /// <returns>The report builders.</returns>
        private static IEnumerable<IReportBuilder> LoadReportBuilders()
        {
            AggregateCatalog aggregateCatalog = new AggregateCatalog();

            foreach (var assembly in AssemblyAccumulator.Assemblies())
            {
                var assemblyCatalog = new AssemblyCatalog(assembly);
                assemblyCatalog.Parts.ToArray(); // This may throw ReflectionTypeLoadException 
                aggregateCatalog.Catalogs.Add(assemblyCatalog);
            }


            using (var container = new CompositionContainer(aggregateCatalog))
            {
                var reportBuilders = container.GetExportedValues<IReportBuilder>();
                Console.WriteLine(string.Format("Found:{0}",reportBuilders.Count()));
                return reportBuilders;
            }
        }
    }
}
