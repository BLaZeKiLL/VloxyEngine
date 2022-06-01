using System.Diagnostics;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Page;
using CodeBlaze.Vloxy.Engine.Jobs.Mesh;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.World {

    public class VloxyWorld : MonoBehaviour {

        [SerializeField] private Transform _Focus;
        [SerializeField] private VloxySettings _Settings;

        protected int3 FocusChunkCoord;

        protected ChunkState ChunkState;
        protected ChunkStore ChunkStore;
        protected NoiseProfile NoiseProfile;
        protected ChunkBehaviourPool ChunkBehaviourPool;
        protected MeshBuildScheduler MeshBuildScheduler;
        protected ChunkPageScheduler ChunkPageScheduler;
        protected BurstFunctionPointers BurstFunctionPointers;

        private bool _IsFocused;

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
                provider.Settings = _Settings;
#if VLOXY_LOGGING
                VloxyLogger.Info<VloxyWorld>("Provider Initialized");
#endif
                WorldInitialize();
            });

            ConstructVloxyComponents();
            
            FocusChunkCoord = new int3(1,1,1) * int.MinValue;

            WorldAwake();
        }

        private void Start() {
            _IsFocused = _Focus != null;
            
            GenerateWorld();

            WorldStart();
        }

        private void Update() {
            var NewFocusChunkCoord = _IsFocused ? VloxyUtils.GetChunkCoords(_Focus.position) : int3.zero;

            if (!(NewFocusChunkCoord == FocusChunkCoord).AndReduce() && MeshBuildScheduler.CanSchedule()) {
                ViewRegionUpdate(NewFocusChunkCoord);
            }

            WorldUpdate();

            // ChunkStore.ActiveChunkUpdate();
            
            MeshBuildScheduler.Complete();
        }

        private void OnDestroy() {
            ChunkStore.Dispose();
            MeshBuildScheduler.Dispose();
        }
        
        #endregion

        private void ConstructVloxyComponents() {
            ChunkState = VloxyProvider.Current.ChunkState();
            NoiseProfile = VloxyProvider.Current.NoiseProfile();
            ChunkBehaviourPool = VloxyProvider.Current.ChunkPool(transform);
            BurstFunctionPointers = VloxyProvider.Current.SetupBurstFunctionPointers();
            MeshBuildScheduler = VloxyProvider.Current.MeshBuildScheduler(ChunkState, ChunkBehaviourPool, BurstFunctionPointers);
            ChunkPageScheduler = VloxyProvider.Current.ChunkDataScheduler(NoiseProfile, BurstFunctionPointers);
            ChunkStore = VloxyProvider.Current.ChunkStore(ChunkState, ChunkPageScheduler);

#if VLOXY_LOGGING
            VloxyLogger.Info<VloxyWorld>("Vloxy Components Constructed");
#endif
        }

        private void GenerateWorld() {
#if VLOXY_LOGGING
            var watch = new Stopwatch();
            watch.Start();
#endif
            ChunkState.Initialize(int3.zero);
            ChunkStore.GenerateChunks();
#if VLOXY_LOGGING
            watch.Stop();
            VloxyLogger.Info<VloxyWorld>($"Vloxy World Generated : {watch.ElapsedMilliseconds} MS");
#endif
        }

        private void ViewRegionUpdate(int3 NewFocusChunkCoord) {
            var (claim, reclaim) = ChunkStore.ViewRegionUpdate(NewFocusChunkCoord, FocusChunkCoord);
            
            if (claim.Count != 0) MeshBuildScheduler.Schedule(claim, ChunkStore.Accessor);

            for (var index = 0; index < reclaim.Count; index++) {
                ChunkBehaviourPool.Reclaim(reclaim[index]);
            }

            WorldViewRegionUpdate();
            
            FocusChunkCoord = NewFocusChunkCoord;
        }

    }

}