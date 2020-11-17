using System;
using System.Collections.Generic;
using System.Diagnostics;

using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.World;

namespace CodeBlaze.Voxel.Engine.Meshing.Coordinator {

    public class SingleThreadedMeshBuildCoordinator<B> : MeshBuildCoordinator<B> where B : IBlock {

        protected readonly Queue<Chunk<B>> BuildQueue;

        public SingleThreadedMeshBuildCoordinator(World<B> world) : base(world) {
            BuildQueue = new Queue<Chunk<B>>();
        }

        public override void Add(Chunk<B> chunk) => BuildQueue.Enqueue(chunk);

        public override void Process() {
            var mesher = VoxelProvider<B>.Current.MeshBuilder();
            var watch = new Stopwatch();
            var count = BuildQueue.Count;
            
            watch.Start();
            while (BuildQueue.Count > 0) {
                var chunk = BuildQueue.Dequeue();

                Render(chunk, mesher.GenerateMesh(chunk, World.GetNeighbors(chunk)));
                
                mesher.Clear();
            }
            watch.Stop();
                    
            UnityEngine.Debug.Log($"[MeshBuildCoordinator] Average mesh build time : {(float)watch.ElapsedMilliseconds / count:0.###} ms");
            UnityEngine.Debug.Log($"[MeshBuildCoordinator] Build queue process time : {watch.Elapsed.TotalMilliseconds:0.###} ms");
            
            GC.Collect();
        }

        protected override void Render(Chunk<B> chunk, MeshData data) {
            World.ChunkPool.Claim(chunk).Render(data);
        }

    }

}