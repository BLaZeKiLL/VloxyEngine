using System;

using CodeBlaze.Voxel.Engine.Behaviour;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Chunk {

    public abstract class Chunk<B> where B : IBlock  {

        protected readonly B[] Blocks;
        
        public Vector3Int Position { get; }
        public Vector3Int Size { get; }
        
        protected Chunk(Vector3Int size, Vector3Int position) {
            Size = size;
            Position = position;
            Blocks = new B[Size.x * Size.y * Size.z];
        }

        public void SetBlock(B block, Vector3Int index) => SetBlock(block, index.x, index.y, index.z);

        public void SetBlock(B block, int x, int y, int z) {
            if (!ContainsIndex(x, y, z)) {
                throw new IndexOutOfRangeException($"Chunk does not contain index: ({x},{y},{z})");
            }

            Blocks[FlattenIndex(x, y, z)] = block;
        }

        public B GetBlock(Vector3Int index) => GetBlock(index.x, index.y, index.z);

        public B GetBlock(int x, int y, int z) {
            if (!ContainsIndex(x, y, z)) {
                throw new IndexOutOfRangeException($"Chunk does not contain index: ({x},{y},{z})");
            }
            
            return Blocks[FlattenIndex(x, y, z)];
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