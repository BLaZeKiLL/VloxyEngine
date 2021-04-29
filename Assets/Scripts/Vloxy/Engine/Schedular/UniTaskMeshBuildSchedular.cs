﻿using System.Collections.Generic;
using System.Diagnostics;

using CBSL.Extension.UniTask.Threading;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Schedular {

    public class UniTaskMeshBuildSchedular<B> : MeshBuildSchedular<B> where B : IBlock {

        private int _batchSize;

        public UniTaskMeshBuildSchedular(ChunkBehaviourPool<B> chunkBehaviourPool) : base(chunkBehaviourPool) {
            _batchSize = VoxelProvider<B>.Current.Settings.Scheduler.BatchSize;
        }
        
        public override void Schedule(List<MeshBuildJobData<B>> jobs) => InternalProcess(jobs).Forget();

        protected override void Render(Chunk<B> chunk, MeshData meshData) {
            if (chunk.State == ChunkState.PROCESSING && meshData.Vertices.Count != 0) ChunkBehaviourPool.Claim(chunk).Render(meshData);
        }

        private async UniTaskVoid InternalProcess(List<MeshBuildJobData<B>> jobs) {
            PreProcess(jobs);

            var watch = new Stopwatch();
            
            watch.Start();
            await BatchScheduler.Process(
                jobs, 
                _batchSize, 
                meshBuildJobData => VoxelProvider<B>.Current.MeshBuilder().GenerateMesh(meshBuildJobData), 
                null,
                (batch, meshData) => {
                    for (int i = 0; i < meshData.Length; i++) {
                        Render(batch.Input[i].GetChunk(), meshData[i]);
                    }
                });
            watch.Stop();

            CBSL.Logging.Logger.Info<MeshBuildSchedular<B>>($"{jobs.Count} Jobs ({Mathf.CeilToInt((float) jobs.Count / _batchSize)} Batches) processed in : {watch.Elapsed.TotalMilliseconds:0.###} ms");

            PostProcess(jobs);
        }

    }

}