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
        [SerializeField] protected VloxySettings Settings;

        protected int3 FocusChunkCoord;

        protected NoiseProfile NoiseProfile;
        protected ChunkManager ChunkManager;

        private BurstFunctionPointers BurstFunctionPointers;
        
        private ChunkPoolV2 ChunkPoolV2;
        private VloxySchedulerV2 VloxySchedulerV2;
        private MeshBuildSchedulerV2 MeshBuildSchedulerV2;
        private ChunkDataSchedulerV2 ChunkDataSchedulerV2;

        private bool _IsFocused;

        private byte _UpdateFrame = 1;

        #region Virtual

        protected virtual VloxyProvider Provider() => new();
        protected virtual void WorldConfigure() { }
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
                
                provider.Settings = Settings;
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
            if (_UpdateFrame % 16 == 0) {
                _UpdateFrame = 1;
                
                var NewFocusChunkCoord = _IsFocused ? VloxyUtils.GetChunkCoords(_Focus.position) : int3.zero;

                if (!(NewFocusChunkCoord == FocusChunkCoord).AndReduce()) {
                    VloxySchedulerV2.FocusUpdate(NewFocusChunkCoord);
                    
                    WorldRegionUpdate();

                    FocusChunkCoord = NewFocusChunkCoord;
                }
            
                VloxySchedulerV2.SchedulerUpdate();

                WorldUpdate();
            } else {
                _UpdateFrame++;
            }

            // ChunkStore.ActiveChunkUpdate();
        }

        private void LateUpdate() {
            VloxySchedulerV2.SchedulerLateUpdate();
        }

        private void OnDestroy() {
            VloxySchedulerV2.Dispose();
            ChunkManager.Dispose();
        }
        
        #endregion

        private void ConfigureSettings() {
            Settings.Chunk.LoadDistance = Settings.Chunk.DrawDistance * 2;

            Settings.Scheduler.MeshingBatchSize = 16;
            Settings.Scheduler.StreamingBatchSize = 32;

            WorldConfigure();
        }
        
        private void ConstructVloxyComponents() {
            NoiseProfile = VloxyProvider.Current.NoiseProfile();
            ChunkManager = VloxyProvider.Current.ChunkManager();

            ChunkPoolV2 = VloxyProvider.Current.ChunkPoolV2(transform);
            BurstFunctionPointers = VloxyProvider.Current.SetupBurstFunctionPointers();

            MeshBuildSchedulerV2 = VloxyProvider.Current.MeshBuildSchedulerV2(
                ChunkManager.Accessor, 
                ChunkPoolV2, 
                BurstFunctionPointers
            );
            
            ChunkDataSchedulerV2 = VloxyProvider.Current.ChunkDataSchedulerV2(
                ChunkManager.Store,
                NoiseProfile, 
                BurstFunctionPointers
            );

            VloxySchedulerV2 = VloxyProvider.Current.VloxySchedulerV2(
                MeshBuildSchedulerV2, 
                ChunkDataSchedulerV2,
                ChunkManager.Store,
                ChunkPoolV2
            );

#if VLOXY_LOGGING
            VloxyLogger.Info<VloxyWorld>("Vloxy Components Constructed");
#endif
        }

        private void GenerateWorld() {
#if VLOXY_LOGGING
            var watch = new Stopwatch();
            watch.Start();
#endif
            ChunkDataSchedulerV2.GenerateChunks(ChunkManager.InitialChunkRegion(Allocator.TempJob));
            
#if VLOXY_LOGGING
            watch.Stop();
            VloxyLogger.Info<VloxyWorld>($"Initial Chunks Generated : {ChunkManager.Store.ChunkCount()}");
            VloxyLogger.Info<VloxyWorld>($"Vloxy World Generated : {watch.ElapsedMilliseconds} MS");
#endif
        }

    }

}