﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Builder;
using CodeBlaze.Voxel.Engine.World;

using Cysharp.Threading.Tasks;

namespace CodeBlaze.Voxel.Engine.Meshing.Coordinator {

    public abstract class UniTaskMultiThreadedMeshBuildCoordinator<B> : MeshBuildCoordinator<B> where B : IBlock {

        protected readonly Queue<Chunk<B>> BuildQueue;

        protected UniTaskMultiThreadedMeshBuildCoordinator(World<B> world) : base(world) {
            BuildQueue = new Queue<Chunk<B>>();
        }
        
        public override void Add(Chunk<B> chunk) => BuildQueue.Enqueue(chunk);
        
        public override void Process() => InternalProcess().Forget();

        protected abstract override IMeshBuilder<B> MeshBuilderProvider();

        protected abstract override void Render(Chunk<B> chunk, MeshData data);

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private async UniTaskVoid InternalProcess() {
            var tasks = new List<UniTask<long>>();

            var watch = new Stopwatch();
            
            watch.Start();
            while (BuildQueue.Count > 0) {
                tasks.Add(Build(BuildQueue.Dequeue()));
            }

            var result = await UniTask.WhenAll(tasks);
            watch.Stop();

            UnityEngine.Debug.Log($"[MeshBuildCoordinator] Average mesh build time : {result.Average()} ms");
            UnityEngine.Debug.Log($"[MeshBuildCoordinator] Build queue process time : {watch.Elapsed:s\\.fff} sec");

            GC.Collect();
            
            PostProcess();
        }
        
        private async UniTask<long> Build(Chunk<B> chunk) {
            var watch = new Stopwatch();
            
            var data = await UniTask.RunOnThreadPool(
                () => {
                    watch.Start();
                    var _data = MeshBuilderProvider().GenerateMesh(chunk, World.GetNeighbors(chunk));
                    watch.Stop();

                    return _data;
                }
            );

            Render(chunk, data);
            
            return watch.ElapsedMilliseconds;
        }  
        #else
        public async UniTaskVoid InternalProcess() {
            var tasks = new List<UniTask>();
            
            while (BuildQueue.Count > 0) {
                tasks.Add(Build(BuildQueue.Dequeue()));
            }
            
            await UniTask.WhenAll(tasks);
            
            GC.Collect();
            
            PostProcess();
        }
        
        private async UniTask Build(Chunk<B> chunk) {
            var data = await UniTask.RunOnThreadPool(
                () => MeshBuilderProvider().GenerateMesh(chunk, World.GetNeighbors(chunk))
            );

            Render(chunk, data);
        }  
        #endif

    }

}