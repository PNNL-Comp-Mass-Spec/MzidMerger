using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PRISM;
using PSI_Interface.CV;
using PSI_Interface.IdentData;
using PSI_Interface.IdentData.IdentDataObjs;
using PSI_Interface.IdentData.mzIdentML;

namespace MzidMerger
{
    public sealed class MzidMerging
    {
        public static void MergeMzids(Options options)
        {
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            if (options.FilesToMerge.Count < 2)
            {
                ConsoleMsgUtils.ShowWarning("Must supply two or more .mzID files; nothing to do");
                return;
            }

            var targetFile = options.FilesToMerge[0];
            IEnumerable<IdentDataObj> toMerge;
            if (!options.MultiThread)
            {
                toMerge = options.FilesToMerge.Skip(1).Select(x => ReadAndPreprocessFile(x, options));
            }
            else
            {
                toMerge = options.FilesToMerge.Skip(1).ParallelPreprocess(x => ReadAndPreprocessFile(x, options), 2);
            }

            Console.WriteLine("Reading first file (the merge target)...");
            var targetObj = ReadAndPreprocessFile(targetFile, options);

            var merger = new MzidMerging(targetObj);
            merger.MergeIdentData(toMerge, options, true);

            // Add the merging information
            var mergerSoftware = new AnalysisSoftwareObj
            {
                Id = "MzidMerger_Id",
                Name = "MzidMerger",
                Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                SoftwareName = new ParamObj { Item = new UserParamObj { Name = "MzidMerger" } },
            };

            targetObj.AnalysisSoftwareList.Add(mergerSoftware);

            targetObj.AnalysisProtocolCollection.SpectrumIdentificationProtocols[0].AdditionalSearchParams.Items.Add(new UserParamObj {
                Name = "Merger_KeepOnlyBestResults",
                Value = options.KeepOnlyBestResults.ToString() });

            if (options.MaxSpecEValue < 50)
            {
                targetObj.AnalysisProtocolCollection.SpectrumIdentificationProtocols[0].AdditionalSearchParams.Items.Add(new UserParamObj {
                    Name = "Merger_MaxSpecEValue",
                    Value = options.MaxSpecEValue.ToString(CultureInfo.InvariantCulture) });
            }

            var count = 1;

            foreach (var file in options.FilesToMerge)
            {
                var sourceFile = new SourceFileInfo
                {
                    Id = $"MergedMzid_{count++}",
                    Location = file,
                    Name = Path.GetFileName(file),
                    FileFormat = new FileFormatInfo { CVParam = new CVParamObj(CV.CVID.MS_mzIdentML_format) },
                };
                targetObj.DataCollection.Inputs.SourceFiles.Add(sourceFile);
            }

            Console.WriteLine("Writing merged file...");

            MzIdentMlReaderWriter.Write(new MzIdentMLType(targetObj), options.OutputFilePath);

            stopWatch.Stop();
            Console.WriteLine("Total time to merge {0} files: {1:g}", options.FilesToMerge.Count, stopWatch.Elapsed);
        }

