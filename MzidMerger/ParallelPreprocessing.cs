using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MzidMerger
{
    public static class ParallelPreprocessing
    {
        /// <summary>
        /// Performs pre-processing using parallelization. Up to <paramref name="maxThreads"/> threads will be used to process data prior to it being requested by (and simultaneous with) the enumerable consumer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sourceEnum">source enumerable; preferably something like a list of file that need to be loaded</param>
        /// <param name="processFunction">Transform function from <paramref name="sourceEnum"/> to return type; should involve heavy processing (if x => x, you may see a performance penalty)</param>
        /// <param name="maxThreads">Max number of <paramref name="sourceEnum"/> items to process simultaneously</param>
        /// <param name="maxPreprocessed">Max number of items to allow being preprocessed or completed-but-not-consumed at any time; defaults to <paramref name="maxThreads"/></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IEnumerable of items that have been processed via <paramref name="processFunction"/></returns>
        public static IEnumerable<TResult> ParallelPreprocess<T,TResult>(this IEnumerable<T> sourceEnum, Func<T,TResult> processFunction, int maxThreads = 1, int maxPreprocessed = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new ParallelPreprocessor<T,TResult>(sourceEnum, processFunction, maxThreads).ConsumeAll();
        }


        private class ParallelPreprocessor<T,TResult>
        {
            private readonly BufferBlock<TResult> buffer;
            private readonly Semaphore preprocessedLimiter;
            private readonly List<Thread> producerThreads = new List<Thread>();
            private readonly CancellationToken cancelToken;

            /// <summary>
            /// Return a processed item one at a time, as they are requested and become available, until done.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<TResult> ConsumeAll()
            {
                Tuple<bool, TResult> item;

                // while we get an item with a boolean value of true, return the result
                while ((item = TryConsume().Result).Item1)
                {
                    yield return item.Item2;
                }
            }

            /// <summary>
            /// Try to consume an item.
            /// </summary>
            /// <returns>Tuple with a boolean and TResult; the boolean is true if successful, false otherwise</returns>
            /// <remarks>out and ref parameters are not allowed with async methods, otherwise this would just return a bool and have an out parameter with the result</remarks>
            private async Task<Tuple<bool, TResult>> TryConsume()
            {
                while (await buffer.OutputAvailableAsync(cancelToken))
                {
                    preprocessedLimiter.Release(); // release one, allow another item to be preprocessed
                    return new Tuple<bool, TResult>(true, buffer.Receive());
                }

                return new Tuple<bool, TResult>(false, default(TResult));
            }

            private void Start(IEnumerable<T> sourceEnum, Func<T,TResult> processFunction, int numThreads = 1)
            {
                var enumerator = sourceEnum.GetEnumerator();
                var enumeratorLock = new object();

                for (var i = 0; i < numThreads; i++)
                {
                    var thread = new Thread(() => Producer(enumerator, processFunction, enumeratorLock));
                    producerThreads.Add(thread);
                    thread.Start();
                }

                threadMonitor = new Timer(ThreadMonitorCheck, this, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
            }

            private Timer threadMonitor = null;

            private void ThreadMonitorCheck(object sender)
            {
                var done = true;
                foreach (var thread in producerThreads)
                {
                    if (thread.IsAlive)
                    {
                        done = false;
                        break;
                    }
                }

                if (done)
                {
                    // Report no more items
                    buffer.Complete();
                    threadMonitor?.Dispose();
                }
            }

            private async void Producer(IEnumerator<T> sourceEnumerator, Func<T, TResult> processFunction, object accessLock)
            {
                while (true)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    preprocessedLimiter.WaitOne(); // check the preprocessing limit, wait until there is another "space" available
                    T item = default(T);
                    lock (accessLock)
                    {
                        if (!sourceEnumerator.MoveNext())
                        {
                            break;
                        }

                        item = sourceEnumerator.Current;
                    }

                    var processed = processFunction(item);

                    //buffer.Post(organism);
                    var result = await buffer.SendAsync(processed, cancelToken);
                    if (!result)
                    {
                        Console.WriteLine("ERROR: Producer.SendAsync() failed to add item to processing queue!!!");
                    }
                }
            }

            /// <summary>
            /// Performs pre-processing using parallelization. Up to <paramref name="maxThreads"/> threads will be used to process data prior to it being requested by (and simultaneous with) the enumerable consumer.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TResult"></typeparam>
            /// <param name="source">source enumerable; preferably something like a list of file that need to be loaded</param>
            /// <param name="processFunction">Transform function from <paramref name="source"/> to return type; should involve heavy processing (if x => x, you may see a performance penalty)</param>
            /// <param name="maxThreads">Max number of <paramref name="source"/> items to process simultaneously</param>
            /// <param name="maxPreprocessed">Max number of items to allow being preprocessed or completed-but-not-consumed at any time; defaults to <paramref name="maxThreads"/></param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>IEnumerable of items that have been processed via <paramref name="processFunction"/></returns>
            public ParallelPreprocessor(IEnumerable<T> source, Func<T, TResult> processFunction, int maxThreads, int maxPreprocessed = -1, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancelToken = cancellationToken;
                if (maxPreprocessed < 1)
                {
                    maxPreprocessed = maxThreads;
                }

                preprocessedLimiter = new Semaphore(maxPreprocessed, maxPreprocessed);

                buffer = new BufferBlock<TResult>(new DataflowBlockOptions(){ BoundedCapacity = maxPreprocessed });

                Start(source, processFunction, maxThreads);
            }
        }
    }
}
