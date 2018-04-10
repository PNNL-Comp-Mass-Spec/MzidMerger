using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRISM;

namespace MzidMerger
{
    public class Options
    {
        [Option("inDir", ArgPosition = 1, Required = true, HelpText = "Path to directory containing mzid files to be merged.", HelpShowsDefault = false)]
        public string InputDirectory { get; set; }

        [Option("filter", HelpText = "Filename filter; filenames that match this string will be merged. *.mzid and *.mzid.gz are added if an extension is not present. Use '*' for wildcard matches. (Default: All files ending in .mzid or .mzid.gz).", HelpShowsDefault = false)]
        public string NameFilter { get; set; }

        [Option("out", HelpText = "Filepath/filename of output file; if no path, input directory is used; by default will determine and use the common portion of the input file names.", HelpShowsDefault = false)]
        public string OutputFilePath { get; set; }

        [Option("maxSpecEValue", HelpText = "Maximum SpecEValue to include in the merged file. Default value includes all results.", Min = 1e-30)]
        public double MaxSpecEValue { get; set; }

        [Option("keepOnlyBestResults", HelpText = "If specified, only the best-scoring results for each spectrum are kept.")]
        public bool KeepOnlyBestResults { get; set; }

        [Option("highmem", Hidden = true, HelpText = "If specified, extra-high resource usage will be allowed, for a minor runtime advantage.")]
        public bool AllowHighResourceUsage { get; set; }

        [Option("threads", Hidden = true, HelpText = "Max number of threads to use.")]
        public int MaxThreads { get; set; }

        public List<string> FilesToMerge { get; } = new List<string>();

        public Options()
        {
            InputDirectory = "";
            NameFilter = "";
            MaxSpecEValue = 100;
            KeepOnlyBestResults = false;
            AllowHighResourceUsage = false;
            MaxThreads = GetOptimalMaxThreads();
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(InputDirectory))
            {
                Console.WriteLine("ERROR: Input directory must be specified!");
                return false;
            }



            return false;
        }

        private int GetOptimalMaxThreads()
        {
            var cores = SystemInfo.GetCoreCount();
            var threads = SystemInfo.GetLogicalCoreCount();
            if (cores == threads)
            {
                return cores - 1;
            }

            if (cores > 4)
            {
                return threads - 2;
            }

            return threads - 1;
        }
    }
}