        private static IdentDataObj ReadAndPreprocessFile(string filePath, Options options)
        {
            var identData = IdentDataReaderWriter.Read(filePath);

            if (options.FixIDs)
            {
                try
                {
                    //var pepDict = new Dictionary<string, PeptideObj>(); // TODO: Monitor for duplicates
                    var mods = identData.AnalysisProtocolCollection.SpectrumIdentificationProtocols[0].ModificationParams.Where(x => x.FixedMod);
                    var fixedModDict = new Dictionary<string, List<SearchModificationObj>>();

                    foreach (var mod in mods)
                    {
                        var massStr = mod.MassDelta.ToString("F4");

                        if (!fixedModDict.ContainsKey(massStr))
                        {
                            fixedModDict.Add(massStr, new List<SearchModificationObj>());
                        }

                        fixedModDict[massStr].Add(mod);
                    }

                    foreach (var peptide in identData.SequenceCollection.Peptides)
                    {
                        // Re-ID peptides...
                        var seq = peptide.PeptideSequence;
                        foreach (var mod in peptide.Modifications.OrderByDescending(x => x.Location).ThenByDescending(x => x.MonoisotopicMassDelta))
                        {
                            var isNTerm = false;
                            var isCTerm = false;
                            if (mod.Location == 0)
                            {
                                isNTerm = true;
                            }

                            if (mod.Location == 0 || mod.Location == peptide.PeptideSequence.Length + 1)
                            {
                                isCTerm = true;
                            }

                            var massStr = mod.MonoisotopicMassDelta.ToString("F4");

                            // match to mass and residue (backward lookup from location)
                            if (fixedModDict.TryGetValue(massStr, out var fixedMods))
                            {
                                string residue;

                                if (isNTerm || isCTerm)
                                {
                                    residue = ".";
                                }
                                else
                                {
                                    residue = peptide.PeptideSequence[mod.Location - 1].ToString();
                                }

                                foreach (var fixedMod in fixedMods)
                                {
                                    // TODO: should also check specificity rules
                                    if (fixedMod.Residues.Contains(residue))
                                    {
                                        continue;
                                    }
                                }
                            }

                            if (isNTerm)
                            {
                                seq = $"[{mod.MonoisotopicMassDelta:+F2}{seq}";
                            }
                            else if (isCTerm)
                            {
                                seq += $"}}{mod.MonoisotopicMassDelta:+F2}";
                            }
                            else
                            {
                                seq = $"{seq.Substring(0, mod.Location)}{mod.MonoisotopicMassDelta:+F2}{seq.Substring(mod.Location)}";
                            }
                        }

                        peptide.Id = "Pep_" + seq;
                    }
                    //pepDict.Clear(); // TODO:

                    // var pepEvDict = new Dictionary<string, PeptideEvidenceObj>(); // TODO: Monitor for duplicates
                    foreach (var pepEv in identData.SequenceCollection.PeptideEvidences)
                    {
                        // Re-Id PeptideEvidences
                        var dbSeq = pepEv.DBSequence.Id;
                        if (dbSeq.StartsWith("DBSeq", StringComparison.OrdinalIgnoreCase))
                        {
                            dbSeq = dbSeq.Substring(5);
                            if (int.TryParse(dbSeq, out var offset))
                            {
                                dbSeq = (offset + pepEv.Start - 1).ToString();
                            }
                        }

                        var pepId = pepEv.Peptide.Id.Substring(4);

                        pepEv.Id = $"PepEv_{dbSeq}_{pepId}_{pepEv.Start}";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return identData;
        }

        private void MergeIdentData(IEnumerable<IdentDataObj> toMerge, Options options, bool remapPostMerge)
        {
            var mergedCount = 2; // start at 2, since we are merging into the first file.

            foreach (var mergeObj in toMerge)
            {
                Console.Write("\rMerging file {0} / {1} ...                                 ", mergedCount, options.FilesToMerge.Count);
                MergeIdentData(mergeObj, options.MaxSpecEValue, options.KeepOnlyBestResults, remapPostMerge);
                mergedCount++;
            }

            Console.WriteLine();

            if (remapPostMerge)
            {
                Console.WriteLine("Repopulating the sequence collection and fixing IDs...");
                FilterAndRepopulateSequenceCollection(options.MaxSpecEValue, options.KeepOnlyBestResults);
            }

            Console.WriteLine("File merge complete.");

            // Final TODO: rewrite the IDs, to ensure uniqueness
        }

        /// <summary>
        /// merge 2 files
        /// </summary>
        /// <param name="toMerge"></param>
        /// <param name="maxSpecEValue"></param>
        /// <param name="keepOnlyBestResult">if true, only the best-specEValue result(s) will be kept for each spectrum</param>
        /// <param name="remapPostMerge">if true, the items under SequenceCollection are not merged (to be repopulated as a last step)</param>
        private void MergeIdentData(IdentDataObj toMerge, double maxSpecEValue, bool keepOnlyBestResult, bool remapPostMerge)
        {
            Merge(targetIdentDataObj.CVList, toMerge.CVList);
            Merge(targetIdentDataObj.AnalysisSoftwareList, toMerge.AnalysisSoftwareList);
            Merge(targetIdentDataObj.Provider, toMerge.Provider);
            Merge(targetIdentDataObj.AuditCollection, toMerge.AuditCollection);
            Merge(targetIdentDataObj.AnalysisSampleCollection, toMerge.AnalysisSampleCollection);
            Merge(targetIdentDataObj.SequenceCollection, toMerge.SequenceCollection, remapPostMerge);
            Merge(targetIdentDataObj.AnalysisCollection, toMerge.AnalysisCollection);
            Merge(targetIdentDataObj.AnalysisProtocolCollection, toMerge.AnalysisProtocolCollection);
            Merge(targetIdentDataObj.DataCollection, toMerge.DataCollection, maxSpecEValue, keepOnlyBestResult, remapPostMerge);
            Merge(targetIdentDataObj.BibliographicReferences, toMerge.BibliographicReferences);

            // Final TODO: rewrite the IDs, to ensure uniqueness
        }

        private MzidMerging(IdentDataObj target)
        {
            targetIdentDataObj = target;
        }

        private readonly IdentDataObj targetIdentDataObj;

        #region Divide-And-Conquer merge (uses high resources)

        public static void MergeMzidsDivideAndConquer(Options options)
        {
            if (options.FilesToMerge.Count < 2)
            {
                return;
            }

            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            // Semaphore: initialCount, is the number initially available, maximumCount is the max allowed
            var threadLimiter = new Semaphore(options.MaxThreads, options.MaxThreads);
            var mergedData = DivideAndConquerMergeIdentData(options.FilesToMerge, threadLimiter, options.MaxSpecEValue, options.KeepOnlyBestResults, true, options).targetIdentDataObj;

            stopWatch.Stop();
            Console.WriteLine("Mzid read time: {0:g}", mReadTime);
            Console.WriteLine("Mzid merge time: {0:g}", mMergeTime);
            Console.WriteLine("Mzid read and merge time: {0:g}", stopWatch.Elapsed);
            stopWatch.Restart();

            MzIdentMlReaderWriter.Write(new MzIdentMLType(mergedData), options.OutputFilePath);

            stopWatch.Stop();
            Console.WriteLine("Mzid write time: {0}", stopWatch.Elapsed);
        }

        private static TimeSpan mReadTime = TimeSpan.Zero;
        private static TimeSpan mMergeTime = TimeSpan.Zero;
        private static readonly object ReadTimeWriteLock = new();
        private static readonly object MergeTimeWriteLock = new();

        private MzidMerging(string filePath, Options options)
        {
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            targetIdentDataObj = ReadAndPreprocessFile(filePath, options);
            stopWatch.Stop();
            lock (ReadTimeWriteLock)
            {
                mReadTime += stopWatch.Elapsed;
            }
        }

        private static MzidMerging DivideAndConquerMergeIdentData(List<string> filePaths, Semaphore threadLimiter, double maxSpecEValue, bool keepOnlyBestResult, bool finalize, Options options)
        {
            if (filePaths.Count >= 2)
            {
                var mid = (filePaths.Count + 1) / 2; // keep the greater half in the first half
                var firstHalf = filePaths.GetRange(0, mid);
                var secondHalf = filePaths.GetRange(mid, filePaths.Count - mid);

                /**/
                // run in parallel
                var merged1Task = Task.Run(() => DivideAndConquerMergeIdentData(firstHalf, threadLimiter, maxSpecEValue, keepOnlyBestResult, false, options));
                var merged2Task = Task.Run(() => DivideAndConquerMergeIdentData(secondHalf, threadLimiter, maxSpecEValue, keepOnlyBestResult, false, options));

                // wait for them to complete
                Task.WaitAll(merged1Task, merged2Task);

                var merged1 = merged1Task.Result;
                var merged2 = merged2Task.Result;
                /*/
                var merged1 = DivideAndConquerMergeIdentData(firstHalf);
                var merged2 = DivideAndConquerMergeIdentData(secondHalf);
                /**/
                merged2.DropDictionaries();
                var merged2Data = merged2.targetIdentDataObj;

                var stopWatch = System.Diagnostics.Stopwatch.StartNew();

                // merge the results
                threadLimiter.WaitOne();
                merged1.MergeIdentData(merged2Data, maxSpecEValue, keepOnlyBestResult, true);
                threadLimiter.Release();
                stopWatch.Stop();

                Console.WriteLine("Time to merge {0}{1} files into {2}{3} files: {4:g}",
                                  secondHalf.Count,
                                  secondHalf.Count > 1 ? " merged" : string.Empty,
                                  firstHalf.Count,
                                  firstHalf.Count > 1 ? " merged" : string.Empty,
                                  stopWatch.Elapsed);

                lock (MergeTimeWriteLock)
                {
                    mMergeTime += stopWatch.Elapsed;
                }

                if (finalize)
                {
                    stopWatch.Restart();
                    // repopulate the DBSequence/Peptide/PeptideEvidence lists
                    merged1.FilterAndRepopulateSequenceCollection(maxSpecEValue, keepOnlyBestResult);

                    stopWatch.Stop();
                    Console.WriteLine("Time to repopulate sequence collection: {0}", stopWatch.Elapsed);
                }

                return merged1;
            }

            if (filePaths.Count == 1)
            {
                threadLimiter.WaitOne();
                var merger = new MzidMerging(filePaths[0], options);
                threadLimiter.Release();
                return merger;
            }

            // Shouldn't encounter this, unless we are doing bad math above, or had bad original input
            return null;
        }

        private void DropDictionaries()
        {
            if (peptideDictionary != null)
            {
                peptideDictionary.Clear();
                peptideDictionary = null;
            }

            if (spectraDataLookupByName != null)
            {
                spectraDataLookupByName.Clear();
                spectraDataLookupByName = null;
            }

            if (spectrumResultLookupByFilenameAndSpecId != null)
            {
                spectrumResultLookupByFilenameAndSpecId.Clear();
                spectrumResultLookupByFilenameAndSpecId = null;
            }
        }

        #endregion

        #region Final Processing: repopulate sequence collection and fix IDs

        private void FilterAndRepopulateSequenceCollection(double maxSpecEValue = 100.0, bool keepOnlyBestResult = false)
        {
            var dbSeqList = targetIdentDataObj.SequenceCollection.DBSequences;
            var pepList = targetIdentDataObj.SequenceCollection.Peptides;
            var pepEvList = targetIdentDataObj.SequenceCollection.PeptideEvidences;

            dbSeqList.Clear();
            pepList.Clear();
            pepEvList.Clear();

            var dbSeqLookup = new Dictionary<string, DbSequenceObj>();
            var pepLookup = new Dictionary<string, PeptideObj>();
            var pepEvLookup = new Dictionary<string, PeptideEvidenceObj>();

            //foreach (var identList in targetIdentDataObj.DataCollection.AnalysisData.SpectrumIdentificationList)
            //{

            var identList = targetIdentDataObj.DataCollection.AnalysisData.SpectrumIdentificationList[0];

            // Use .ToList() to create a distinct list, and allow modification of the original
            foreach (var specIdResult in identList.SpectrumIdentificationResults.ToList())
            {
                if (specIdResult.BestSpecEVal() > maxSpecEValue)
                {
                    identList.SpectrumIdentificationResults.Remove(specIdResult);
                    continue;
                }

                if (keepOnlyBestResult)
                {
                    // remove all but the highest scoring result(s) for each spectrum
                    specIdResult.RemoveMatchesNotBestSpecEValue();
                }

                // Re-rank the spectrumIdentificationItems in each spectrumIdentification result
                specIdResult.ReRankBySpecEValue();

                // Use .ToList() to create a distinct list, and allow modification of the original
                foreach (var specIdItem in specIdResult.SpectrumIdentificationItems.ToList())
                {
                    if (specIdItem.GetSpecEValue() > maxSpecEValue)
                    {
                        specIdResult.SpectrumIdentificationItems.Remove(specIdItem);
                    }
                }

                foreach (var specIdItem in specIdResult.SpectrumIdentificationItems)
                {
                    if (pepLookup.TryGetValue(specIdItem.Peptide.Id, out var pepMatch))
                    {
                        if (!pepMatch.Equals(specIdItem.Peptide))
                        {
                            Console.WriteLine("ERROR: duplicate peptide ID detected for non-duplicate peptide; skipping! ID: \"{0}\"", specIdItem.Peptide.Id);
                        }

                        specIdItem.Peptide = pepMatch;
                    }
                    else
                    {
                        //pepList.Add(specIdItem.Peptide); // TODO: do at end, for speed optimizations
                        pepLookup.Add(specIdItem.Peptide.Id, specIdItem.Peptide);
                    }

                    // TODO: need to re-id (and check for previous re-id) of the peptideEvidences and dbSequences before merging here...
                    foreach (var pepEv in specIdItem.PeptideEvidences)
                    {
                        ChangePepEvId(pepEv.PeptideEvidence);

                        if (pepEvLookup.TryGetValue(pepEv.PeptideEvidence.Id, out var pepEvMatch))
                        {
                            pepEv.PeptideEvidence = pepEvMatch;
                        }
                        else
                        {
                            //pepEvList.Add(pepEv.PeptideEvidence); // TODO: do at end, for speed optimizations
                            pepEvLookup.Add(pepEv.PeptideEvidence.Id, pepEv.PeptideEvidence);

                            // Verify a difference before checking the lookup
                            if (!pepEv.PeptideEvidence.Peptide.Equals(specIdItem.Peptide))
                            {
                                if (pepLookup.TryGetValue(pepEv.PeptideEvidence.Peptide.Id, out var pepMatch2))
                                {
                                    if (!pepMatch2.Equals(pepEv.PeptideEvidence.Peptide))
                                    {
                                        Console.WriteLine("ERROR: duplicate peptide ID detected for non-duplicate peptide; skipping! ID: \"{0}\"",
                                            pepEv.PeptideEvidence.Peptide.Id);
                                    }

                                    pepEv.PeptideEvidence.Peptide = pepMatch;
                                }
                                else
                                {
                                    //pepList.Add(specIdItem.Peptide); // TODO: do at end, for speed optimizations
                                    pepLookup.Add(pepEv.PeptideEvidence.Peptide.Id, pepEv.PeptideEvidence.Peptide);
                                }
                            }
                            else
                            {
                                pepEv.PeptideEvidence.Peptide = specIdItem.Peptide;
                            }

                            if (dbSeqLookup.TryGetValue(pepEv.PeptideEvidence.DBSequence.Id, out var dbSeqMatch))
                            {
                                if (!dbSeqMatch.Equals(pepEv.PeptideEvidence.DBSequence))
                                {
                                    Console.WriteLine("ERROR: duplicate DBSequence ID detected for non-duplicate Protein; skipping! ID: \"{0}\"",
                                        pepEv.PeptideEvidence.DBSequence.Id);
                                }

                                pepEv.PeptideEvidence.DBSequence = dbSeqMatch;
                            }
                            else
                            {
                                //pepList.Add(specIdItem.Peptide); // TODO: do at end, for speed optimizations
                                dbSeqLookup.Add(pepEv.PeptideEvidence.DBSequence.Id, pepEv.PeptideEvidence.DBSequence);
                            }
                        }
                    }
                }
            }

            // Re-sort the spectrumIdentificationResults in the spectrumIdentificationList, according to specEValue
            identList.Sort();
            //}

            pepEvList.AddRange(pepEvLookup.Values);
            pepList.AddRange(pepLookup.Values);
            dbSeqList.AddRange(dbSeqLookup.Values);
        }

        private static string ChangeDBSequenceId(DbSequenceObj dbSeq)
        {
            var sdbId = dbSeq.SearchDatabase.Id;

            if (sdbId.StartsWith("SearchDB_"))
            {
                sdbId = sdbId.Replace("SearchDB_", "sdb");
            }

            if (!dbSeq.Id.EndsWith(sdbId))
            {
                dbSeq.Id = $"{dbSeq.Id}_{sdbId}";
            }

            return sdbId;
        }

        private static void ChangePepEvId(PeptideEvidenceObj pepEv)
        {
            var sdbId = ChangeDBSequenceId(pepEv.DBSequence);

            if (!pepEv.Id.EndsWith(sdbId))
            {
                pepEv.Id = $"{pepEv.Id}_{sdbId}";
            }
        }

        #endregion

        #region Base-level elements

        private static void Merge(IdentDataList<CVInfo> target, IdentDataList<CVInfo> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            foreach (var item in toMerge)
            {
                if (target.Any(x => x.Equals(item) && x.Id.Equals(item.Id)))
                {
                    continue;
                }

                // if the item doesn't have an ID match, add it anyway?
                // TODO: not checking for duplicate IDs!!
                target.Add(item);
            }
        }

        private static void Merge(IdentDataList<AnalysisSoftwareObj> target, IdentDataList<AnalysisSoftwareObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            foreach (var item in toMerge)
            {
                if (target.Any(x => x.Equals(item) && x.Id.Equals(item.Id)))
                {
                    continue;
                }

                // TODO: Check for duplicate ID!!!
                target.Add(item);
            }
            // TODO: Details!!!
        }

        private static void Merge(ProviderObj target, ProviderObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Not used by MS-GF+
            // TODO: Details!!!
        }

        private static void Merge(IReadOnlyCollection<AbstractContactObj> target, IReadOnlyCollection<AbstractContactObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Not used by MS-GF+
            // TODO: Details!!!
        }

        private static void Merge(IReadOnlyCollection<SampleObj> target, IReadOnlyCollection<SampleObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Not used by MS-GF+
            // TODO: Details!!!
        }

        private void Merge(SequenceCollectionObj target, SequenceCollectionObj toMerge, bool remapPostMerge = false)
        {
            if (remapPostMerge)
            {
                return; // TODO: these will be re-added with the last merge, pulled out of the references from the spectrum identification items
            }

            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.DBSequences, toMerge.DBSequences);
            Merge(target.Peptides, toMerge.Peptides);
            Merge(target.PeptideEvidences, toMerge.PeptideEvidences, target.Peptides);
        }

        private static void Merge(AnalysisCollectionObj target, AnalysisCollectionObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.SpectrumIdentifications, toMerge.SpectrumIdentifications);
            Merge(target.ProteinDetection, toMerge.ProteinDetection);
        }

        private static void Merge(AnalysisProtocolCollectionObj target, AnalysisProtocolCollectionObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.SpectrumIdentificationProtocols, toMerge.SpectrumIdentificationProtocols);
            // Merge(target.ProteinDetectionProtocol, toMerge.ProteinDetectionProtocol); // TODO: not used by MS-GF+
        }

