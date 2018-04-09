using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PSI_Interface.IdentData;
using PSI_Interface.IdentData.IdentDataObjs;
using PSI_Interface.IdentData.mzIdentML;

namespace MzidMerger
{
    public class MzidMerging
    {
        public static void MergeMzids(List<string> inputPaths, string outputPath, double maxSpecEValue = 100.0)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var targetFile = inputPaths.First();
            var toMerge = inputPaths.Skip(1).ParallelPreprocess(x => new IdentDataObj(MzIdentMlReaderWriter.Read(x)), 1);
            var targetObj = new IdentDataObj(MzIdentMlReaderWriter.Read(targetFile));

            sw.Stop();
            Console.WriteLine("Mzid Read time: {0}", sw.Elapsed);
            sw.Restart();

            var merger = new MzidMerging(targetObj);
            merger.MergeIdentData(toMerge, maxSpecEValue, true);

            sw.Stop();
            Console.WriteLine("Mzid merge time: {0}", sw.Elapsed);
            sw.Restart();

            MzIdentMlReaderWriter.Write(new MzIdentMLType(targetObj), outputPath);

            sw.Stop();
            Console.WriteLine("Mzid write time: {0}", sw.Elapsed);
        }

        public static void MergeMzidsOld(List<string> inputPaths, string outputPath, double maxSpecEValue = 100.0)
        {
            var identObjs = new List<IdentDataObj>();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var objListLock = new object();
            Parallel.ForEach(inputPaths, inputPath =>
            {
                var data = new IdentDataObj(MzIdentMlReaderWriter.Read(inputPath));
                lock (objListLock)
                {
                    identObjs.Add(data);
                }

            });
            //foreach (var inputPath in inputPaths)
            //{
            //    identObjs.Add(new IdentDataObj(MzIdentMlReaderWriter.Read(inputPath)));
            //}

            sw.Stop();
            Console.WriteLine("Mzid Read time: {0}", sw.Elapsed);
            sw.Restart();

            var targetObj = identObjs.First();
            identObjs.Remove(targetObj);

            var merger = new MzidMerging(targetObj);
            merger.MergeIdentData(identObjs, maxSpecEValue, true);

            sw.Stop();
            Console.WriteLine("Mzid merge time: {0}", sw.Elapsed);
            sw.Restart();

            MzIdentMlReaderWriter.Write(new MzIdentMLType(targetObj), outputPath);

            sw.Stop();
            Console.WriteLine("Mzid write time: {0}", sw.Elapsed);
        }

        public static void MergeMzidsDivideAndConquer(List<string> inputPaths, string outputPath, double maxSpecEValue = 100.0, int maxThreads = 200)
        {
            if (inputPaths.Count < 2)
            {
                return;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Semaphore: initialCount, is the number initially available, maximumCount is the max allowed
            var threadLimiter = new Semaphore(maxThreads, maxThreads);
            var mergedData = DivideAndConquerMergeIdentData(inputPaths, threadLimiter, maxSpecEValue, true).targetIdentDataObj;

            sw.Stop();
            Console.WriteLine("Mzid read time: {0}", readTime);
            Console.WriteLine("Mzid convert/map time: {0}", readConvertTime);
            Console.WriteLine("Mzid merge time: {0}", mergeTime);
            Console.WriteLine("Mzid read and merge time: {0}", sw.Elapsed);
            sw.Restart();

            MzIdentMlReaderWriter.Write(new MzIdentMLType(mergedData), outputPath);

            sw.Stop();
            Console.WriteLine("Mzid write time: {0}", sw.Elapsed);
        }

        private MzidMerging(IdentDataObj target)
        {
            targetIdentDataObj = target;
        }

        private static TimeSpan readTime = TimeSpan.Zero;
        private static TimeSpan readConvertTime = TimeSpan.Zero;
        private static TimeSpan mergeTime = TimeSpan.Zero;
        private static readonly object ReadTimeWriteLock = new object();
        private static readonly object MergeTimeWriteLock = new object();

        private MzidMerging(string filePath)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var mzid = MzIdentMlReaderWriter.Read(filePath);
            sw.Stop();
            var myReadTime = sw.Elapsed;
            sw.Restart();
            targetIdentDataObj = new IdentDataObj(mzid);
            sw.Stop();
            lock (ReadTimeWriteLock)
            {
                readTime += myReadTime;
                readConvertTime += sw.Elapsed;
            }
        }

