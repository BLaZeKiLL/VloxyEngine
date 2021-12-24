using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Scheduler;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Mathematics;


using UnityEngine;

#if VLOXY_PROFILING
using CodeBlaze.Vloxy.Profiling;
#endif

namespace CodeBlaze.Vloxy.Engine.World {

    public class VloxyWorld : MonoBehaviour {

        [SerializeField] private Transform _focus;
        [SerializeField] private VloxySettings _settings;

        protected ChunkBehaviourPool ChunkBehaviourPool;
        protected MeshBuildScheduler Scheduler;
        protected ChunkStore ChunkStore;
        protected int3 FocusChunkCoord;
        
        public INoiseProfile NoiseProfile;

        private BurstFunctionPointers BurstFunctionPointers;

        #region Virtual

        protected virtual VloxyProvider Provider() => new();
        protected virtual void WorldInitialize() { }
        protected virtual void WorldAwake() { }
        protected virtual void WorldStart() { }
        protected virtual void WorldUpdate() { }
        protected virtual void WorldViewRegionUpdate() { }

        #endregion

        #region Unity

        private void Awake() {
            VloxyProvider.Initialize(Provider(), provider => {
                provider.Settings = _settings;
#if VLOXY_LOGGING
                VloxyLogger.Info<VloxyWorld>("Provider Initialized");
#endif
                WorldInitialize();
            });

            ConstructVloxyComponents();
            
            WorldAwake();
        }

        private void ConstructVloxyComponents() {
            BurstFunctionPointers = VloxyProvider.Current.SetupBurstFunctionPointers();
            NoiseProfile = VloxyProvider.Current.NoiseProfile();
            ChunkBehaviourPool = VloxyProvider.Current.ChunkPool(transform);
            Scheduler = VloxyProvider.Current.MeshBuildScheduler(ChunkBehaviourPool, BurstFunctionPointers);
            ChunkStore = VloxyProvider.Current.ChunkStore(NoiseProfile);
            
#if VLOXY_PROFILING
            VloxyProfiler.Initialize();
#endif
#if VLOXY_LOGGING
            VloxyLogger.Info<VloxyWorld>("Vloxy Components Constructed");
#endif
        }

        private void Start() {
            NoiseProfile.GenerateHeightMap();

            ChunkStore.GenerateChunks();

            NoiseProfile.Dispose();
            
            FocusChunkCoord = new int3(1,1,1) * int.MinValue;

            WorldStart();
        }

        private void Update() {
            var NewFocusChunkCoord = _focus != null ? VloxyUtils.GetChunkCoords(_focus.position) : int3.zero;

            if (!(NewFocusChunkCoord == FocusChunkCoord).AndReduce()) ViewRegionUpdate(NewFocusChunkCoord);

            WorldUpdate();

            // ChunkStore.ActiveChunkUpdate();
        }

        private void LateUpdate() {
            Scheduler.Complete();
        }

        private void OnDestroy() {
            ChunkStore.Dispose();
            Scheduler.Dispose();

#if VLOXY_PROFILING
            VloxyProfiler.Dispose();
#endif
        }

        private void ViewRegionUpdate(int3 NewFocusChunkCoord) {
#if VLOXY_PROFILING
            VloxyProfiler.ViewRegionUpdateMarker.Begin();
#endif
            var (claim, reclaim) = ChunkStore.ViewRegionUpdate(NewFocusChunkCoord, FocusChunkCoord);

            if (claim.Count != 0) Scheduler.Schedule(claim, ChunkStore.Accessor);

            for (var index = 0; index < reclaim.Count; index++) {
                ChunkBehaviourPool.Reclaim(reclaim[index]);
            }

            WorldViewRegionUpdate();

            FocusChunkCoord = NewFocusChunkCoord;
            
#if VLOXY_PROFILING
            VloxyProfiler.ViewRegionUpdateMarker.End();
#endif
        }

        #endregion

    }

}