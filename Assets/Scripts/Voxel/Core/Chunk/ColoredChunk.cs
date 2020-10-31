using System;
using System.Collections.Generic;

using UnityEngine;

namespace CodeBlaze.Voxel.Core {

    public class ColoredChunk : Chunk<Block> {

        public ColoredChunk(Vector3Int size) : base(size) { }

    }

    public static class ColoredChunkExtensions {

        public static void Fill(this ColoredChunk chunk, Block block) {
            for (int x = 0; x < chunk.Size.x; x++) {
                for (int y = 0; y < chunk.Size.y; y++) {
                    for (int z = 0; z < chunk.Size.z; z++) {
                        chunk.SetBlock(block, x, y, z);
                    }
                }
            }
        }

        public static void Fill(this ColoredChunk chunk, IEnumerable<Vector3Int> indexes, Block block) {
            foreach (var index in indexes) {
                chunk.SetBlock(block, index);
            }
        }

    }

}