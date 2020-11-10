using System.Collections.Generic;

using CodeBlaze.Library.Collections.Pools;
using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine.Renderer;
using CodeBlaze.Voxel.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.World {

    public abstract class World<T> : MonoBehaviour where T : IBlock {

        [SerializeField] private WorldSettings _worldSettings;
        [SerializeField] private ChunkRendererSettings _chunkRendererSettings;
        [SerializeField] private BuildCoordinatorSettings _buildCoordinatorSettings;

        #region Settings

        public WorldSettings WorldSettings {
            get => _worldSettings;
            set => _worldSettings = value;
        }

        public ChunkRendererSettings ChunkRendererSettings {
            get => _chunkRendererSettings;
            set => _chunkRendererSettings = value;
        }

        public BuildCoordinatorSettings BuildCoordinatorSettings {
            get => _buildCoordinatorSettings;
            set => _buildCoordinatorSettings = value;
        }

        #endregion

        public IObjectPool<ChunkRenderer> RendererPool { get; private set; }
        
        protected Dictionary<Vector3Int, Chunk<T>> Chunks;
        protected MeshBuildCoordinator<T> BuildCoordinator;

        protected virtual void Awake() {
            Chunks = new Dictionary<Vector3Int, Chunk<T>>();

            RendererPool = CreateRendererPool(WorldSettings.DrawSize);
            BuildCoordinator = MeshBuildCoordinatorProvider();
        }

        protected abstract MeshBuildCoordinator<T> MeshBuildCoordinatorProvider();
        
        #region Neighbors
        
        public NeighborChunks<T> GetNeighbors(Chunk<T> chunk) {
            var position = chunk.Position;

            var px = position + Vector3Int.right * WorldSettings.ChunkSize;
            var py = position + Vector3Int.up * WorldSettings.ChunkSize;
            var pz = position + new Vector3Int(0, 0, 1) * WorldSettings.ChunkSize;
            var nx = position + Vector3Int.left * WorldSettings.ChunkSize;
            var ny = position + Vector3Int.down * WorldSettings.ChunkSize;
            var nz = position + new Vector3Int(0, 0, -1) * WorldSettings.ChunkSize;
            
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
            var px = chunk.Position + Vector3Int.right * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(px) ? Chunks[px] : null;
        }
        
        public Chunk<T> GetNeighborPY(Chunk<T> chunk) {
            var py = chunk.Position + Vector3Int.up * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(py) ? Chunks[py] : null;
        }
        
        public Chunk<T> GetNeighborPZ(Chunk<T> chunk) {
            var pz = chunk.Position + new Vector3Int(0, 0, 1) * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(pz) ? Chunks[pz] : null;
        }
        
        public Chunk<T> GetNeighborNX(Chunk<T> chunk) {
            var nx = chunk.Position + Vector3Int.left * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(nx) ? Chunks[nx] : null;
        }
        
        public Chunk<T> GetNeighborNY(Chunk<T> chunk) {
            var ny = chunk.Position + Vector3Int.down * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(ny) ? Chunks[ny] : null;
        }
        
        public Chunk<T> GetNeighborNZ(Chunk<T> chunk) {
            var nz = chunk.Position + new Vector3Int(0, 0, -1) * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(nz) ? Chunks[nz] : null;
        }
        
        #endregion
        
        private IObjectPool<ChunkRenderer> CreateRendererPool(int drawSize) => new ObjectPool<ChunkRenderer>( // pool size = x^2 + 1
            (2 * drawSize + 1) * (2 * drawSize + 1) + 1,
            index => {
                var go = new GameObject("Chunk", typeof(ChunkRenderer));
                go.transform.parent = transform;
                go.SetActive(false);
            
                var chunkRenderer = go.GetComponent<ChunkRenderer>();
                chunkRenderer.SetRenderSettings(ChunkRendererSettings.Material, ChunkRendererSettings.CastShadows);

                return chunkRenderer;
            },
            chunkRenderer => chunkRenderer.gameObject.SetActive(true),
            chunkRenderer => chunkRenderer.gameObject.SetActive(false)
        );
        
    }

}