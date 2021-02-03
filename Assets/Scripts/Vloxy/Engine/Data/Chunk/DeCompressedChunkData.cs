using System;
using System.IO;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class DeCompressedChunkData<B> : IChunkData<B> where B : IBlock {

        private B[] _blocks;
        private Vector3Int _size;

        public DeCompressedChunkData(B[] blocks, Vector3Int size) {
            if (blocks.Length != size.x * size.y * size.z) throw new InvalidDataException("Unexcpected number of blocks");
            
            _size = size;
            _blocks = blocks;
        }

        public object GetData() => _blocks;
        
        public void SetBlock(B block, int x, int y, int z) {
            try {
                _blocks[FlattenIndex(x, y, z)] = block;
            } catch (IndexOutOfRangeException) {
                throw new IndexOutOfRangeException($"Chunk does not contain index: ({x},{y},{z})");
            }
        }
        
        public B GetBlock(int x, int y, int z) {
            try {
                return _blocks[FlattenIndex(x, y, z)];
            } catch (IndexOutOfRangeException) {
                throw new IndexOutOfRangeException($"Chunk does not contain index: ({x},{y},{z})");
            }
        }

        private int FlattenIndex(int x, int y, int z) =>
            y * _size.x * _size.z +
            z * _size.x +
            x;

    }

}