        private void Merge(DataCollectionObj target, DataCollectionObj toMerge, double maxSpecEValue, bool keepOnlyBestResult, bool remapPostMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.Inputs, toMerge.Inputs);
            Merge(target.AnalysisData, toMerge.AnalysisData, maxSpecEValue, keepOnlyBestResult, remapPostMerge);
        }

        private static void Merge(IReadOnlyCollection<BibliographicReferenceObj> target, IReadOnlyCollection<BibliographicReferenceObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Not used by MS-GF+
            // TODO: Details!!!
        }

        #endregion

        #region Sequence collection contents

        private static void Merge(IdentDataList<DbSequenceObj> target, IReadOnlyCollection<DbSequenceObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // We are focusing on merging results from split-fasta searches right now, so just include the DBSequences
            // IDs will need to be changed later to ensure distinction TODO
            target.AddRange(toMerge);
        }

        private Dictionary<string, PeptideObj> peptideDictionary;

        private void Merge(IdentDataList<PeptideObj> target, IReadOnlyCollection<PeptideObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            if (peptideDictionary == null || peptideDictionary.Count == 0)
            {
                peptideDictionary = new Dictionary<string, PeptideObj>((int)(target.Count * 1.5));
                // Should only need to do this once, because we maintain as we go.
                foreach (var item in target)
                {
                    if (!peptideDictionary.ContainsKey(item.Id))
                    {
                        peptideDictionary.Add(item.Id, item);
                    }
                    else
                    {
                        Console.WriteLine("ERROR creating peptide dictionary: duplicate peptide ID \"{0}\" encountered", item.Id);
                    }
                }
            }

            var totalSize = target.Count + toMerge.Count;
            if (target.Capacity < totalSize)
            {
                target.Capacity = totalSize;
            }

            // We can have duplicate peptides across different Fasta files; we do need to make sure the peptideEvidence references are appropriately updated
            foreach (var item in toMerge)
            {
                if (peptideDictionary.TryGetValue(item.Id, out var existing))
                {
                    if (existing.Equals(item))
                    {
                        // exact duplicate; no action needed, besides correcting the peptide references
                        continue;
                    }

                    // Duplicate ID, non-duplicate peptide: report it to the user!
                    Console.WriteLine("ERROR: duplicate peptide ID detected for non-duplicate peptide; skipping! ID: \"{0}\"", item.Id);
                    continue;
                }

                target.Add(item);
                peptideDictionary.Add(item.Id, item);
            }
        }

