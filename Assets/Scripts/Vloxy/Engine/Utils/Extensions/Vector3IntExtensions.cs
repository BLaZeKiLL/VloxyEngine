using System;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Utils.Extensions {

    public static class Vector3IntExtensions {

        public static int Flatten(this Vector3Int vec, int x, int y, int z) =>
            y * vec.x * vec.z +
            z * vec.x +
            x;

        public static int Flatten(this Vector3Int vec, Vector3Int pos) =>
            pos.y * vec.x * vec.z +
            pos.z * vec.x +
            pos.x;

        public static int Size(this Vector3Int vec) => vec.x * vec.y * vec.z;

        public static void ForEach(this Vector3Int vec, Action<int, int> action) {
            action(vec.x, 0);
            action(vec.y, 1);
            action(vec.z, 2);
        }

    }

}