using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Jobs;
using CodeBlaze.Vloxy.Engine.Jobs.Chunk;
using CodeBlaze.Vloxy.Engine.Jobs.Collider;
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
        
        #region API
        public Transform Focus => _Focus;
        public VloxySettings Settings => _Settings;
        public int3 FocusChunkCoord { get; private set; }
        public VloxyScheduler Scheduler { get; private set; }
        public NoiseProfile NoiseProfile { get; private set; }
        public ChunkManager ChunkManager { get; private set; }

        #endregion
        
        private ChunkPool _ChunkPool;
        private MeshBuildScheduler _MeshBuildScheduler;
        private ChunkDataScheduler _ChunkDataScheduler;
        private ColliderBuildScheduler _ColliderBuildScheduler;

        private bool _IsFocused;

        private byte _UpdateFrame = 1;

        #region Virtual

        protected virtual VloxyProvider Provider() => new();
        protected virtual void WorldConfigure() { }
        protected virtual void WorldInitialize() { }
        protected virtual void WorldAwake() { }
        protected virtual void WorldStart() { }
        protected virtual void WorldUpdate() { }
        protected virtual void WorldFocusUpdate() { }
        protected virtual void WorldSchedulerUpdate() { }
        protected virtual void WorldLateUpdate() {}

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

            WorldStart();
        }

        private void Update() {
            var NewFocusChunkCoord = _IsFocused ? VloxyUtils.GetChunkCoords(_Focus.position) : int3.zero;

            if (!(NewFocusChunkCoord == FocusChunkCoord).AndReduce()) {
                FocusChunkCoord = NewFocusChunkCoord;
                
                WorldFocusUpdate();
            }
            
            // We can change this, so that update happens only when required
            Scheduler.FocusUpdate(FocusChunkCoord);

            // Schedule every 'x' frames (throttling)
            if (_UpdateFrame % Settings.Scheduler.TickRate == 0) {
                _UpdateFrame = 1;

                Scheduler.SchedulerUpdate();

                WorldSchedulerUpdate();
            } else {
                _UpdateFrame++;
            }

            WorldUpdate();
        }

        private void LateUpdate() {
            Scheduler.SchedulerLateUpdate();

            WorldLateUpdate();
        }

        private void OnDestroy() {
            Scheduler.Dispose();
            ChunkManager.Dispose();
        }
        
        #endregion

        private void ConfigureSettings() {
            Settings.Chunk.LoadDistance = Settings.Chunk.DrawDistance + 2;
            Settings.Chunk.UpdateDistance = Settings.Chunk.DrawDistance; // can reduce this by 2 maybe

            // TODO : Should these be dynamic or manual ?
            Settings.Scheduler.MeshingBatchSize = 16;
            Settings.Scheduler.StreamingBatchSize = 24;
            Settings.Scheduler.ColliderBatchSize = 32;

            WorldConfigure();
        }
        
        private void ConstructVloxyComponents() {
            NoiseProfile = VloxyProvider.Current.NoiseProfile();
            ChunkManager = VloxyProvider.Current.ChunkManager();

            _ChunkPool = VloxyProvider.Current.ChunkPoolV2(transform);

            _MeshBuildScheduler = VloxyProvider.Current.MeshBuildSchedulerV2(
                ChunkManager.Store, 
                _ChunkPool
            );
            
            _ChunkDataScheduler = VloxyProvider.Current.ChunkDataSchedulerV2(
                ChunkManager.Store,
                NoiseProfile
            );

            _ColliderBuildScheduler = VloxyProvider.Current.ColliderBuildScheduler(
                _ChunkPool
            );

            Scheduler = VloxyProvider.Current.VloxySchedulerV2(
                _MeshBuildScheduler, 
                _ChunkDataScheduler,
                _ColliderBuildScheduler,
                ChunkManager.Store,
                _ChunkPool
            );

#if VLOXY_LOGGING
            VloxyLogger.Info<VloxyWorld>("Vloxy Components Constructed");
#endif
        }

    }

}