        private void Merge(IdentDataList<PeptideEvidenceObj> target, IdentDataList<PeptideEvidenceObj> toMerge, ICollection<PeptideObj> targetPeptides)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // We are focusing on merging results from split-fasta searches right now, so just include the DBSequences (because each one is tied to a distinct protein)
            // IDs will need to be changed later to ensure distinction TODO
            target.AddRange(toMerge);

            // correct any duplicate peptide references
            foreach (var item in toMerge)
            {
                if (targetPeptides.Contains(item.Peptide))
                {
                    continue;
                }

                //item.Peptide = targetPeptides.First(x => x.Id.Equals(item.Peptide.Id));
                item.Peptide = peptideDictionary[item.Peptide.Id]; // TODO: This is probably expensive...
            }
        }

        #endregion

        #region Analysis collection contents

        private static void Merge(IdentDataList<SpectrumIdentificationObj> target, IdentDataList<SpectrumIdentificationObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // Since we are combining files, merge those with matching protocols, but remember to change the IDs of those just added to guarantee uniqueness
            foreach (var item in toMerge)
            {
                var matches = target.Where(x => x.SpectrumIdentificationProtocol.Equals(item.SpectrumIdentificationProtocol)).ToList();
                if (matches.Count > 0)
                {
                    var match = matches[0];
                    foreach (var specFile in item.InputSpectra)
                    {
                        if (!match.InputSpectra.Any(x => x.SpectraData.Name.Equals(specFile.SpectraData.Name)))
                        {
                            match.InputSpectra.Add(specFile);
                        }
                    }

                    foreach (var searchDb in item.SearchDatabases)
                    {
                        if (!match.SearchDatabases.Any(x => x.SearchDatabase.Equals(searchDb.SearchDatabase))) // equality doesn't care about the file path, only the file name.
                        {
                            match.SearchDatabases.Add(searchDb);
                        }
                    }
                }
                else
                {
                    // Modify: if combining a split search, check the searched spectra file and keep only a single reference.
                    foreach (var specFile in item.InputSpectra.ToList())
                    {
                        if (target.Any(x => x.InputSpectra.Any(y => y.SpectraData.Name.Equals(specFile.SpectraData.Name))))
                        {
                            var targetObj = target.First(x => x.InputSpectra.Any(y => y.SpectraData.Name.Equals(specFile.SpectraData.Name)));
                            specFile.SpectraData = targetObj.InputSpectra.First(x => x.SpectraData.Name.Equals(specFile.SpectraData.Name)).SpectraData;
                        }
                    }

                    if (item.Id.StartsWith("SpecIdent_", StringComparison.OrdinalIgnoreCase))
                    {
                        var counter = 1;
                        var id = item.Id;
                        while (target.Any(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                        {
                            id = "SpecIdent_" + counter++;
                        }

                        item.Id = id;
                    }
                    else
                    {
                        var id = item.Id;
                        while (target.Any(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                        {
                            id += "x";
                        }

                        item.Id = id;
                    }

                    target.Add(item);

                    // NOTE: references SpectrumIdentificationList
                    // NOTE: references SpectrumIdentificationProtocol
                    // NOTE: references SearchDatabase
                }
            }
        }

        private static void Merge(ProteinDetectionObj target, ProteinDetectionObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Not used by MS-GF+
            //Merge(target.InputSpectrumIdentifications, toMerge.InputSpectrumIdentifications);
            // NOTE: references ProteinDetectionList
            // NOTE: references ProteinDetectionProtocol
        }

        private static void Merge(IReadOnlyCollection<InputSpectrumIdentificationsObj> target, List<InputSpectrumIdentificationsObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
            foreach (var inputSpecId in toMerge)
            {
                //inputSpecId.
                //if (target.Any(x => x.))
            }
        }

        private static void Merge(InputSpectrumIdentificationsObj target, InputSpectrumIdentificationsObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // NOTE: references SpectrumIdentificationList
        }

        #endregion

        #region Analysis protocol collection contents

        private static void Merge(IdentDataList<SpectrumIdentificationProtocolObj> target, IdentDataList<SpectrumIdentificationProtocolObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            foreach (var item in toMerge)
            {
                if (target.Any(x => x.Id.Equals(item.Id) && x.Equals(item)))
                {
                    // exact duplicate, just need to re-map references TODO
                    continue;
                }

                if (target.Any(x => x.Equals(item)))
                {
                    // exact duplicate, just need to change ids and then re-map references TODO

                    item.Id = target.First(x => x.Equals(item)).Id;
                    continue;
                }

                if (target.Any(x => x.Id.Equals(item.Id)))
                {
                    // not a duplicate, add it but change the ID since it is a duplicate
                    var id = item.Id + "x";

                    while (target.Any(x => x.Id.Equals(id)))
                    {
                        id += "x";
                    }

                    item.Id = id;
                }

                target.Add(item);
            }
        }

        private static void Merge(ProteinDetectionProtocolObj target, ProteinDetectionProtocolObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.AnalysisParams, toMerge.AnalysisParams);
            Merge(target.AnalysisSoftware, toMerge.AnalysisSoftware);
            // TODO: More details!!!
        }

        private static void Merge(AnalysisSoftwareObj target, AnalysisSoftwareObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.ContactRole, toMerge.ContactRole);

            // TODO: Details!!!
        }

        #endregion

        #region Data collection contents

        private static void Merge(InputsObj target, InputsObj toMerge, double maxSpecEValue = 100.0)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // Merge(target.SourceFiles, toMerge.SourceFiles); // TODO: Not used by MS-GF+ - but should probably be populated with information from this merge process
            Merge(target.SearchDatabases, toMerge.SearchDatabases);
            Merge(target.SpectraDataList, toMerge.SpectraDataList);
        }

        private static void Merge(IdentDataList<SearchDatabaseInfo> target, IdentDataList<SearchDatabaseInfo> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // Assuming we are combining a split search, we should just add the new ones
            // We will need to change the IDs for uniqueness
            foreach (var item in toMerge)
            {
                if (item.Id.StartsWith("SearchDB_", StringComparison.OrdinalIgnoreCase))
                {
                    var counter = 1;
                    var id = item.Id;
                    while (target.Any(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    {
                        id = "SearchDB_" + counter++;
                    }

                    item.Id = id;
                }
                else
                {
                    var id = item.Id;
                    while (target.Any(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    {
                        id += "x";
                    }

                    item.Id = id;
                }

                target.Add(item);
            }
        }

        private static void Merge(IdentDataList<SpectraDataObj> target, IdentDataList<SpectraDataObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            foreach (var item in toMerge)
            {
                if (target.Any(x => x.Name.Equals(item.Name)))
                {
                    // Name is the same, assume they are the same file; need to update references
                    continue;
                }

                target.Add(item);
            }
        }

        private void Merge(AnalysisDataObj target, AnalysisDataObj toMerge, double maxSpecEValue, bool keepOnlyBestResult, bool remapPostMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            //Merge(target.ProteinDetectionList, toMerge.ProteinDetectionList); // TODO: Not used by MS-GF+
            Merge(target.SpectrumIdentificationList, toMerge.SpectrumIdentificationList, maxSpecEValue, keepOnlyBestResult, remapPostMerge);
        }

        private void Merge(IReadOnlyCollection<SpectrumIdentificationListObj> target, IdentDataList<SpectrumIdentificationListObj> toMerge, double maxSpecEValue, bool keepOnlyBestResult, bool remapPostMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // Technically (for keeping search database references straight) we shouldn't combine the individual SpectrumIdentificationLists, but those references are kept alive via the DBSequence
            var targetSpecList = target.First();
            foreach (var item in toMerge)
            {
                Merge(targetSpecList, item, maxSpecEValue, keepOnlyBestResult, remapPostMerge);
            }
        }

        private void Merge(SpectrumIdentificationListObj target, SpectrumIdentificationListObj toMerge, double maxSpecEValue, bool keepOnlyBestResult, bool remapPostMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.FragmentationTables, toMerge.FragmentationTables);
            Merge(target.SpectrumIdentificationResults, toMerge.SpectrumIdentificationResults, maxSpecEValue, keepOnlyBestResult, remapPostMerge);
            // Merge(target.CVParams, toMerge.CVParams);        // TODO: Not used by MS-GF+
            // Merge(target.UserParams, toMerge.UserParams);    // TODO: Not used by MS-GF+
            toMerge.Id = target.Id;
            toMerge.Name = target.Name;
        }

        private static void Merge(IdentDataList<MeasureObj> target, IdentDataList<MeasureObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            foreach (var item in toMerge)
            {
                if (target.Any(x => x.Id.Equals(item.Id) && x.Equals(item)))
                {
                    continue;
                }

                if (target.Any(x => x.Equals(item)))
                {
                    item.Id = target.First(x => x.Equals(item)).Id;
                    continue;
                }

                target.Add(item);
            }
        }

        private Dictionary<string, SpectraDataObj> spectraDataLookupByName;
        private Dictionary<string, SpectrumIdentificationResultObj> spectrumResultLookupByFilenameAndSpecId;

        private static string CreateSpectrumResultLookupName(string spectraDataName, string spectrumId)
        {
            return $"{spectraDataName}_{spectrumId}";
        }

        private void Merge(
            IdentDataList<SpectrumIdentificationResultObj> target,
            IReadOnlyCollection<SpectrumIdentificationResultObj> toMerge,
            double maxSpecEValue,
            bool keepOnlyBestResult,
            bool cleanupPostMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            if (spectraDataLookupByName == null || spectrumResultLookupByFilenameAndSpecId == null || spectrumResultLookupByFilenameAndSpecId.Count == 0)
            {
                spectraDataLookupByName = new Dictionary<string, SpectraDataObj>();
                spectrumResultLookupByFilenameAndSpecId = new Dictionary<string, SpectrumIdentificationResultObj>((int)(target.Count * 1.5));
                target.RemoveAll(x => x.BestSpecEVal() > maxSpecEValue);
                // Only do this once, they are otherwise maintained as we go
                foreach (var result in target)
                {
                    var lookupName = CreateSpectrumResultLookupName(result.SpectraData.Name, result.SpectrumID);
                    if (!spectrumResultLookupByFilenameAndSpecId.ContainsKey(lookupName))
                    {
                        spectrumResultLookupByFilenameAndSpecId.Add(lookupName, result);
                    }
                    else
                    {
                        Console.WriteLine("ERROR creating spectrum result lookup: duplicate filename/SpectrumID! \"{0}\"", lookupName);
                    }

                    if (!spectraDataLookupByName.ContainsKey(result.SpectraData.Name))
                    {
                        spectraDataLookupByName.Add(result.SpectraData.Name, result.SpectraData);
                    }
                }
            }

            var totalSize = target.Count + toMerge.Count;
            if (target.Capacity < totalSize)
            {
                target.Capacity = totalSize;
            }

            foreach (var item in toMerge)
            {
                var lookupName = CreateSpectrumResultLookupName(item.SpectraData.Name, item.SpectrumID);

                if (spectrumResultLookupByFilenameAndSpecId.TryGetValue(lookupName, out var existing))
                {
                    Merge(existing, item, maxSpecEValue, keepOnlyBestResult, cleanupPostMerge);
                    continue;
                }

                if (item.BestSpecEVal() > maxSpecEValue)
                {
                    continue;
                }

                if (spectraDataLookupByName.TryGetValue(item.SpectraData.Name, out var existingFileRef))
                {
                    item.SpectraData = existingFileRef;
                }
                else
                {
                    spectraDataLookupByName.Add(item.SpectraData.Name, item.SpectraData);
                }

                target.Add(item);
                spectrumResultLookupByFilenameAndSpecId.Add(lookupName, item);
            }
        }

        private static void Merge(SpectrumIdentificationResultObj target, SpectrumIdentificationResultObj toMerge, double maxSpecEValue, bool keepOnlyBestResult, bool cleanupPostMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            var toMergeItems = toMerge.SpectrumIdentificationItems.Where(x => !(x.GetSpecEValue() > maxSpecEValue));
            if (keepOnlyBestResult)
            {
                var bestSpecEValue = target.BestSpecEVal();
                toMergeItems = toMergeItems.Where(x => x.GetSpecEValue() <= bestSpecEValue);
            }

            var prevCount = target.SpectrumIdentificationItems.Count;
            // Add all spectrum identification items
            target.SpectrumIdentificationItems.AddRange(toMergeItems);

            if (keepOnlyBestResult && prevCount < target.SpectrumIdentificationItems.Count)
            {
                target.RemoveMatchesNotBestSpecEValue();
            }

            if (!cleanupPostMerge)
            {
                // sort by score, update rank and id
                target.ReRankBySpecEValue();
            }
        }

        #endregion

        #region Unimplemented...

        private static void Merge(ProteinDetectionListObj target, ProteinDetectionListObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private static void Merge(ContactRoleObj target, ContactRoleObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.Contact, toMerge.Contact);
            Merge(target.Role, toMerge.Role);
        }

        private static void Merge(AbstractContactObj target, AbstractContactObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private static void Merge(RoleObj target, RoleObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private static void Merge(ParamListObj target, ParamListObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private static void Merge(MeasureObj target, MeasureObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private static void Merge(List<CVParamObj> target, List<CVParamObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private static void Merge(CVParamObj target, CVParamObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private static void Merge(IdentDataList<UserParamObj> target, IdentDataList<UserParamObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private static void Merge(UserParamObj target, UserParamObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        /*
        private static void Merge(Obj target, Obj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }
        */

        #endregion
    }
}
