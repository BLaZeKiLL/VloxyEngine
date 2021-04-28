using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.World {

    public class World<B> : MonoBehaviour where B : IBlock {

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
        
        protected virtual void WorldInitialize() { }
        protected virtual void WorldAwake() { }
        protected virtual void WorldStart() { }
        protected virtual void WorldUpdate() { }
        protected virtual void WorldViewRegionUpdate() { }

        #endregion

        #region Unity

        private void Awake() {
            VoxelProvider<B>.Initialize(Provider(), provider => {
                provider.Settings = _settings;
                CBSL.Logging.Logger.Info<World<B>>("Provider Initialized");
                WorldInitialize();
            });

            _chunkSettings = VoxelProvider<B>.Current.Settings.Chunk;
            
            ConstructVloxyComponents();

            WorldAwake();
        }

        private void ConstructVloxyComponents() {
            NoiseProfile = VoxelProvider<B>.Current.NoiseProfile();
            ChunkBehaviourPool = VoxelProvider<B>.Current.ChunkPool(transform);
            BuildCoordinator = VoxelProvider<B>.Current.MeshBuildCoordinator(ChunkBehaviourPool);
            ChunkStore = VoxelProvider<B>.Current.ChunkStore(NoiseProfile);
            
            CBSL.Logging.Logger.Info<World<B>>("Vloxy Components Constructed");
        }

        private void Start() {
            NoiseProfile.GenerateHeightMap();

            ChunkStore.GenerateChunks();

            NoiseProfile.Clear();
            
            FocusChunkCoord = Vector3Int.one * int.MinValue;

            WorldStart();
        }

        private void Update() {
            var NewFocusChunkCoord = _focus != null ? VloxyUtils<B>.GetChunkCoords(_focus.position) : Vector3Int.zero;
            
            WorldUpdate();

            ChunkStore.ActiveChunkUpdate();

            if (NewFocusChunkCoord.x == FocusChunkCoord.x && NewFocusChunkCoord.y == FocusChunkCoord.y && NewFocusChunkCoord.z == FocusChunkCoord.z) return;

            ViewRegionUpdate(NewFocusChunkCoord);
        }

        private void ViewRegionUpdate(Vector3Int NewFocusChunkCoord) {
            var (claim, reclaim) = ChunkStore.ViewRegionUpdate(NewFocusChunkCoord, FocusChunkCoord);

            if (claim.Count != 0) BuildCoordinator.Schedule(claim);

            reclaim.ForEach(x => ChunkBehaviourPool.Reclaim(x));

            WorldViewRegionUpdate();

            FocusChunkCoord = NewFocusChunkCoord;
        }

        #endregion

    }

}