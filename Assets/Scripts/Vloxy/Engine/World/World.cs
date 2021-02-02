using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.World {

    public class World<B> : MonoBehaviour where B : IBlock {

        private const string TAG = "<color=cyan>World</color>";
        
        [SerializeField] private Transform _focus;
        [SerializeField] private VoxelSettings _settings;

        public ChunkPool<B> ChunkPool { get; private set; }
        protected MeshBuildCoordinator<B> BuildCoordinator;
        protected INoiseProfile<B> NoiseProfile;
        
        protected Dictionary<Vector3Int, Chunk<B>> Chunks;
        protected Vector3Int FocusChunkCoord;

        private ChunkSettings _chunkSettings;
        
        #region Virtual

        protected virtual VoxelProvider<B> Provider() => new VoxelProvider<B>();
        protected virtual void WorldAwake() { }
        protected virtual void WorldStart() { }
        protected virtual void WorldUpdate() { }
        protected virtual void WorldChunkPoolUpdate() { }

        #endregion

        #region Unity

        private void Awake() {
            VoxelProvider<B>.Initialize(Provider(), provider => {
                provider.Settings = _settings;
            });
            
            Debug.unityLogger.Log(TAG,"Provider Initialized");

            _chunkSettings = VoxelProvider<B>.Current.Settings.Chunk;

            Chunks = new Dictionary<Vector3Int, Chunk<B>>();
            
            ChunkPool = VoxelProvider<B>.Current.ChunkPool(transform);
            BuildCoordinator = VoxelProvider<B>.Current.MeshBuildCoordinator(ChunkPool);
            NoiseProfile = VoxelProvider<B>.Current.NoiseProfile();

            WorldAwake();
        }

        private void Start() {
            NoiseProfile.Generate(_settings);
            
            Debug.unityLogger.Log(TAG,"Height Map Generated");
            
            for (int x = -_chunkSettings.ChunkPageSize; x < _chunkSettings.ChunkPageSize; x++) {
                for (int z = -_chunkSettings.ChunkPageSize; z < _chunkSettings.ChunkPageSize; z++) {
                    for (int y = -_chunkSettings.ChunkPageSize; y < _chunkSettings.ChunkPageSize; y++) {
                        var pos = new Vector3Int(x, y, z) * _chunkSettings.ChunkSize;
                        var chunk = VoxelProvider<B>.Current.CreateChunk(pos);
                        NoiseProfile.Fill(chunk);
                        Chunks.Add(pos, chunk);
                    }
                }
            }
            
            NoiseProfile.Clear();
            
            FocusChunkCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            
            Debug.unityLogger.Log(TAG,"Chunks Created : " + Chunks.Count);
            
            WorldStart();
        }

        private void Update() {
            var coords = _focus != null
                ? GetChunkCoords(_focus.position)
                : Vector3Int.zero;
            
            WorldUpdate();

            if (coords.x == FocusChunkCoord.x && coords.z == FocusChunkCoord.z) return;

            FocusChunkCoord = coords;
                
            // chunk update
            ChunkPoolUpdate();
        }
        
        #endregion

        #region Neighbors
        public Chunk<B> GetNeighborPX(Chunk<B> chunk) {
            var px = chunk.Position + Vector3Int.right * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(px) ? Chunks[px] : null;
        }
        
        public Chunk<B> GetNeighborPY(Chunk<B> chunk) {
            var py = chunk.Position + Vector3Int.up * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(py) ? Chunks[py] : null;
        }
        
        public Chunk<B> GetNeighborPZ(Chunk<B> chunk) {
            var pz = chunk.Position + new Vector3Int(0, 0, 1) * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(pz) ? Chunks[pz] : null;
        }
        
        public Chunk<B> GetNeighborNX(Chunk<B> chunk) {
            var nx = chunk.Position + Vector3Int.left * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(nx) ? Chunks[nx] : null;
        }
        
        public Chunk<B> GetNeighborNY(Chunk<B> chunk) {
            var ny = chunk.Position + Vector3Int.down * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(ny) ? Chunks[ny] : null;
        }
        
        public Chunk<B> GetNeighborNZ(Chunk<B> chunk) {
            var nz = chunk.Position + new Vector3Int(0, 0, -1) * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(nz) ? Chunks[nz] : null;
        }
        
        #endregion

        #region Utils

        public Vector3Int GetChunkCoords(Vector3 Position) {
            var pos = Vector3Int.FloorToInt(Position);

            var x = pos.x - pos.x % _chunkSettings.ChunkSize.x;
            var y = pos.y - pos.y % _chunkSettings.ChunkSize.y;
            var z = pos.z - pos.z % _chunkSettings.ChunkSize.z;

            x = pos.x < 0 ? x - _chunkSettings.ChunkSize.x : x;
            y = pos.y < 0 ? y - _chunkSettings.ChunkSize.y : y;
            z = pos.z < 0 ? z - _chunkSettings.ChunkSize.z : z;
            
            return new Vector3Int(x,y,z);
        }
        
        public Vector3Int GetChunkCoords(Vector3Int Position) {
            var x = Position.x - Position.x % _chunkSettings.ChunkSize.x;
            var y = Position.y - Position.y % _chunkSettings.ChunkSize.y;
            var z = Position.z - Position.z % _chunkSettings.ChunkSize.z;
            
            return new Vector3Int(x,y,z);
        }

        #endregion

        #region Private
        private void ChunkPoolUpdate() {
            var jobs = ChunkPool
                .Update(FocusChunkCoord)
                .FindAll(coord => Chunks.ContainsKey(coord))
                .Select(coord => GetChunkJobData(Chunks[coord]))
                .ToList();

            BuildCoordinator.Process(jobs);

            WorldChunkPoolUpdate();
        }
        
        private ChunkJobData<B> GetChunkJobData(Chunk<B> chunk) {
            var position = chunk.Position;

            var px = position + Vector3Int.right * _chunkSettings.ChunkSize;
            var py = position + Vector3Int.up * _chunkSettings.ChunkSize;
            var pz = position + new Vector3Int(0, 0, 1) * _chunkSettings.ChunkSize;
            var nx = position + Vector3Int.left * _chunkSettings.ChunkSize;
            var ny = position + Vector3Int.down * _chunkSettings.ChunkSize;
            var nz = position + new Vector3Int(0, 0, -1) * _chunkSettings.ChunkSize;
            
            return new ChunkJobData<B> {
                Chunk = chunk,
                ChunkPX = Chunks.ContainsKey(px) ? Chunks[px] : null,
                ChunkPY = Chunks.ContainsKey(py) ? Chunks[py] : null,
                ChunkPZ = Chunks.ContainsKey(pz) ? Chunks[pz] : null,
                ChunkNX = Chunks.ContainsKey(nx) ? Chunks[nx] : null,
                ChunkNY = Chunks.ContainsKey(ny) ? Chunks[ny] : null,
                ChunkNZ = Chunks.ContainsKey(nz) ? Chunks[nz] : null
            };
        }
        #endregion

    }

}