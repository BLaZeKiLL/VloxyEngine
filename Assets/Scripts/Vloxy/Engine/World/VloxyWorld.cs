using System.Diagnostics;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs;
using CodeBlaze.Vloxy.Engine.Jobs.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Mesh;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.World {

    public class VloxyWorld : MonoBehaviour {

        [SerializeField] private Transform _Focus;
        [SerializeField] private VloxySettings _Settings;

        protected int3 FocusChunkCoord;

        protected NoiseProfile NoiseProfile;
        protected ChunkManager ChunkManager;

        private VloxyScheduler VloxyScheduler;
        private ChunkBehaviourPool ChunkBehaviourPool;
        private MeshBuildScheduler MeshBuildScheduler;
        private ChunkDataScheduler _ChunkDataScheduler;
        private BurstFunctionPointers BurstFunctionPointers;

        private bool _IsFocused;

        #region Virtual

        protected virtual VloxyProvider Provider() => new();
        protected virtual void WorldInitialize() { }
        protected virtual void WorldAwake() { }
        protected virtual void WorldStart() { }
        protected virtual void WorldUpdate() { }
        protected virtual void WorldRegionUpdate() { }

        #endregion

        #region Unity

        private void Awake() {
            VloxyProvider.Initialize(Provider(), provider => {
                ConfigureSettings();
                
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

            if (!(NewFocusChunkCoord == FocusChunkCoord).AndReduce()) {
                ViewRegionUpdate(NewFocusChunkCoord);
                ChunkRegionUpdate(NewFocusChunkCoord);
                
                WorldRegionUpdate();

                FocusChunkCoord = NewFocusChunkCoord;
            }
            
            VloxyScheduler.Update();

            WorldUpdate();

            // ChunkStore.ActiveChunkUpdate();
        }

        private void LateUpdate() {
            VloxyScheduler.LateUpdate();
        }

        private void OnDestroy() {
            VloxyScheduler.Dispose();
            ChunkManager.Dispose();
        }
        
        #endregion

        private void ConfigureSettings() {
            _Settings.Chunk.LoadDistance = _Settings.Chunk.DrawDistance + 2;
            _Settings.Chunk.HeightSize = _Settings.Noise.Height / _Settings.Chunk.ChunkSize.y / 2;

            _Settings.Scheduler.MeshingBatchSize = 2 * _Settings.Chunk.DrawDistance + 1;
            _Settings.Scheduler.StreamingBatchSize = 2 * _Settings.Chunk.LoadDistance + 1;
        }
        
        private void ConstructVloxyComponents() {
            NoiseProfile = VloxyProvider.Current.NoiseProfile();
            ChunkManager = VloxyProvider.Current.ChunkManager();

            ChunkBehaviourPool = VloxyProvider.Current.ChunkPool(transform);
            BurstFunctionPointers = VloxyProvider.Current.SetupBurstFunctionPointers();

            MeshBuildScheduler = VloxyProvider.Current.MeshBuildScheduler(
                ChunkManager.State, 
                ChunkManager.Accessor, 
                ChunkBehaviourPool, 
                BurstFunctionPointers
            );
            
            _ChunkDataScheduler = VloxyProvider.Current.ChunkDataScheduler(
                ChunkManager.State,
                ChunkManager.Store,
                NoiseProfile, 
                BurstFunctionPointers
            );

            VloxyScheduler = VloxyProvider.Current.VloxyScheduler(MeshBuildScheduler, _ChunkDataScheduler);
            
#if VLOXY_LOGGING
            VloxyLogger.Info<VloxyWorld>("Vloxy Components Constructed");
#endif
        }

        private void GenerateWorld() {
#if VLOXY_LOGGING
            var watch = new Stopwatch();
            watch.Start();
#endif
            _ChunkDataScheduler.GenerateChunks(ChunkManager.InitialChunkRegion(Allocator.TempJob));
            
#if VLOXY_LOGGING
            watch.Stop();
            VloxyLogger.Info<VloxyWorld>($"Initial Chunks Generated : {ChunkManager.Store.ChunkCount()}");
            VloxyLogger.Info<VloxyWorld>($"Vloxy World Generated : {watch.ElapsedMilliseconds} MS");
#endif
        }

        private void ViewRegionUpdate(int3 NewFocusChunkCoord) {
            var (claim, reclaim) = ChunkManager.ViewRegionUpdate(NewFocusChunkCoord, FocusChunkCoord);
            
            if (claim.Count != 0) MeshBuildScheduler.Schedule(claim);

            for (var index = 0; index < reclaim.Count; index++) {
                ChunkBehaviourPool.Reclaim(reclaim[index]);
            }
        }

        private void ChunkRegionUpdate(int3 NewFocusChunkCoord) {
            var (claim, reclaim) = ChunkManager.ChunkRegionUpdate(NewFocusChunkCoord, FocusChunkCoord);
            
            if (claim == null || reclaim == null) return;
            
            if (claim.Count != 0) _ChunkDataScheduler.Schedule(claim);
            
            for (var index = 0; index < reclaim.Count; index++) {
                ChunkManager.State.RemoveState(reclaim[index]);
                ChunkManager.Store.RemoveChunk(reclaim[index]);
            }
            
        }

    }

}