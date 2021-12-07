using System;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    /// <summary>
    /// For fixed size worlds, for there to be edge faces ChunkPageSize = DrawDistance
    /// </summary>
    [Serializable]
    public class ChunkSettings {
        
        [Tooltip("Number of chunks per page = (2 * chunk_page_size + 1)^2")]
        public int ChunkPageSize = 8;
        
        [Tooltip("Number of chunk_behaviours per page = (2 * draw_distance + 1)^2")]
        public int DrawDistance = 4;

        [Tooltip("Chunk dimensions")]
        public int3 ChunkSize = 32 * new int3(1,1,1);

    }

}