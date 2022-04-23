using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PRISM;

namespace MzidMerger
{
    public class Options
    {
        [Option("inDir", ArgPosition = 1, Required = true, HelpText = "Path to directory containing mzid files to be merged.", HelpShowsDefault = false)]
        public string InputDirectory { get; set; }

        [Option("filter", HelpText = "Filename filter; filenames that match this string will be merged. *.mzid and *.mzid.gz are appended if neither ends the filter string. Use '*' for wildcard matches. (Default: All files ending in .mzid or .mzid.gz).", HelpShowsDefault = false)]
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

        [Option("fixIds", HelpText = "Fix the peptide and peptideEvidence IDs. Only use for e.g. older MS-GF+ results, that output many errors about duplicate IDs. Only fixes Peptide and PeptideEvidence IDs.")]
        public bool FixIDs { get; set; }

        [Option("multithread", HelpText = "If supplied, program will attempt to decrease merge time by reading multiple files in parallel. Will also require more memory, and is more likely to crash.")]
        public bool MultiThread { get; set; }

        public List<string> FilesToMerge { get; } = new List<string>();

        public Options()
        {
            InputDirectory = string.Empty;
            NameFilter = string.Empty;
            MaxSpecEValue = 100;
            KeepOnlyBestResults = false;
            AllowHighResourceUsage = false;
            MaxThreads = GetOptimalMaxThreads();
            FixIDs = false;
            MultiThread = false;
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(InputDirectory))
            {
                Console.WriteLine("ERROR: Input directory must be specified!");
                return false;
            }

            if (!Directory.Exists(InputDirectory))
            {
                Console.WriteLine("ERROR: Input path is not a Directory!");
                return false;
            }

            if (!NameFilter.ToLower().EndsWith(".mzid") && !NameFilter.ToLower().EndsWith(".mzid.gz"))
            {
                FilesToMerge.AddRange(Directory.EnumerateFiles(InputDirectory, NameFilter + "*.mzid"));
                FilesToMerge.AddRange(Directory.EnumerateFiles(InputDirectory, NameFilter + "*.mzid.gz"));
                if (FilesToMerge.Count < 2)
                {
                    Console.WriteLine("ERROR: Not enough files in directory \"{0}\" that matched filter \"{1}\" or \"{2}\": found {3} files.", InputDirectory, NameFilter + "*.mzid", NameFilter + "*.mzid.gz", FilesToMerge.Count);
                    return false;
                }
            }
            else
            {
                FilesToMerge.AddRange(Directory.EnumerateFiles(InputDirectory, NameFilter));
                if (FilesToMerge.Count < 2)
                {
                    Console.WriteLine("ERROR: Not enough files in directory \"{0}\" that matched filter \"{1}\": found {2} files.", InputDirectory, NameFilter, FilesToMerge.Count);
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(OutputFilePath))
            {
                OutputFilePath = GetOutputNameStart(FilesToMerge) + GetOutputNameEnd(FilesToMerge);

                if (File.Exists(OutputFilePath))
                {
                    Console.WriteLine("ERROR: file already exists at the auto-determined output path \"{0}\"!", OutputFilePath);
                    return false;
                }
            }
            else
            {
                if (OutputFilePath.Equals(Path.GetFileName(OutputFilePath)))
                {
                    OutputFilePath = Path.Combine(InputDirectory, OutputFilePath);
                }

                if (!OutputFilePath.ToLower().EndsWith(".mzid") && !OutputFilePath.ToLower().EndsWith(".mzid.gz"))
                {
                    OutputFilePath += ".mzid";
                }

                var dirName = Path.GetDirectoryName(OutputFilePath);
                if (!string.IsNullOrWhiteSpace(dirName) && !Directory.Exists(dirName))
                {
                    try
                    {
                        Directory.CreateDirectory(dirName);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR: Could not create directory for output file: \"{0}\"", dirName);
                        return false;
                    }
                }
            }

            return true;
        }

        public void ReportSettings()
        {
            Console.WriteLine("Input files: (count: {0})", FilesToMerge.Count);
            foreach (var file in FilesToMerge)
            {
                Console.WriteLine("\t\"{0}\"", file);
            }
            Console.WriteLine("Output file: \"{0}\"", OutputFilePath);
            Console.WriteLine("Max SpecEValue: {0}", StringUtilities.DblToString(MaxSpecEValue, 2));
            Console.WriteLine("KeepOnlyBestResults: {0}", KeepOnlyBestResults.ToString());
        }

        public static string GetOutputNameStart(List<string> input)
        {
            var identical = GetLeftIdentical(input);
            if (identical.ToLower().EndsWith("_part"))
            {
                identical = identical.Substring(0, identical.Length - 5);
            }

            return identical.TrimEnd('-', '_');
        }

        public static string GetOutputNameEnd(List<string> input)
        {
            var identical = GetRightIdentical(input);
            if (string.IsNullOrWhiteSpace(identical))
            {
                identical = ".mzid";
            }

            return identical;
        }

        public static string GetRightIdentical(List<string> input)
        {
            return Reverse(GetLeftIdentical(input.Select(Reverse).ToList()));
        }

        private static string Reverse(string s)
        {
            //https://stackoverflow.com/questions/228038/best-way-to-reverse-a-string
            // Note: won't handle UTF-32 (no surprise) or UTF-16 combining characters (may be a surprise) properly
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string GetLeftIdentical(List<string> input)
        {
            if (input.Count == 0)
            {
                return string.Empty;
            }

            if (input.Count == 1)
            {
                return input[0];
            }

            // https://stackoverflow.com/questions/30979119/get-equal-part-of-multiple-strings-at-the-beginning
            var first = input[0];
            var rest = input.GetRange(1, input.Count - 1);
            return new string(first.TakeWhile((c, index) => rest.All(s => s[index] == c)).ToArray());
        }

        private int GetOptimalMaxThreads()
        {
            var cores = SystemInfo.GetCoreCount();
            if (cores == -1)
            {
                Console.WriteLine("NOTE: Above error about the CPU info can be ignored.");
            }
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
