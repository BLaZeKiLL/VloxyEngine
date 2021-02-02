using System;
using System.Collections.Generic;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class CompressedChunkData<B> : IChunkData<B> where B : IBlock {

        private List<byte> _bytes;

        public CompressedChunkData(List<byte> bytes) {
            _bytes = bytes;
        }

        public object GetData() => _bytes;
        
        public void SetBlock(B block, int x, int y, int z) {
            throw new InvalidOperationException("Block writing is not possible on compressed chunk data");
        }

        public B GetBlock(int x, int y, int z) {
            throw new InvalidOperationException("Block reading is not possible on compressed chunk data");
        }

        public override string ToString() => BitConverter.ToString(_bytes.ToArray());

    }

}