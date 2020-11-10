using System.Collections.Generic;

using CodeBlaze.Library.Collections.Pools;
using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine.Behaviour;
using CodeBlaze.Voxel.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.World {

    public abstract class World<B> : MonoBehaviour where B : IBlock {

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

        public IObjectPool<ChunkBehaviour> ChunkPool { get; private set; }
        
        protected Dictionary<Vector3Int, Chunk<B>> Chunks;
        protected MeshBuildCoordinator<B> BuildCoordinator;

        protected virtual void Awake() {
            Chunks = new Dictionary<Vector3Int, Chunk<B>>();

            ChunkPool = CreateRendererPool(WorldSettings.DrawSize);
            BuildCoordinator = MeshBuildCoordinatorProvider();
        }

        protected abstract MeshBuildCoordinator<B> MeshBuildCoordinatorProvider();
        
        #region Neighbors
        
        public NeighborChunks<B> GetNeighbors(Chunk<B> chunk) {
            var position = chunk.Position;

            var px = position + Vector3Int.right * WorldSettings.ChunkSize;
            var py = position + Vector3Int.up * WorldSettings.ChunkSize;
            var pz = position + new Vector3Int(0, 0, 1) * WorldSettings.ChunkSize;
            var nx = position + Vector3Int.left * WorldSettings.ChunkSize;
            var ny = position + Vector3Int.down * WorldSettings.ChunkSize;
            var nz = position + new Vector3Int(0, 0, -1) * WorldSettings.ChunkSize;
            
            return new NeighborChunks<B> {
                ChunkPX = Chunks.ContainsKey(px) ? Chunks[px] : null,
                ChunkPY = Chunks.ContainsKey(py) ? Chunks[py] : null,
                ChunkPZ = Chunks.ContainsKey(pz) ? Chunks[pz] : null,
                ChunkNX = Chunks.ContainsKey(nx) ? Chunks[nx] : null,
                ChunkNY = Chunks.ContainsKey(ny) ? Chunks[ny] : null,
                ChunkNZ = Chunks.ContainsKey(nz) ? Chunks[nz] : null
            };
        }

        public Chunk<B> GetNeighborPX(Chunk<B> chunk) {
            var px = chunk.Position + Vector3Int.right * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(px) ? Chunks[px] : null;
        }
        
        public Chunk<B> GetNeighborPY(Chunk<B> chunk) {
            var py = chunk.Position + Vector3Int.up * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(py) ? Chunks[py] : null;
        }
        
        public Chunk<B> GetNeighborPZ(Chunk<B> chunk) {
            var pz = chunk.Position + new Vector3Int(0, 0, 1) * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(pz) ? Chunks[pz] : null;
        }
        
        public Chunk<B> GetNeighborNX(Chunk<B> chunk) {
            var nx = chunk.Position + Vector3Int.left * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(nx) ? Chunks[nx] : null;
        }
        
        public Chunk<B> GetNeighborNY(Chunk<B> chunk) {
            var ny = chunk.Position + Vector3Int.down * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(ny) ? Chunks[ny] : null;
        }
        
        public Chunk<B> GetNeighborNZ(Chunk<B> chunk) {
            var nz = chunk.Position + new Vector3Int(0, 0, -1) * WorldSettings.ChunkSize;

            return Chunks.ContainsKey(nz) ? Chunks[nz] : null;
        }
        
        #endregion
        
        private IObjectPool<ChunkBehaviour> CreateRendererPool(int drawSize) => new ObjectPool<ChunkBehaviour>( // pool size = x^2 + 1
            (2 * drawSize + 1) * (2 * drawSize + 1) + 1,
            index => {
                var go = new GameObject("Chunk", typeof(ChunkBehaviour));
                go.transform.parent = transform;
                go.SetActive(false);
            
                var chunkRenderer = go.GetComponent<ChunkBehaviour>();
                chunkRenderer.SetRenderSettings(ChunkRendererSettings.Material, ChunkRendererSettings.CastShadows);

                return chunkRenderer;
            },
            chunkRenderer => chunkRenderer.gameObject.SetActive(true),
            chunkRenderer => chunkRenderer.gameObject.SetActive(false)
        );
        
    }

}