        private readonly IdentDataObj targetIdentDataObj;

        private static MzidMerging DivideAndConquerMergeIdentData(List<string> filePaths, Semaphore threadLimiter, double maxSpecEValue = 100.0, bool finalize = false)
        {
            if (filePaths.Count >= 2)
            {

                var mid = (filePaths.Count + 1) / 2; // keep the greater half in the first half
                var firstHalf = filePaths.GetRange(0, mid);
                var secondHalf = filePaths.GetRange(mid, filePaths.Count - mid);

                /**/
                // run in parallel
                var merged1Task = Task.Run(() => DivideAndConquerMergeIdentData(firstHalf, threadLimiter, maxSpecEValue, false));
                var merged2Task = Task.Run(() => DivideAndConquerMergeIdentData(secondHalf, threadLimiter, maxSpecEValue, false));

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

                var sw = System.Diagnostics.Stopwatch.StartNew();
                // merge the results

                threadLimiter.WaitOne();
                merged1.MergeIdentData(merged2Data, maxSpecEValue, true);
                threadLimiter.Release();
                sw.Stop();

                Console.WriteLine("Time to merge {0}{1} files into {2}{3} files: {4}", secondHalf.Count, secondHalf.Count > 1 ? " merged" : "", firstHalf.Count, firstHalf.Count > 1 ? " merged" : "", sw.Elapsed);
                lock (MergeTimeWriteLock)
                {
                    mergeTime += sw.Elapsed;
                }

                if (finalize)
                {
                    sw.Restart();
                    // repopulate the DBSequence/Peptide/PeptideEvidence lists
                    merged1.FilterAndRepopulateSequenceCollection(maxSpecEValue);

                    //// fix ids... DONE by the above function...
                    //foreach (var dbseq in merged1.targetIdentDataObj.SequenceCollection.DBSequences)
                    //{
                    //    var sdbId = dbseq.SearchDatabase.Id;
                    //    if (sdbId.StartsWith("SearchDB_"))
                    //    {
                    //        sdbId = sdbId.Replace("SearchDB_", "sdb");
                    //    }
                    //
                    //    dbseq.Id = $"{dbseq.Id}_{sdbId}";
                    //}

                    sw.Stop();
                    Console.WriteLine("Time to repopulate sequence collection: {0}", sw.Elapsed);
                }

                return merged1;
            }

            if (filePaths.Count == 1)
            {
                threadLimiter.WaitOne();
                var merger = new MzidMerging(filePaths[0]);
                threadLimiter.Release();
                return merger;
            }

            // Shouldn't encounter this, unless we are doing bad math above, or had bad original input
            return null;
        }

        private void MergeIdentData(IEnumerable<IdentDataObj> toMerge, double maxSpecEValue = 100.0, bool remapPostMerge = false)
        {
            var sw = new System.Diagnostics.Stopwatch();
            var mergedCount = 1;
            foreach (var mergeObj in toMerge)
            {
                sw.Restart();
                MergeIdentData(mergeObj, maxSpecEValue, remapPostMerge);
                sw.Stop();
                Console.WriteLine("Time to merge 1 file into {0}{1} files: {2}", mergedCount, mergedCount > 1 ? " merged" : "", sw.Elapsed);
            }

            if (remapPostMerge)
            {
                sw.Restart();
                FilterAndRepopulateSequenceCollection(maxSpecEValue);
                sw.Stop();
                Console.WriteLine("Time to repopulate sequence collection: {0}", sw.Elapsed);
            }

            // Final TODO: rewrite all of the IDs, to ensure uniqueness
        }

