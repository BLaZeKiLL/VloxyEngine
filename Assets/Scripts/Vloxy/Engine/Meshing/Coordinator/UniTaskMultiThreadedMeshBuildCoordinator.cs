using System;
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
            var result = await UniTask.WhenAll(CreateBatches(jobs).Select(ScheduleMeshBuildJob).ToList());
            watch.Stop();

            if (result.Length > 0) {
                UnityEngine.Debug.unityLogger.Log(TAG,$"Number of batches : {result.Length}");
                UnityEngine.Debug.unityLogger.Log(TAG,$"Average batch process time : {result.Average():0.###} ms");
                UnityEngine.Debug.unityLogger.Log(TAG,$"Total batch process time : {watch.Elapsed.TotalMilliseconds:0.###} ms");
            }
            
            PostProcess(jobs);
        }
        
        private async UniTask<long> ScheduleMeshBuildJob(Batch batch) {
            var watch = new Stopwatch();
            
            var	result = await UniTask.RunOnThreadPool(
                () => {
                    watch.Start();
                    var _meshData = batch.ForEach(job => VoxelProvider<B>.Current.MeshBuilder().GenerateMesh(job));
                    watch.Stop();

                    return _meshData;
                }
            );

            for (var index = 0; index < result.Length; index++) {
                Render(batch.Jobs[index].Chunk, result[index]);
            }

            return watch.ElapsedMilliseconds;
        }  
        #else
        public async UniTaskVoid InternalProcess(List<MeshBuildJobData<B>> jobs) {
            PreProcess(jobs);
            
            await UniTask.WhenAll(CreateBatches(jobs).Select(ScheduleMeshBuildJob).ToList());
            
            PostProcess(jobs);
        }
        
        private async UniTask ScheduleMeshBuildJob(Batch batch) {
            var	result = await UniTask.RunOnThreadPool(() => batch.ForEach(job => VoxelProvider<B>.Current.MeshBuilder().GenerateMesh(job)));

            for (var index = 0; index < result.Length; index++) {
                Render(batch.Jobs[index].Chunk, result[index]);
            }
        }  
        #endif

        protected IEnumerable<Batch> CreateBatches(List<MeshBuildJobData<B>> jobs) {
            var batches = new Batch[Mathf.CeilToInt((float) jobs.Count / _batchSize)];
            var bindex = 0;
            for (int i = 0; i < jobs.Count; i += _batchSize) {
                batches[bindex++] = new Batch(jobs.GetRange(i, Math.Min(_batchSize, jobs.Count - i)));
            }

            return batches;
        }

        protected class Batch {
            
            public List<MeshBuildJobData<B>> Jobs { get; }

            public Batch(List<MeshBuildJobData<B>> jobs) {
                Jobs = jobs;
            }

            public T[] ForEach<T>(Func<MeshBuildJobData<B>, T> func) {
                var result = new T[Jobs.Count];
                
                for (int i = 0; i < Jobs.Count; i++) {
                    result[i] = func(Jobs[i]);
                }

                return result;
            }

            public void ForEach(Action<MeshBuildJobData<B>> action) {
                for (int i = 0; i < Jobs.Count; i++) {
                    action(Jobs[i]);
                }
            }

        }

    }

}