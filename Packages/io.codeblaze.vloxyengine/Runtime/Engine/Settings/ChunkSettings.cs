using System;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    /// <summary>
    /// For fixed size worlds, for there to be edge faces ChunkPageSize = DrawDistance
    /// </summary>
    [Serializable]
    public class ChunkSettings {

        [Tooltip("Number of active chunks = (2 * draw_distance + 1)^2")]
        public int DrawDistance = 2;

        [Tooltip("Chunk dimensions")]
        public int3 ChunkSize = 32 * new int3(1,1,1);

        [HideInInspector]
        [Tooltip("Number of chunks in memory = (draw_distance + 2)")]
        public int LoadDistance = 0;

    }

}