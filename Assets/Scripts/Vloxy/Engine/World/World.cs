using CodeBlaze.Vloxy.Engine.Components;
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

        protected ChunkBehaviourPool<B> ChunkBehaviourPool;
        protected MeshBuildCoordinator<B> BuildCoordinator;
        protected INoiseProfile<B> NoiseProfile;
        
        protected ChunkStore<B> ChunkStore;
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
            ChunkBehaviourPool = VoxelProvider<B>.Current.ChunkPool(transform);
            BuildCoordinator = VoxelProvider<B>.Current.MeshBuildCoordinator(ChunkBehaviourPool);
            NoiseProfile = VoxelProvider<B>.Current.NoiseProfile();
            ChunkStore = VoxelProvider<B>.Current.ChunkStore(NoiseProfile);
            
            Debug.unityLogger.Log(TAG,"Components Constructed");

            WorldAwake();
        }

        private void Start() {
            NoiseProfile.Generate();

            ChunkStore.GenerateChunks();

            NoiseProfile.Clear();
            
            FocusChunkCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

            WorldStart();
        }

        private void Update() {
            var coords = _focus != null
                ? GetChunkCoords(_focus.position)
                : Vector3Int.zero;
            
            // WorldUpdate();

            if (coords.x == FocusChunkCoord.x && coords.z == FocusChunkCoord.z) return;

            FocusChunkCoord = coords;
            
            // ChunkPoolUpdate();
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
            // var jobs = ChunkBehaviourPool
            //     .Update(FocusChunkCoord)
            //     .FindAll(coord => ChunkStore.ContainsChunk(coord))
            //     .Select(coord => GetChunkJobData(Chunks[coord]))
            //     .ToList();
            //
            // BuildCoordinator.Process(jobs);

            WorldChunkPoolUpdate();
        }
        
        // private MeshBuildJobData<B> GetChunkJobData(Chunk<B> chunk) {
        //     var position = chunk.Position;
        //
        //     var px = position + Vector3Int.right * _chunkSettings.ChunkSize;
        //     var py = position + Vector3Int.up * _chunkSettings.ChunkSize;
        //     var pz = position + new Vector3Int(0, 0, 1) * _chunkSettings.ChunkSize;
        //     var nx = position + Vector3Int.left * _chunkSettings.ChunkSize;
        //     var ny = position + Vector3Int.down * _chunkSettings.ChunkSize;
        //     var nz = position + new Vector3Int(0, 0, -1) * _chunkSettings.ChunkSize;
        //     
        //     return new MeshBuildJobData<B> {
        //         Chunk = chunk,
        //         ChunkPX = Chunks.ContainsKey(px) ? Chunks[px] : null,
        //         ChunkPY = Chunks.ContainsKey(py) ? Chunks[py] : null,
        //         ChunkPZ = Chunks.ContainsKey(pz) ? Chunks[pz] : null,
        //         ChunkNX = Chunks.ContainsKey(nx) ? Chunks[nx] : null,
        //         ChunkNY = Chunks.ContainsKey(ny) ? Chunks[ny] : null,
        //         ChunkNZ = Chunks.ContainsKey(nz) ? Chunks[nz] : null
        //     };
        // }
        #endregion

    }

}