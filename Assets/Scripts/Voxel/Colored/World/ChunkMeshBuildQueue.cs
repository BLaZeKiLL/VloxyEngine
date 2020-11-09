using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Colored.Mesher;
using CodeBlaze.Voxel.Engine.Settings;

using Cysharp.Threading.Tasks;

using Debug = UnityEngine.Debug;

namespace CodeBlaze.Voxel.Colored.World {

    public class ChunkMeshBuildQueue {
        
        private ColoredWorld _world;
        private Queue<ColoredChunk> _buildQueue;
        
        public ChunkMeshBuildQueue(ColoredWorld world) {
            _world = world;
            _buildQueue = new Queue<ColoredChunk>();
        }

        public void AddToBuildQueue(ColoredChunk chunk) => _buildQueue.Enqueue(chunk);

        public void Process() {
            switch (_world.BuildQueueSettings.ProcessMethod) {
                case BuildQueueSettings.BuildMethod.MultiThreaded:
                    BuildQueueMultiThread().Forget();

                    break;
                case BuildQueueSettings.BuildMethod.SingleThreaded:
                    BuildQueueSingleThread();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private async UniTaskVoid BuildQueueMultiThread() {
            var tasks = new List<UniTask<long>>();

            var watch = new Stopwatch();
            
            watch.Start();
            while (_buildQueue.Count > 0) {
                tasks.Add(Build(_buildQueue.Dequeue()));
            }

            var result = await UniTask.WhenAll(tasks);
            watch.Stop();

            Debug.Log($"Average mesh build time : {result.Average()} ms");
            Debug.Log($"Build queue process time : {watch.Elapsed:s\\.fff} sec");

            GC.Collect();
        }
        
        private void BuildQueueSingleThread() {
            var mesher = new ColoredGreedyMesher();
            var watch = new Stopwatch();
            var count = _buildQueue.Count;
            
            watch.Start();
            while (_buildQueue.Count > 0) {
                var chunk = _buildQueue.Dequeue();
                var data = mesher.GenerateMesh(chunk, _world.GetNeighbors(chunk));
                
                var renderer = _world.RendererPool.Claim();
                renderer.transform.position = chunk.Position;
                renderer.name += $" {chunk.ID}";
                renderer.Render(data);
                
                mesher.Clear();
            }
            watch.Stop();
                    
            Debug.Log($"Average mesh build time : {(float)watch.ElapsedMilliseconds / count} ms");
            Debug.Log($"Build queue process time : {watch.Elapsed:s\\.fff} sec");
            
            GC.Collect();
        }

        private async UniTask<long> Build(ColoredChunk chunk) {
            var watch = new Stopwatch();
            
            watch.Start();
            var data = await UniTask.RunOnThreadPool(
                () => new ColoredGreedyMesher().GenerateMesh(chunk, _world.GetNeighbors(chunk))
            );
            watch.Stop();

            var renderer = _world.RendererPool.Claim();
            renderer.transform.position = chunk.Position;
            renderer.name += $" {chunk.ID}";
            renderer.Render(data);
            
            return watch.ElapsedMilliseconds;
        }

    }

}