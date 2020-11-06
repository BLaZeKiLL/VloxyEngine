using System;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Core {

    public abstract class Chunk<T> where T : IBlock  {

        protected T[] _blocks;

        protected Chunk(Vector3Int size) {
            Size = size;
            _blocks = new T[Size.x * Size.y * Size.z];
        }
        
        public Vector3Int Size { get; }
        
        public void SetBlock(T block, Vector3Int index) => SetBlock(block, index.x, index.y, index.z);

        public void SetBlock(T block, int x, int y, int z) {
            if (!ContainsIndex(x, y, z)) {
                throw new IndexOutOfRangeException($"Chunk does not contain index: ({x},{y},{z})");
            }

            _blocks[FlattenIndex(x, y, z)] = block;
        }

        public T GetBlock(Vector3Int index) => GetBlock(index.x, index.y, index.z);

        public T GetBlock(int x, int y, int z) {
            if (!ContainsIndex(x, y, z)) {
                throw new IndexOutOfRangeException($"Chunk does not contain index: ({x},{y},{z})");
            }
            
            return _blocks[FlattenIndex(x, y, z)];
        }

        private int FlattenIndex(int x, int y, int z) =>
            y * Size.x * Size.z +
            z * Size.x +
            x;

        private bool ContainsIndex(int x, int y, int z) =>
            x >= 0 && x < Size.x &&
            y >= 0 && y < Size.y &&
            z >= 0 && z < Size.z;
        
    }

}