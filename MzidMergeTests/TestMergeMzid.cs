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

        [Test]
        public void TestNameIdenticals()
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

            Console.WriteLine("Start: \"{0}\"", Options.GetLeftIdentical(inputs));
            Console.WriteLine("Start2: \"{0}\"", Options.GetOutputNameStart(inputs));
            Console.WriteLine("End: \"{0}\"", Options.GetRightIdentical(inputs));
            Console.WriteLine("End2: \"{0}\"", Options.GetOutputNameEnd(inputs));
        }

        [Test]
        public void TestOptions()
        {
            var options = new Options();
            options.InputDirectory = @"E:\Mzid_merge\New_test";
            options.OutputFilePath = "blah";
            options.MaxSpecEValue = 1e-10;

            if (options.Validate())
            {
                options.ReportSettings();
                return;
            }

            Console.WriteLine("Validation Failed!!!");
        }
    }
}
