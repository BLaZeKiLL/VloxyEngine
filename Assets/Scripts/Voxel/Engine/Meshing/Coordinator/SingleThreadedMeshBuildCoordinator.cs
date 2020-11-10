using System;
using System.Collections.Generic;
using System.Diagnostics;

using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Builder;
using CodeBlaze.Voxel.Engine.World;

namespace CodeBlaze.Voxel.Engine.Meshing.Coordinator {

    public abstract class SingleThreadedMeshBuildCoordinator<T> : MeshBuildCoordinator<T> where T : IBlock {

        protected readonly Queue<Chunk<T>> BuildQueue;

        public SingleThreadedMeshBuildCoordinator(World<T> world) : base(world) {
            BuildQueue = new Queue<Chunk<T>>();
        }

        public override void Add(Chunk<T> chunk) => BuildQueue.Enqueue(chunk);

        public override void Process() {
            var mesher = MeshBuilderProvider();
            var watch = new Stopwatch();
            var count = BuildQueue.Count;
            
            watch.Start();
            while (BuildQueue.Count > 0) {
                var chunk = BuildQueue.Dequeue();

                Render(chunk, mesher.GenerateMesh(chunk, World.GetNeighbors(chunk)));
                
                mesher.Clear();
            }
            watch.Stop();
                    
            UnityEngine.Debug.Log($"Average mesh build time : {(float)watch.ElapsedMilliseconds / count} ms");
            UnityEngine.Debug.Log($"Build queue process time : {watch.Elapsed:s\\.fff} sec");
            
            GC.Collect();
        }

        protected abstract override IMeshBuilder<T> MeshBuilderProvider();

        protected abstract override void Render(Chunk<T> chunk, MeshData data);

    }

}