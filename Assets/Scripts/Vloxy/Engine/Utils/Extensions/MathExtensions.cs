using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Utils.Extensions {

    public static class MathExtensions {

        public static int CubedSize(this int num) => (2 * num + 1) * (2 * num + 1) * (2 * num + 1);
        
        public static int Flatten(this int3 vec, int x, int y, int z) =>
            y * vec.x * vec.z +
            z * vec.x +
            x;

        public static int Flatten(this int3 vec, int3 pos) =>
            pos.y * vec.x * vec.z +
            pos.z * vec.x +
            pos.x;

        public static int Size(this int3 vec) => vec.x * vec.y * vec.z;

        public static Vector3Int GetVector3Int(this int3 vec) => new(vec.x, vec.y, vec.z);
        
        public static Vector3 GetVector3(this int3 vec) => new(vec.x, vec.y, vec.z);

    }

}