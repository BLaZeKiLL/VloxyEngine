using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Utils {

    public static class VloxyUtils {

        private static int3 ChunkSize = VloxyProvider.Current.Settings.Chunk.ChunkSize;

        public static int3 GetChunkCoords(Vector3 Position) => GetChunkCoords(Vector3Int.FloorToInt(Position));
        
        public static int3 GetChunkCoords(Vector3Int Position) {
            var x = Position.x - Position.x % ChunkSize.x;
            var y = Position.y - Position.y % ChunkSize.y;
            var z = Position.z - Position.z % ChunkSize.z;
            
            x = Position.x < 0 ? x - ChunkSize.x : x;
            y = Position.y < 0 ? y - ChunkSize.y : y;
            z = Position.z < 0 ? z - ChunkSize.z : z;
            
            return new int3(x,y,z);
        }

    }

}