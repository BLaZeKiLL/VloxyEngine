using System;
using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Library.Collections.Pools;
using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine.Behaviour;
using CodeBlaze.Voxel.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.World {

    public abstract class World<B> : MonoBehaviour where B : IBlock {

        [SerializeField] private Transform _focus;
        [SerializeField] private VoxelSettings _settings;

        public IObjectPool<ChunkBehaviour> ChunkPool { get; private set; }

        protected WorldSettings WorldSettings { get; private set; }
        protected Dictionary<Vector3Int, Chunk<B>> Chunks;
        protected MeshBuildCoordinator<B> BuildCoordinator;
        protected Vector3Int FocusChunkCoord;

        private int _poolSize;
        private List<Vector3Int> _activeChunks;
        
        protected virtual void Awake() {
            VoxelProvider.Initialize(_settings);

            WorldSettings = VoxelProvider.Current.Settings.World;

            Chunks = new Dictionary<Vector3Int, Chunk<B>>();
            _activeChunks = new List<Vector3Int>();
            
            _poolSize = (2 * WorldSettings.DrawDistance + 1) * (2 * WorldSettings.DrawDistance + 1) + 1;
            ChunkPool = CreateRendererPool();
            BuildCoordinator = MeshBuildCoordinatorProvider();

            FocusChunkCoord = _focus != null
                ? GetChunkCoords(_focus.position)
                : Vector3Int.zero;
        }

        protected void Start() {
            
            for (int x = -WorldSettings.ChunkPageSize; x <= WorldSettings.ChunkPageSize; x++) {
                for (int z = -WorldSettings.ChunkPageSize; z <= WorldSettings.ChunkPageSize; z++) {
                    var pos = new Vector3Int(x, 0, z) * WorldSettings.ChunkSize;
                    Chunks.Add(pos, CreateChunk(pos));
                }
            }

            WorldUpdate();
            
            Debug.Log("[World][Start] Done");
        }

        protected virtual void Update() {
            var coords = GetChunkCoords(_focus.position);

            if (coords.x == FocusChunkCoord.x && coords.z == FocusChunkCoord.z) return;

            FocusChunkCoord = coords;
                
            // update
            WorldUpdate();
        }
        
        protected abstract MeshBuildCoordinator<B> MeshBuildCoordinatorProvider();

        protected abstract Chunk<B> CreateChunk(Vector3Int position);

        protected virtual void WorldUpdate() {
            var current = new List<Vector3Int>(_poolSize);

            var focusPosition = GetChunkCoords(_focus.transform.position);
            
            for (int x = -WorldSettings.DrawDistance; x <= WorldSettings.DrawDistance; x++) {
                for (int z = -WorldSettings.DrawDistance; z <= WorldSettings.DrawDistance; z++) {
                    current.Add( focusPosition + new Vector3Int(x,0,z) * WorldSettings.ChunkSize);
                }
            }

            var reclaim = _activeChunks.Where(x => !current.Contains(x)).ToList();
            var claim = current.Where(x => !_activeChunks.Contains(x)).ToList();
            
            Debug.Log($"[World][Update] Reclaim : {reclaim.Count} Claim : {claim.Count}");
            
            foreach (var x in reclaim) {
                ChunkPool.Reclaim(Chunks[x].Behaviour);
            }
            
            foreach (var x in claim) {
                BuildCoordinator.Add(Chunks[x]);
            }

            _activeChunks = current;
            
            BuildCoordinator.Process();
        }

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

        #region Utils

        public Vector3Int GetChunkCoords(Vector3 Position) {
            var pos = Vector3Int.FloorToInt(Position);

            var x = pos.x - pos.x % WorldSettings.ChunkSize.x;
            var y = 0; //pos.y - pos.y % WorldSettings.ChunkSize.y;
            var z = pos.z - pos.z % WorldSettings.ChunkSize.z;

            x = pos.x < 0 ? x - WorldSettings.ChunkSize.x : x;
            //y = pos.y < 0 ? y - WorldSettings.ChunkSize.y : y;
            z = pos.z < 0 ? z - WorldSettings.ChunkSize.z : z;
            
            return new Vector3Int(x,y,z);
        }
        
        public Vector3Int GetChunkCoords(Vector3Int Position) {
            var x = Position.x - Position.x % WorldSettings.ChunkSize.x;
            var y = 0;//Position.y - Position.y % WorldSettings.ChunkSize.y;
            var z = Position.z - Position.z % WorldSettings.ChunkSize.z;
            
            return new Vector3Int(x,y,z);
        }

        #endregion

        #region Private

        private IObjectPool<ChunkBehaviour> CreateRendererPool() => new ObjectPool<ChunkBehaviour>( // pool size = x^2 + 1
            _poolSize,
            index => {
                var go = new GameObject("Chunk", typeof(ChunkBehaviour));
                go.transform.parent = transform;
                go.SetActive(false);
            
                var chunkRenderer = go.GetComponent<ChunkBehaviour>();
                chunkRenderer.SetRenderSettings(VoxelProvider.Current.Settings.ChunkRenderer);

                return chunkRenderer;
            },
            chunkRenderer => chunkRenderer.gameObject.SetActive(true),
            chunkRenderer => chunkRenderer.gameObject.SetActive(false)
        );

        #endregion

    }

}