using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Data;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public class UniTaskMultiThreadedMeshBuildCoordinator<B> : MeshBuildCoordinator<B> where B : IBlock {

        private int _batchSize;

        public UniTaskMultiThreadedMeshBuildCoordinator(ChunkPool<B> chunkPool, int batchSize) : base(chunkPool) {
            _batchSize = batchSize;
        }
        
        public override void Process(List<ChunkJobData<B>> jobs) => InternalProcess(jobs).Forget();

        protected override void Render(Chunk<B> chunk, MeshData meshData) {
            ChunkPool.Claim(chunk).Render(meshData);
        }

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private async UniTaskVoid InternalProcess(List<ChunkJobData<B>> jobs) {
            var watch = new Stopwatch();
            
            watch.Start();
            var result = await UniTask.WhenAll(CreateBatches(jobs).Select(ScheduleJob).ToList());
            watch.Stop();

            if (result.Length > 0) {
                UnityEngine.Debug.Log($"[MeshBuildCoordinator] Number of batches : {result.Length}");
                UnityEngine.Debug.Log($"[MeshBuildCoordinator] Average batch process time : {result.Average():0.###} ms");
                UnityEngine.Debug.Log($"[MeshBuildCoordinator] Total batch process time : {watch.Elapsed.TotalMilliseconds:0.###} ms");
            }
            
            PostProcess();
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
        public async UniTaskVoid InternalProcess(List<ChunkJobData<B>> jobs) {
            var tasks = new List<UniTask>();
            
            await UniTask.WhenAll(CreateBatches(jobs).Select(ScheduleJob).ToList());
            
            PostProcess();
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

        private IEnumerable<Batch> CreateBatches(List<ChunkJobData<B>> jobs) {
            var batches = new Batch[Mathf.CeilToInt((float) jobs.Count / 32)];
            var bindex = 0;
            for (int i = 0; i < jobs.Count; i += _batchSize) {
                batches[bindex++] = new Batch(jobs.GetRange(i, Math.Min(_batchSize, jobs.Count - i)));
            }

            return batches;
        }

        private class Batch {
            
            private List<ChunkJobData<B>> _data;

            public Batch(List<ChunkJobData<B>> data) {
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