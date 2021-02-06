using System.Collections.Generic;

using CBSL.Core.Collections.Compressed;

using CodeBlaze.Vloxy.Engine.Extensions;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Data {

    public abstract class CompressibleChunkData<B> : CompressedArray<B>, IChunkData<B> where B : IBlock {

        private Vector3Int _chunkSize;
        
        public CompressibleChunkData(B[] data, int dataSize, Vector3Int chunkSize) : base(data, dataSize) {
            _chunkSize = chunkSize;
        }

        public CompressibleChunkData(List<byte> bytes, int dataSize, Vector3Int chunkSize) : base(bytes, chunkSize.Size(), dataSize) {
            _chunkSize = chunkSize;
        }

        protected abstract override B FromBytes(byte[] bytes);

        protected abstract override byte[] GetBytes(B obj);

        public void SetBlock(B block, int x, int y, int z) => GetDeCompressedData()[_chunkSize.Flatten(x, y, z)] = block;

        public B GetBlock(int x, int y, int z) => GetDeCompressedData()[_chunkSize.Flatten(x, y, z)];

    }

}