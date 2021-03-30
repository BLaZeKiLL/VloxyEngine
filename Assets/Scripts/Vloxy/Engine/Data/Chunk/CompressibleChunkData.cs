using System;
using System.Collections.Generic;

using CBSL.Core.Collections.Compressed;

using CodeBlaze.Vloxy.Engine.Extensions;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class CompressibleChunkData<B> : IChunkData<B> where B : IBlock {

        private Vector3Int _chunkSize;

        private CompressedNodeList<B> _data;
        
        public CompressibleChunkData(B[] data) {
            _chunkSize = VoxelProvider<B>.Current.Settings.Chunk.ChunkSize;
            _data = new CompressedNodeList<B>(data);
        }

        public DataState State => _data.State;

        public void Compress() {
            if (State == DataState.COMPRESSED) return;
            _data.Compress();
        }

        public void DeCompress() {
            if (State == DataState.DECOMPRESSED) return;
            _data.Decompress();
        }

        public void SetBlock(B block, int x, int y, int z) => _data.SetAt(_chunkSize.Flatten(x, y, z), block);

        public B GetBlock(int x, int y, int z) => _data.GetAt(_chunkSize.Flatten(x, y, z));

        public void ForEach(Action<B> opt) {
            for (int i = 0; i < _data.Length; i++) {
                opt(_data.GetAt(i));
            }
        }

    }

}