using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class DataflowExtensions
    {
        public static async Task ParallelForEachAsync<T>(
            this IEnumerable<T> source,
            Func<T, Task> body,
            int maxDegreeOfParallelism
        )
        {
            var actionBlock = new ActionBlock<T>(body, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            });
            actionBlock.Dispatch(source);

            await actionBlock.Completion.ConfigureAwait(false);
        }

        public static async Task<IEnumerable<TResp>> ParallelForEachAsync<TTarget, TResp>(
            this IEnumerable<TTarget> source,
            Func<TTarget, Task<TResp>> body,
            int maxDegreeOfParallelism
        )
        {
            var transformBlock = new TransformBlock<TTarget, TResp>(body, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            });
            transformBlock.Dispatch(source);
            return await transformBlock.GetOutputsAsync().ConfigureAwait(false);
        }

        private static void Dispatch<T>(this ITargetBlock<T> block, IEnumerable<T> source)
        {
            foreach (var item in source)
                block.Post(item);
            block.Complete();
        }

        private static async Task<IEnumerable<TOutput>> GetOutputsAsync<TOutput>(
            this ISourceBlock<TOutput> transformBlock
        )
        {
            var results = new List<TOutput>();
            while (await transformBlock.OutputAvailableAsync().ConfigureAwait(false))
                results.Add(await transformBlock.ReceiveAsync().ConfigureAwait(false));

            return results;
        }
    }
}