        /// <summary>
        /// merge 2 files
        /// </summary>
        /// <param name="toMerge"></param>
        /// <param name="maxSpecEValue"></param>
        /// <param name="remapPostMerge">if true, the items under SequenceCollection are not merged (to be repopulated as a last step)</param>
        private void MergeIdentData(IdentDataObj toMerge, double maxSpecEValue = 100.0, bool remapPostMerge = false)
        {
            Merge(targetIdentDataObj.CVList, toMerge.CVList);
            Merge(targetIdentDataObj.AnalysisSoftwareList, toMerge.AnalysisSoftwareList);
            Merge(targetIdentDataObj.Provider, toMerge.Provider);
            Merge(targetIdentDataObj.AuditCollection, toMerge.AuditCollection);
            Merge(targetIdentDataObj.AnalysisSampleCollection, toMerge.AnalysisSampleCollection);
            Merge(targetIdentDataObj.SequenceCollection, toMerge.SequenceCollection, remapPostMerge);
            Merge(targetIdentDataObj.AnalysisCollection, toMerge.AnalysisCollection);
            Merge(targetIdentDataObj.AnalysisProtocolCollection, toMerge.AnalysisProtocolCollection);
            Merge(targetIdentDataObj.DataCollection, toMerge.DataCollection, maxSpecEValue, remapPostMerge);
            Merge(targetIdentDataObj.BibliographicReferences, toMerge.BibliographicReferences);

            // Final TODO: rewrite all of the IDs, to ensure uniqueness
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

        private void FilterAndRepopulateSequenceCollection(double maxSpecEValue = 100.0)
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
            var identList = targetIdentDataObj.DataCollection.AnalysisData.SpectrumIdentificationList.First();

            // re-rank all of the spectrumIdentificationItems in each spectrumIdentification result
            foreach (var specIdResult in identList.SpectrumIdentificationResults)
            {
                specIdResult.ReRankBySpecEValue();
            }

            // Re-sort the spectrumIdentificationResults in the spectrumIdentificationList, according to specEValue
            identList.Sort();

            // ToList() to create a distinct list, and allow modification of the original
            foreach (var specIdResult in identList.SpectrumIdentificationResults.ToList())
            {
                if (specIdResult.BestSpecEVal() > maxSpecEValue)
                {
                    identList.SpectrumIdentificationResults.Remove(specIdResult);
                    continue;
                }

                // ToList() to create a distinct list, and allow modification of the original
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
            //}

            pepEvList.AddRange(pepEvLookup.Values);
            pepList.AddRange(pepLookup.Values);
            dbSeqList.AddRange(dbSeqLookup.Values);
        }

        private string ChangeDBSequenceId(DbSequenceObj dbSeq)
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

        private void ChangePepEvId(PeptideEvidenceObj pepEv)
        {
            var sdbId = ChangeDBSequenceId(pepEv.DBSequence);

            if (!pepEv.Id.EndsWith(sdbId))
            {
                pepEv.Id = $"{pepEv.Id}_{sdbId}";
            }
        }

        #region Base-level elements

        private void Merge(IdentDataList<CVInfo> target, IdentDataList<CVInfo> toMerge)
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

        private void Merge(IdentDataList<AnalysisSoftwareObj> target, IdentDataList<AnalysisSoftwareObj> toMerge)
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

        private void Merge(ProviderObj target, ProviderObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Not used by MS-GF+
            // TODO: Details!!!
        }

        private void Merge(IdentDataList<AbstractContactObj> target, IdentDataList<AbstractContactObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Not used by MS-GF+
            // TODO: Details!!!
        }

        private void Merge(IdentDataList<SampleObj> target, IdentDataList<SampleObj> toMerge)
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

        private void Merge(AnalysisCollectionObj target, AnalysisCollectionObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.SpectrumIdentifications, toMerge.SpectrumIdentifications);
            Merge(target.ProteinDetection, toMerge.ProteinDetection);
        }

