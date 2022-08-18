using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Utils.Extensions {

    [BurstCompatible]
    public static class MathExtensions {

        [BurstCompile]
        public static int CubedSize(this int num) => (2 * num + 1) * (2 * num + 1) * (2 * num + 1);
        
        [BurstCompile]
        public static int YCubedSize(this int num, int y) => (2 * num + 1) * (2 * num + 1) * (2 * y + 1);
        
        [BurstCompile]
        public static int Flatten(this int3 vec, int x, int y, int z) =>
            y * vec.x * vec.z +
            z * vec.x +
            x;

        [BurstCompile]
        public static int Flatten(this int3 vec, int3 pos) =>
            pos.y * vec.x * vec.z +
            pos.z * vec.x +
            pos.x;

        [BurstCompile]
        public static bool OrReduce(this bool3 val) => val.x || val.y || val.z;
        
        [BurstCompile]
        public static bool AndReduce(this bool3 val) => val.x && val.y && val.z;

        [BurstCompile]
        public static int Size(this int3 vec) => vec.x * vec.y * vec.z;

        [BurstCompile]
        public static Vector3Int GetVector3Int(this int3 vec) => new(vec.x, vec.y, vec.z);
        
        [BurstCompile]
        public static Vector3 GetVector3(this int3 vec) => new(vec.x, vec.y, vec.z);

    }

}