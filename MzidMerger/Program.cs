using System.Reflection;
using PRISM;

namespace MzidMerger
{
    public class Program
    {
        // Ignore Spelling: mzid

        public static int Main(string[] args)
        {
            var options = new Options();
            var asmName = typeof(Program).GetTypeInfo().Assembly.GetName();
            var programVersion = typeof(Program).GetTypeInfo().Assembly.GetName().Version;
            var version = $"version {programVersion.Major}.{programVersion.Minor}.{programVersion.Build}";
            if (!CommandLineParser<Options>.ParseArgs(args, options, asmName.Name, version) || !options.Validate())
            {
                System.Threading.Thread.Sleep(1500);
                return -1;
            }

            options.ReportSettings();

            if (options.AllowHighResourceUsage)
            {
                MzidMerging.MergeMzidsDivideAndConquer(options);
            }
            else
            {
                MzidMerging.MergeMzids(options);
            }

            return 0;
        }
    }
}
