using NUnit.Framework;
using System;
using System.Collections.Generic;
using MzidMerger;

namespace MzidMergeTests
{
    [TestFixture]
    public class TestMergeMzid
    {
        [Test]
        public void MergeRecentMzids()
        {
            var inputs = new List<string>
            {
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part1.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part2.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part3.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part4.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part5.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part6.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part7.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part8.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part9.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part10.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part11.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part12.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part13.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part14.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part15.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part16.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part17.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part18.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part19.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part20.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part21.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part22.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part23.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part24.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part25.mzid.gz",
            };

            var output = @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus.mzid.gz";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var options = new Options()
            {
                OutputFilePath = output,
                MaxSpecEValue = 100,
            };
            options.FilesToMerge.AddRange(inputs);
            MzidMerging.MergeMzids(options);
            sw.Stop();
            Console.WriteLine("Total processing time: {0}", sw.Elapsed);
            /*
             * Mzid Read time: 00:58:57.4200490
             * Mzid merge time: 04:28:21.6242130
             * Mzid write time: 00:01:39.8106994
             * Total processing time: 05:28:58.8590444
             */
        }

        [Test]
        public void MergeRecentMzidsDivideConquer()
        {
            var inputs = new List<string>
            {
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part1.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part2.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part3.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part4.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part5.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part6.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part7.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part8.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part9.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part10.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part11.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part12.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part13.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part14.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part15.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part16.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part17.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part18.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part19.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part20.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part21.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part22.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part23.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part24.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part25.mzid.gz",
            };

            var output = @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_dc.mzid.gz";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var options = new Options()
            {
                OutputFilePath = output,
                MaxSpecEValue = 100,
                MaxThreads = 6,
            };
            options.FilesToMerge.AddRange(inputs);
            MzidMerging.MergeMzidsDivideAndConquer(options);
            sw.Stop();
            Console.WriteLine("Total processing time: {0}", sw.Elapsed);
        }
        [Test]
        public void MergeRecentMzidsFilter()
        {
            var inputs = new List<string>
            {
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part1.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part2.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part3.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part4.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part5.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part6.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part7.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part8.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part9.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part10.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part11.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part12.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part13.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part14.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part15.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part16.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part17.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part18.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part19.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part20.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part21.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part22.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part23.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part24.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part25.mzid.gz",
            };

            var output = @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_filter.mzid.gz";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var options = new Options()
            {
                OutputFilePath = output,
                MaxSpecEValue = 1e-10,
            };
            options.FilesToMerge.AddRange(inputs);
            MzidMerging.MergeMzids(options);
            sw.Stop();
            Console.WriteLine("Total processing time: {0}", sw.Elapsed);
        }

        [Test]
        public void MergeRecentMzidsDivideConquerFilter()
        {
            var inputs = new List<string>
            {
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part1.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part2.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part3.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part4.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part5.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part6.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part7.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part8.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part9.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part10.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part11.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part12.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part13.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part14.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part15.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part16.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part17.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part18.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part19.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part20.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part21.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part22.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part23.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part24.mzid.gz",
                @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_Part25.mzid.gz",
            };

            var output = @"E:\Mzid_merge\New_test\Buckley_12Ccell_Ag_09_14_QE_RR_29Sep17_Pippin_17-07-05_msgfplus_filter_dc.mzid.gz";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var options = new Options()
            {
                OutputFilePath = output,
                MaxSpecEValue = 100,
                MaxThreads = 6,
            };
            options.FilesToMerge.AddRange(inputs);
            MzidMerging.MergeMzidsDivideAndConquer(options);
            sw.Stop();
            Console.WriteLine("Total processing time: {0}", sw.Elapsed);
        }
    }
}
