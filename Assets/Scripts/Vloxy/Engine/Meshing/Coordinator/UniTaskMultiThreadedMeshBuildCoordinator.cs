﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public class UniTaskMultiThreadedMeshBuildCoordinator<B> : MeshBuildCoordinator<B> where B : IBlock {
        
        private const string TAG = "<color=green>MeshBuildCoordinator</color>";
        
        private int _batchSize;

        public UniTaskMultiThreadedMeshBuildCoordinator(ChunkBehaviourPool<B> chunkBehaviourPool, int batchSize) : base(chunkBehaviourPool) {
            _batchSize = batchSize;
        }
        
        public override void Process(List<MeshBuildJobData<B>> jobs) => InternalProcess(jobs).Forget();

        protected override void Render(Chunk<B> chunk, MeshData meshData) {
            ChunkBehaviourPool.Claim(chunk).Render(meshData);
        }

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private async UniTaskVoid InternalProcess(List<MeshBuildJobData<B>> jobs) {
            PreProcess(jobs);
            
            var watch = new Stopwatch();
            
            watch.Start();
            var result = await UniTask.WhenAll(CreateBatches(jobs).Select(ScheduleJob).ToList());
            watch.Stop();

            if (result.Length > 0) {
                UnityEngine.Debug.unityLogger.Log(TAG,$"Number of batches : {result.Length}");
                UnityEngine.Debug.unityLogger.Log(TAG,$"Average batch process time : {result.Average():0.###} ms");
                UnityEngine.Debug.unityLogger.Log(TAG,$"Total batch process time : {watch.Elapsed.TotalMilliseconds:0.###} ms");
            }
            
            PostProcess(jobs);
        }
        
        private async UniTask<long> ScheduleJob(Batch batch) {
            var watch = new Stopwatch();
            
            var	result = await UniTask.RunOnThreadPool(
                () => {
                    watch.Start();
                    var _meshData = batch.Process();
                    watch.Stop();

                    return _meshData;
                }
            );

            for (var index = 0; index < result.Length; index++) {
                Render(batch.GetChunk(index), result[index]);
            }

            return watch.ElapsedMilliseconds;
        }  
        #else
        public async UniTaskVoid InternalProcess(List<MeshBuildJobData<B>> jobs) {
            PreProcess(jobs);
            
            await UniTask.WhenAll(CreateBatches(jobs).Select(ScheduleJob).ToList());
            
            PostProcess(jobs);
        }
        
        private async UniTask ScheduleJob(Batch batch) {
            var	result = await UniTask.RunOnThreadPool(
                () => {
                    var _meshData = batch.Process();

                    return _meshData;
                }
            );

            for (var index = 0; index < result.Length; index++) {
                Render(batch.GetChunk(index), result[index]);
            }
        }  
        #endif

        private IEnumerable<Batch> CreateBatches(List<MeshBuildJobData<B>> jobs) {
            var batches = new Batch[Mathf.CeilToInt((float) jobs.Count / _batchSize)];
            var bindex = 0;
            for (int i = 0; i < jobs.Count; i += _batchSize) {
                batches[bindex++] = new Batch(jobs.GetRange(i, Math.Min(_batchSize, jobs.Count - i)));
            }

            return batches;
        }

        private class Batch {
            
            private List<MeshBuildJobData<B>> _data;

            public Batch(List<MeshBuildJobData<B>> data) {
                _data = data;
            }

            public Chunk<B> GetChunk(int index) => _data[index].Chunk;
            
            public MeshData[] Process() {
                var result = new MeshData[_data.Count];

                for (int i = 0; i < _data.Count; i++) {
                    result[i] = VoxelProvider<B>.Current.MeshBuilder().GenerateMesh(_data[i]);
                }

                return result;
            }

        }

    }

}