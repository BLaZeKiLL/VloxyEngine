using System;
using System.Collections.Generic;

using UnityEngine;

namespace CodeBlaze.Voxel.Core {

    public class Chunk {

        private Block[] _blocks;

        public Chunk(Vector3Int size) {
            Size = size;
            _blocks = new Block[Size.x * Size.y * Size.z];
        }

        public Vector3Int Size { get; }

        public void SetBlock(Block block, Vector3Int index) => SetBlock(block, index.x, index.y, index.z);

        public void SetBlock(Block block, int x, int y, int z) {
            if (!ContainsIndex(x, y, z)) {
                throw new IndexOutOfRangeException($"Chunk does not contain index: ({x},{y},{z})");
            }

            _blocks[FlattenIndex(x, y, z)] = block;
        }

        public Block GetBlock(Vector3Int index) => GetBlock(index.x, index.y, index.z);

        public Block GetBlock(int x, int y, int z) {
            return !ContainsIndex(x, y, z) ? BlockTypes.Air() : _blocks[FlattenIndex(x, y, z)];
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

    public static class ChunkExtensions {

        public static void Fill(this Chunk chunk, Block block) {
            for (int x = 0; x < chunk.Size.x; x++) {
                for (int y = 0; y < chunk.Size.y; y++) {
                    for (int z = 0; z < chunk.Size.z; z++) {
                        chunk.SetBlock(block, x, y, z);
                    }
                }
            }
        }

        public static void Fill(this Chunk chunk, IEnumerable<Vector3Int> indexes, Block block) {
            foreach (var index in indexes) {
                chunk.SetBlock(block, index);
            }
        }

    }

}