        private void Merge(AnalysisProtocolCollectionObj target, AnalysisProtocolCollectionObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.SpectrumIdentificationProtocols, toMerge.SpectrumIdentificationProtocols);
            // Merge(target.ProteinDetectionProtocol, toMerge.ProteinDetectionProtocol); // TODO: not used by MS-GF+
        }

        private void Merge(DataCollectionObj target, DataCollectionObj toMerge, double maxSpecEValue = 100.0, bool remapPostMerge = false)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.Inputs, toMerge.Inputs);
            Merge(target.AnalysisData, toMerge.AnalysisData, maxSpecEValue, remapPostMerge);
        }

        private void Merge(IdentDataList<BibliographicReferenceObj> target, IdentDataList<BibliographicReferenceObj> toMerge)
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

        private void Merge(IdentDataList<DbSequenceObj> target, IdentDataList<DbSequenceObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // We are focusing on merging results from split-fasta searches right now, so just include all of the DBSequences
            // IDs will need to be changed later to ensure distinction TODO
            target.AddRange(toMerge);
        }

        private Dictionary<string, PeptideObj> peptideDictionary = null;

        private void Merge(IdentDataList<PeptideObj> target, IdentDataList<PeptideObj> toMerge)
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

            // We can have duplicate peptides across different fasta files; we do need to make sure the peptideEvidence references are appropriately updated
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

        private void Merge(IdentDataList<PeptideEvidenceObj> target, IdentDataList<PeptideEvidenceObj> toMerge, IdentDataList<PeptideObj> targetPeptides)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // We are focusing on merging results from split-fasta searches right now, so just include all of the DBSequences (because each one is tied to a distinct protein)
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

        private void Merge(IdentDataList<SpectrumIdentificationObj> target, IdentDataList<SpectrumIdentificationObj> toMerge)
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

                    if (item.Id.ToLower().StartsWith("specident_"))
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

        private void Merge(ProteinDetectionObj target, ProteinDetectionObj toMerge)
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

        private void Merge(List<InputSpectrumIdentificationsObj> target, List<InputSpectrumIdentificationsObj> toMerge)
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

        private void Merge(InputSpectrumIdentificationsObj target, InputSpectrumIdentificationsObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // NOTE: references SpectrumIdentificationList
        }

        #endregion

        #region Analysis protocol collection contents

        private void Merge(IdentDataList<SpectrumIdentificationProtocolObj> target, IdentDataList<SpectrumIdentificationProtocolObj> toMerge)
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
                    var newId = target.First(x => x.Equals(item)).Id;
                    item.Id = newId;
                    continue;
                }

                if (target.Any(x => x.Id.Equals(item.Id)))
                {
                    // not a duplicate, add it but change the ID since it is a duplicate
                    var id = item.Id + "x";
                    while (target.Any(x => x.Id.Equals(item.Id)))
                    {
                        id += "x";
                    }

                    item.Id = id;
                }

                target.Add(item);
            }
        }

        private void Merge(ProteinDetectionProtocolObj target, ProteinDetectionProtocolObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.AnalysisParams, toMerge.AnalysisParams);
            Merge(target.AnalysisSoftware, toMerge.AnalysisSoftware);
            // TODO: More details!!!
        }

        private void Merge(AnalysisSoftwareObj target, AnalysisSoftwareObj toMerge)
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

        private void Merge(InputsObj target, InputsObj toMerge, double maxSpecEValue = 100.0)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // Merge(target.SourceFiles, toMerge.SourceFiles); // TODO: Not used by MS-GF+ - but should probably be populated with information from this merge process
            Merge(target.SearchDatabases, toMerge.SearchDatabases);
            Merge(target.SpectraDataList, toMerge.SpectraDataList);
        }

        private void Merge(IdentDataList<SearchDatabaseInfo> target, IdentDataList<SearchDatabaseInfo> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // Assuming we are combining a split search, we should just add all of the new ones
            // We will need to change the IDs for uniqueness
            foreach (var item in toMerge)
            {
                if (item.Id.ToLower().StartsWith("searchdb_"))
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

        private void Merge(IdentDataList<SpectraDataObj> target, IdentDataList<SpectraDataObj> toMerge)
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

        private void Merge(AnalysisDataObj target, AnalysisDataObj toMerge, double maxSpecEValue = 100.0, bool remapPostMerge = false)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            //Merge(target.ProteinDetectionList, toMerge.ProteinDetectionList); // TODO: Not used by MS-GF+
            Merge(target.SpectrumIdentificationList, toMerge.SpectrumIdentificationList, maxSpecEValue, remapPostMerge);
        }

        private void Merge(IdentDataList<SpectrumIdentificationListObj> target, IdentDataList<SpectrumIdentificationListObj> toMerge, double maxSpecEValue = 100.0, bool remapPostMerge = false)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // Technically (for keeping search database references straight) we shouldn't combine the individual SpectrumIdentificationLists, but those references are kept alive via the DBSequence
            var targetSpecList = target.First();
            foreach (var item in toMerge)
            {
                Merge(targetSpecList, item, maxSpecEValue, remapPostMerge);
            }
        }

        private void Merge(SpectrumIdentificationListObj target, SpectrumIdentificationListObj toMerge, double maxSpecEValue = 100.0, bool remapPostMerge = false)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.FragmentationTables, toMerge.FragmentationTables);
            Merge(target.SpectrumIdentificationResults, toMerge.SpectrumIdentificationResults, maxSpecEValue, remapPostMerge);
            // Merge(target.CVParams, toMerge.CVParams);        // TODO: Not used by MS-GF+
            // Merge(target.UserParams, toMerge.UserParams);    // TODO: Not used by MS-GF+
            toMerge.Id = target.Id;
            toMerge.Name = target.Name;
        }

        private void Merge(IdentDataList<MeasureObj> target, IdentDataList<MeasureObj> toMerge)
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

        private Dictionary<string, SpectraDataObj> spectraDataLookupByName = null;
        private Dictionary<string, SpectrumIdentificationResultObj> spectrumResultLookupByFilenameAndSpecId = null;

        private string CreateSpectrumResultLookupName(string spectraDataName, string spectrumId)
        {
            return $"{spectraDataName}_{spectrumId}";
        }

        private void Merge(IdentDataList<SpectrumIdentificationResultObj> target, IdentDataList<SpectrumIdentificationResultObj> toMerge, double maxSpecEValue = 100.0, bool cleanupPostMerge = false)
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
                        Console.WriteLine("ERROR creating spectrumresult lookup: duplicate filename/spectrumid! \"{0}\"", lookupName);
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
                    Merge(existing, item, maxSpecEValue, cleanupPostMerge);
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

        private void Merge(SpectrumIdentificationResultObj target, SpectrumIdentificationResultObj toMerge, double maxSpecEValue = 100.0, bool cleanupPostMerge = false)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // Add all spectrum identification items
            target.SpectrumIdentificationItems.AddRange(toMerge.SpectrumIdentificationItems.Where(x => !(x.GetSpecEValue() > maxSpecEValue)));

            if (!cleanupPostMerge)
            {
                // sort by score, update rank and id
                target.ReRankBySpecEValue();
            }
        }

        #endregion

        private void Merge(ProteinDetectionListObj target, ProteinDetectionListObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private void Merge(ContactRoleObj target, ContactRoleObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            Merge(target.Contact, toMerge.Contact);
            Merge(target.Role, toMerge.Role);
        }

        private void Merge(AbstractContactObj target, AbstractContactObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private void Merge(RoleObj target, RoleObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private void Merge(ParamListObj target, ParamListObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private void Merge(MeasureObj target, MeasureObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private void Merge(List<CVParamObj> target, List<CVParamObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private void Merge(CVParamObj target, CVParamObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private void Merge(IdentDataList<UserParamObj> target, IdentDataList<UserParamObj> toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        private void Merge(UserParamObj target, UserParamObj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }

        /*
        private void Merge(Obj target, Obj toMerge)
        {
            if (target == null || toMerge == null)
            {
                return;
            }

            // TODO: Details!!!
        }
        */
    }
}
