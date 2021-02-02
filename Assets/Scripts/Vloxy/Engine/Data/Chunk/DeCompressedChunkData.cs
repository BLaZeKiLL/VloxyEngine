using System;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class DeCompressedChunkData<B> : IChunkData<B> where B : IBlock {

        private B[] _blocks;
        private Vector3Int _size;

        public DeCompressedChunkData(Vector3Int size) {
            _size = size;
            _blocks = new B[_size.x * _size.y * _size.z];
        }

        public void SetBlock(B block, int x, int y, int z) {
            if (!ContainsIndex(x, y, z)) {
                throw new IndexOutOfRangeException($"Chunk does not contain index: ({x},{y},{z})");
            }

            _blocks[FlattenIndex(x, y, z)] = block;
        }
        
        public B GetBlock(int x, int y, int z) {
            if (!ContainsIndex(x, y, z)) {
                throw new IndexOutOfRangeException($"Chunk does not contain index: ({x},{y},{z})");
            }
            
            return _blocks[FlattenIndex(x, y, z)];
        }

        private int FlattenIndex(int x, int y, int z) =>
            y * _size.x * _size.z +
            z * _size.x +
            x;

        private bool ContainsIndex(int x, int y, int z) =>
            x >= 0 && x < _size.x &&
            y >= 0 && y < _size.y &&
            z >= 0 && z < _size.z;

    }

}