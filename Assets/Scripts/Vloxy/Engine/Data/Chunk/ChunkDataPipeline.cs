using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkDataPipeline<B> where B : IBlock {

        private List<Func<IChunkData<B>, IChunkData<B>>> Funcs;

        public ChunkDataPipeline(List<Func<IChunkData<B>, IChunkData<B>>> funcs) {
            Funcs = funcs;
        }

        public IChunkData<B> Apply(IChunkData<B> data) => Funcs.Aggregate(data, (current, func) => current == null ? null : func(current));
     
        public static class Functions {

            public static readonly Func<IChunkData<B>, IChunkData<B>> EmptyChunkRemover = data => {
                var empty = true;

                data.ForEach(block => empty &= block.IsTransparent());

                return !empty ? data : null;
            };

            public static readonly Func<IChunkData<B>, IChunkData<B>> ChunkDataCompressor = data => {
                ((CompressibleChunkData<B>) data).Compress();

                return data;
            };

            public static readonly Func<IChunkData<B>, IChunkData<B>> ChunkDataDecompressor = data => {
                ((CompressibleChunkData<B>) data).Compress();

                return data;
            };

        }
        
    }

}