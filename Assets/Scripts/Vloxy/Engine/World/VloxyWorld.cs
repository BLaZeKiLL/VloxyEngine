using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Schedular;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.World {

    public class VloxyWorld : MonoBehaviour {

        [SerializeField] private Transform _focus;
        [SerializeField] private VoxelSettings _settings;

        protected ChunkBehaviourPool ChunkBehaviourPool;
        protected MeshBuildSchedular Schedular;
        protected INoiseProfile NoiseProfile;
        protected NativeChunkStore ChunkStore;
        protected int3 FocusChunkCoord;

        private ChunkSettings _chunkSettings;

        #region Virtual

        protected virtual VoxelProvider Provider() => new();
        
        protected virtual void WorldInitialize() { }
        protected virtual void WorldAwake() { }
        protected virtual void WorldStart() { }
        protected virtual void WorldUpdate() { }
        protected virtual void WorldViewRegionUpdate() { }

        #endregion

        #region Unity

        private void Awake() {
            VoxelProvider.Initialize(Provider(), provider => {
                provider.Settings = _settings;
                CBSL.Logging.Logger.Info<VloxyWorld>("Provider Initialized");
                WorldInitialize();
            });

            _chunkSettings = VoxelProvider.Current.Settings.Chunk;
            
            ConstructVloxyComponents();

            WorldAwake();
        }

        private void ConstructVloxyComponents() {
            NoiseProfile = VoxelProvider.Current.NoiseProfile();
            ChunkBehaviourPool = VoxelProvider.Current.ChunkPool(transform);
            Schedular = VoxelProvider.Current.MeshBuildSchedular(ChunkBehaviourPool);
            ChunkStore = VoxelProvider.Current.ChunkStore(NoiseProfile);
            
            CBSL.Logging.Logger.Info<VloxyWorld>("Vloxy Components Constructed");
        }

        private void Start() {
            NoiseProfile.GenerateHeightMap();

            ChunkStore.GenerateChunks();

            NoiseProfile.Clear();
            
            FocusChunkCoord = new int3(1,1,1) * int.MinValue;

            WorldStart();
        }

        private void Update() {
            var NewFocusChunkCoord = _focus != null ? VloxyUtils.GetChunkCoords(_focus.position) : int3.zero;

            if (NewFocusChunkCoord.x == FocusChunkCoord.x && NewFocusChunkCoord.y == FocusChunkCoord.y && NewFocusChunkCoord.z == FocusChunkCoord.z) return;

            ViewRegionUpdate(NewFocusChunkCoord);
            
            WorldUpdate();

            // ChunkStore.ActiveChunkUpdate();
        }

        private void LateUpdate() {
            Schedular.Complete();
        }

        private void OnDestroy() {
            ChunkStore.Dispose();
            Schedular.Dispose();
        }

        private void ViewRegionUpdate(int3 NewFocusChunkCoord) {
            var (claim, reclaim) = ChunkStore.ViewRegionUpdate(NewFocusChunkCoord, FocusChunkCoord);

            if (claim.Length != 0) Schedular.Schedule(claim, ChunkStore.Accessor);

            reclaim.ForEach(pos => ChunkBehaviourPool.Reclaim(pos));

            WorldViewRegionUpdate();

            FocusChunkCoord = NewFocusChunkCoord;
        }

        #endregion

    }

}