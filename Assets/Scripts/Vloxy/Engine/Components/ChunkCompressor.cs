using System;
using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Extensions;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Components {

    public abstract class ChunkCompressor<B> where B : IBlock {

        private int _blockSize;
        private Vector3Int _chunkSize;

        protected ChunkCompressor(int blockSize, Vector3Int chunkSize) {
            _blockSize = blockSize;
            _chunkSize = chunkSize;
        }

        protected abstract B GetBlock(List<byte> bytes);
        
        public IChunkData<B> Compress(IChunkData<B> data) {
            var blocks = (B[]) data.GetData();
            var bytes = new List<byte>();
            var cblock = blocks[0];
            var index = 1;
            
            bytes.AddRange(cblock.GetBytes());

            for (int i = 1; i < blocks.Length; i++) {
                if (blocks[i].Equals(cblock)) {
                    index++;
                } else {
                    bytes.AddRange(BitConverter.GetBytes(index));
                    index = 1;
                    cblock = blocks[i];
                    bytes.AddRange(cblock.GetBytes());
                }
            }

            bytes.AddRange(BitConverter.GetBytes(index));

            return new CompressedChunkData<B>(bytes);
        }

        public IChunkData<B> DeCompress(IChunkData<B> data) {
            var bytes = (List<byte>) data.GetData();
            var blocks = new B[_chunkSize.Size()];
            var step = _blockSize + sizeof(int);
            var bindex = 0;

            for (int i = 0; i < bytes.Count; i += step) {
                var block = GetBlock(bytes.GetRange(i, _blockSize));
                var count = BitConverter.ToInt32(bytes.GetRange(i + _blockSize, sizeof(int)).ToArray(), 0);

                for (int j = bindex; j < count + bindex; j++) {
                    blocks[j] = block;
                }

                bindex += count;
            }

            return new DeCompressedChunkData<B>(blocks, _chunkSize);
        }

    }

}