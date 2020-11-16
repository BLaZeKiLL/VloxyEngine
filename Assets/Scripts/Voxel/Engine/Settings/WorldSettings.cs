using System;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Settings {

    [Serializable]
    public class WorldSettings {
        
        public int ChunkPageSize = 25;
        public int DrawDistance = 1;
        [Range(0.01f, 0.99f)] public float Frequency = 0.15f;
        public Vector3Int ChunkSize = 16 * Vector3Int.one; // this is serialized, rider issue

    }

}