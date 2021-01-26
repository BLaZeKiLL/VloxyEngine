using System;
using System.Collections.Generic;
using System.Diagnostics;

using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public class SingleThreadedMeshBuildCoordinator<B> : MeshBuildCoordinator<B> where B : IBlock {

        protected readonly Queue<ChunkJobData<B>> JobQueue;

        public SingleThreadedMeshBuildCoordinator(ChunkPool<B> chunkPool) : base(chunkPool) {
            JobQueue = new Queue<ChunkJobData<B>>();
        }

        public override void Add(ChunkJobData<B> jobData) => JobQueue.Enqueue(jobData);

        public override void Process() {
            var mesher = VoxelProvider<B>.Current.MeshBuilder();
            var watch = new Stopwatch();
            var count = JobQueue.Count;
            
            watch.Start();
            while (JobQueue.Count > 0) {
                var data = JobQueue.Dequeue();

                Render(data.Chunk, mesher.GenerateMesh(data));
                
                mesher.Clear();
            }
            watch.Stop();
                    
            UnityEngine.Debug.Log($"[MeshBuildCoordinator] Average mesh build time : {(float)watch.ElapsedMilliseconds / count:0.###} ms");
            UnityEngine.Debug.Log($"[MeshBuildCoordinator] Build queue process time : {watch.Elapsed.TotalMilliseconds:0.###} ms");
            
            GC.Collect();
        }

        protected override void Render(Chunk<B> chunk, MeshData meshData) {
            ChunkPool.Claim(chunk).Render(meshData);
        }

    }

}