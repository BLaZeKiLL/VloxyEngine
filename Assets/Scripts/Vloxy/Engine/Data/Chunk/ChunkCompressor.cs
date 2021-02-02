using System;
using System.Collections.Generic;


using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Data {

    public abstract class ChunkCompressor<B> where B : IBlock {

        private int _blockSize = 4;
        private Vector3Int _chunkSize = Vector3Int.one * 32;

        protected abstract B GetBlock(List<byte> bytes);
        
        public List<byte> Compress(B[] blocks) {
            var data = new List<byte>();
            var cblock = blocks[0];
            var index = 1;
            
            data.AddRange(cblock.GetBytes());

            for (int i = 1; i < blocks.Length; i++) {
                if (blocks[i].Equals(cblock)) {
                    index++;
                } else {
                    data.AddRange(BitConverter.GetBytes(index));
                    index = 1;
                    cblock = blocks[i];
                    data.AddRange(cblock.GetBytes());
                }
            }
            
            data.AddRange(BitConverter.GetBytes(index));

            return data;
        }

        public B[] DeCompress(List<byte> data) {
            var blocks = new B[_chunkSize.x * _chunkSize.y * _chunkSize.z];
            var step = _blockSize + sizeof(int);
            var bindex = 0;

            for (int i = 0; i < data.Count; i += step) {
                var block = GetBlock(data.GetRange(i, _blockSize));
                var count = BitConverter.ToInt32(data.GetRange(i + _blockSize, sizeof(int)).ToArray(), 0);

                for (int j = bindex; j < count + bindex; j++) {
                    blocks[j] = block;
                }
            }

            return blocks;
        }

    }

}