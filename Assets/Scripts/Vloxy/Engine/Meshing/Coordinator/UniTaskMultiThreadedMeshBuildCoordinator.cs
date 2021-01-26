using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Data;

using Cysharp.Threading.Tasks;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public class UniTaskMultiThreadedMeshBuildCoordinator<B> : MeshBuildCoordinator<B> where B : IBlock {

        protected readonly Queue<ChunkJobData<B>> JobQueue;

        public UniTaskMultiThreadedMeshBuildCoordinator(ChunkPool<B> chunkPool) : base(chunkPool) {
            JobQueue = new Queue<ChunkJobData<B>>();
        }
        
        public override void Add(ChunkJobData<B> jobData) => JobQueue.Enqueue(jobData);
        
        public override void Process() => InternalProcess().Forget();

        protected override void Render(Chunk<B> chunk, MeshData meshData) {
            ChunkPool.Claim(chunk).Render(meshData);
        }

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private async UniTaskVoid InternalProcess() {
            var tasks = new List<UniTask<long>>();

            var watch = new Stopwatch();
            
            watch.Start();
            while (JobQueue.Count > 0) {
                tasks.Add(ScheduleJob(JobQueue.Dequeue()));
            }

            var result = await UniTask.WhenAll(tasks);
            watch.Stop();

            if (result.Length > 0) {
                UnityEngine.Debug.Log($"[MeshBuildCoordinator] Average mesh build time : {result.Average():0.###} ms");
                UnityEngine.Debug.Log($"[MeshBuildCoordinator] Build queue process time : {watch.Elapsed.TotalMilliseconds:0.###} ms");
            }

            GC.Collect();
            
            PostProcess();
        }
        
        private async UniTask<long> ScheduleJob(ChunkJobData<B> jobData) {
            var watch = new Stopwatch();
            
            var	meshData = await UniTask.RunOnThreadPool(
                () => {
                    watch.Start();
                    var _meshData = VoxelProvider<B>.Current.MeshBuilder().GenerateMesh(jobData);
                    watch.Stop();

                    return _meshData;
                }
            );

            Render(jobData.Chunk, meshData);
            
            return watch.ElapsedMilliseconds;
        }  
        #else
        public async UniTaskVoid InternalProcess() {
            var tasks = new List<UniTask>();
            
            while (JobQueue.Count > 0) {
                tasks.Add(ScheduleJob(JobQueue.Dequeue()));
            }
            
            await UniTask.WhenAll(tasks);
            
            GC.Collect();
            
            PostProcess();
        }
        
        private async UniTask ScheduleJob(ChunkJobData<B> jobData) {
            var meshData = await UniTask.RunOnThreadPool(
                () => VoxelProvider<B>.Current.MeshBuilder().GenerateMesh(jobData)
            );

            Render(jobData.Chunk, meshData);
        }  
        #endif

    }

}