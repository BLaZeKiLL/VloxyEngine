using System;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    /// <summary>
    /// For fixed size worlds, for there to be edge faces ChunkPageSize = DrawDistance
    /// </summary>
    [Serializable]
    public class WorldSettings {
        
        [Tooltip("Number of chunks per page = (2 * chunk_page_size + 1)^2")]
        public int ChunkPageSize = 32;
        
        [Tooltip("Number of chunk_behaviours per page = (2 * draw_distance + 1)^2")]
        public int DrawDistance = 7;

        [Tooltip("Chunk dimensions")]
        public Vector3Int ChunkSize = 32 * Vector3Int.one;

    }

}