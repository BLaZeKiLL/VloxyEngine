using System;
using System.Collections.Generic;
using System.Diagnostics;

using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public class SingleThreadedMeshBuildCoordinator<B> : MeshBuildCoordinator<B> where B : IBlock {
        
        private const string TAG = "<color=green>MeshBuildCoordinator</color>";
        
        public SingleThreadedMeshBuildCoordinator(ChunkPool<B> chunkPool) : base(chunkPool) { }
        
        public override void Process(List<ChunkJobData<B>> jobs) {
            var mesher = VoxelProvider<B>.Current.MeshBuilder();
            var watch = new Stopwatch();
            
            watch.Start();

            foreach (var job in jobs) {
                Render(job.Chunk, mesher.GenerateMesh(job));
                mesher.Clear();
            }
            
            watch.Stop();
            
            UnityEngine.Debug.unityLogger.Log(TAG,$"Average mesh build time : {(float)watch.ElapsedMilliseconds / jobs.Count:0.###} ms");
            UnityEngine.Debug.unityLogger.Log(TAG,$"Build queue process time : {watch.Elapsed.TotalMilliseconds:0.###} ms");
            
            GC.Collect();
        }

        protected override void Render(Chunk<B> chunk, MeshData meshData) {
            ChunkPool.Claim(chunk).Render(meshData);
        }

    }

}