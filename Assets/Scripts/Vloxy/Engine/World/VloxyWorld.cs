using System;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Scheduler;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Mathematics;

using UnityEditor;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.World {

    public class VloxyWorld : MonoBehaviour {

        [SerializeField] private Transform _focus;
        [SerializeField] private VoxelSettings _settings;

        protected ChunkBehaviourPool ChunkBehaviourPool;
        protected MeshBuildScheduler Scheduler;
        protected INoiseProfile NoiseProfile;
        protected ChunkStore ChunkStore;
        protected int3 FocusChunkCoord;

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
                VloxyLogger.Info<VloxyWorld>("Provider Initialized");
                WorldInitialize();
            });

            ConstructVloxyComponents();

            WorldAwake();
        }

        private void ConstructVloxyComponents() {
            NoiseProfile = VloxyProvider.Current.NoiseProfile();
            ChunkBehaviourPool = VloxyProvider.Current.ChunkPool(transform);
            Scheduler = VloxyProvider.Current.MeshBuildScheduler(ChunkBehaviourPool);
            ChunkStore = VloxyProvider.Current.ChunkStore(NoiseProfile);
            
            VloxyLogger.Info<VloxyWorld>("Vloxy Components Constructed");
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
        }

        /// <summary>
        /// Draws the height Map
        /// </summary>
        private void OnDrawGizmosSelected() {
            var heights = ((FastNoiseProfile2D)NoiseProfile).GetHeightMap();
            var style = new GUIStyle {normal = {textColor = Color.magenta}};
            
            foreach (var height in heights) {
                Handles.Label(height + new Vector3(0.5f, 0f, 0.5f), $"{height.y}", style);
            }
        }

        private void ViewRegionUpdate(int3 NewFocusChunkCoord) {
            var (claim, reclaim) = ChunkStore.ViewRegionUpdate(NewFocusChunkCoord, FocusChunkCoord);

            if (claim.Length != 0) Scheduler.Schedule(claim, ChunkStore.Accessor);

            for (var index = 0; index < reclaim.Count; index++) {
                ChunkBehaviourPool.Reclaim(reclaim[index]);
            }

            WorldViewRegionUpdate();

            FocusChunkCoord = NewFocusChunkCoord;
        }

        #endregion

    }

}