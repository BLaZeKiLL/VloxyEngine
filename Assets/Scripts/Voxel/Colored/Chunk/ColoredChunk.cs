using System;
using System.Collections.Generic;

using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Engine.Core;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored.Chunk {

    public class ColoredChunk : Chunk<ColoredBlock> {

        public Vector3Int Position { get; }
        
        public int ID { get; }

        public ColoredChunk(Vector3Int size, Vector3Int position, int Id) : base(size) {
            Position = position;
            ID = Id;
        }

    }

    public static class ColoredChunkExtensions {

        public static void Fill(this ColoredChunk chunk, ColoredBlock block) {
            for (int x = 0; x < chunk.Size.x; x++) {
                for (int y = 0; y < chunk.Size.y; y++) {
                    for (int z = 0; z < chunk.Size.z; z++) {
                        chunk.SetBlock(block, x, y, z);
                    }
                }
            }
        }

        public static void Fill(this ColoredChunk chunk, IEnumerable<Vector3Int> indexes, ColoredBlock block) {
            foreach (var index in indexes) {
                chunk.SetBlock(block, index);
            }
        }

    }

}