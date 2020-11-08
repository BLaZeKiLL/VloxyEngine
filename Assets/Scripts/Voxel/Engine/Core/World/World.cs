using System;
using System.Collections.Generic;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Core.World {

    public class World<T> : MonoBehaviour where T : IBlock {

        [SerializeField] private Settings _worldSettings;
        
        public Settings CurrentSettings {
            get => _worldSettings;
            set => _worldSettings = value;
        }

        protected Dictionary<Vector3Int, Chunk<T>> Chunks;

        protected virtual void Awake() {
            Chunks = new Dictionary<Vector3Int, Chunk<T>>();
        }

        #region Neighbors
        
        public NeighborChunks<T> GetNeighbors(Chunk<T> chunk) {
            var position = chunk.Position;

            var px = position + Vector3Int.right * CurrentSettings.ChunkSize;
            var py = position + Vector3Int.up * CurrentSettings.ChunkSize;
            var pz = position + new Vector3Int(0, 0, 1) * CurrentSettings.ChunkSize;
            var nx = position + Vector3Int.left * CurrentSettings.ChunkSize;
            var ny = position + Vector3Int.down * CurrentSettings.ChunkSize;
            var nz = position + new Vector3Int(0, 0, -1) * CurrentSettings.ChunkSize;
            
            return new NeighborChunks<T> {
                ChunkPX = Chunks.ContainsKey(px) ? Chunks[px] : null,
                ChunkPY = Chunks.ContainsKey(py) ? Chunks[py] : null,
                ChunkPZ = Chunks.ContainsKey(pz) ? Chunks[pz] : null,
                ChunkNX = Chunks.ContainsKey(nx) ? Chunks[nx] : null,
                ChunkNY = Chunks.ContainsKey(ny) ? Chunks[ny] : null,
                ChunkNZ = Chunks.ContainsKey(nz) ? Chunks[nz] : null
            };
        }

        public Chunk<T> GetNeighborPX(Chunk<T> chunk) {
            var px = chunk.Position + Vector3Int.right * CurrentSettings.ChunkSize;

            return Chunks.ContainsKey(px) ? Chunks[px] : null;
        }
        
        public Chunk<T> GetNeighborPY(Chunk<T> chunk) {
            var py = chunk.Position + Vector3Int.up * CurrentSettings.ChunkSize;

            return Chunks.ContainsKey(py) ? Chunks[py] : null;
        }
        
        public Chunk<T> GetNeighborPZ(Chunk<T> chunk) {
            var pz = chunk.Position + new Vector3Int(0, 0, 1) * CurrentSettings.ChunkSize;

            return Chunks.ContainsKey(pz) ? Chunks[pz] : null;
        }
        
        public Chunk<T> GetNeighborNX(Chunk<T> chunk) {
            var nx = chunk.Position + Vector3Int.left * CurrentSettings.ChunkSize;

            return Chunks.ContainsKey(nx) ? Chunks[nx] : null;
        }
        
        public Chunk<T> GetNeighborNY(Chunk<T> chunk) {
            var ny = chunk.Position + Vector3Int.down * CurrentSettings.ChunkSize;

            return Chunks.ContainsKey(ny) ? Chunks[ny] : null;
        }
        
        public Chunk<T> GetNeighborNZ(Chunk<T> chunk) {
            var nz = chunk.Position + new Vector3Int(0, 0, -1) * CurrentSettings.ChunkSize;

            return Chunks.ContainsKey(nz) ? Chunks[nz] : null;
        }
        
        #endregion

        [Serializable]
        public class Settings {

            public int DrawSize = 1;
            [Range(0.01f, 0.99f)] public float Frequency = 0.15f;
            public Vector3Int ChunkSize = 16 * Vector3Int.one; // this is serialized, rider issue

        }

    }

}