using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Utils.Extensions {

    [GenerateTestsForBurstCompatibility]
    public static class BurstMathExtensions {

        [BurstCompile]
        public static int CubedSize(this int num) => (2 * num + 1) * (2 * num + 1) * (2 * num + 1);
        
        [BurstCompile]
        public static int CubedSize(this int3 num) => (2 * num.x + 1) * (2 * num.y + 1) * (2 * num.z + 1);
        
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

    public static class MathExtension {

        public static int SqrMagnitude(this int3 vec) => vec.x * vec.x + vec.y * vec.y + vec.z * vec.z;

        public static int3 MemberMultiply(this int3 a, int3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
        
        public static int3 MemberMultiply(this int3 a, int x, int y, int z) => new(a.x * x, a.y * y, a.z * z);

    }

    public static class VectorExtension {

        public static int3 Int3(this Vector3Int vec) => new(vec.x, vec.y, vec.z);
        
        public static int3 Int3(this Vector3 vec) => Vector3Int.FloorToInt(vec).Int3();

    }
    
}