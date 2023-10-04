using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Utils {

    public static class VloxyUtils {

        private static int3 ChunkSize = VloxyProvider.Current.Settings.Chunk.ChunkSize;

        public static int3 GetChunkCoords(Vector3 Position) => GetChunkCoords(Vector3Int.FloorToInt(Position));

        public static int3 GetChunkCoords(Vector3Int Position) => GetChunkCoords(Position.Int3());
        
        public static int3 GetChunkCoords(int3 Position) {
            var modX = Position.x % ChunkSize.x;
            var modY = Position.y % ChunkSize.y;
            var modZ = Position.z % ChunkSize.z;
            
            var x = Position.x - modX;
            var y = Position.y - modY;
            var z = Position.z - modZ;
            
            x = Position.x < 0 && modX != 0 ? x - ChunkSize.x : x;
            y = Position.y < 0 && modY != 0 ? y - ChunkSize.y : y;
            z = Position.z < 0 && modZ != 0 ? z - ChunkSize.z : z;
            
            return new int3(x,y,z);
        }

        public static int3 GetBlockIndex(Vector3 Position) => GetBlockIndex(Vector3Int.FloorToInt(Position));

        public static int3 GetBlockIndex(Vector3Int Position) {
            var chunk_coords = GetChunkCoords(Position);

            return new int3(Position.x - chunk_coords.x, Position.y - chunk_coords.y, Position.z - chunk_coords.z);
        }

        public static int GetBlockId(Block block) => (int) block;

        public static readonly int3[] Directions = {
            new(1, 0, 0),
            new(-1, 0, 0),
            
            new(0, 1, 0),
            new(0, -1, 0),
            
            new(0, 0, 1),
            new(0, 0, -1),
        };

    }

}