using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Library.Collections.Pools;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Colored.Mesher;
using CodeBlaze.Voxel.Engine.Core.Renderer;

using Cysharp.Threading.Tasks;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CodeBlaze.Voxel.Colored.World {

    public class WorldBuildCoordinator {

        public ChunkRendererSettings RendererSettings { get; }
        
        private SimpleWorld _world;
        private Queue<ColoredChunk> _buildQueue;
        private IObjectPool<ChunkRenderer> _rendererPool;
        
        public WorldBuildCoordinator(SimpleWorld world, ChunkRendererSettings settings) {
            _world = world;
            RendererSettings = settings;
            _buildQueue = new Queue<ColoredChunk>();
            
            _rendererPool = new ObjectPool<ChunkRenderer>(
                GetPoolSize(_world.Settings.DrawSize),
                index => {
                    var go = new GameObject("Chunk", typeof(ChunkRenderer));
                    go.transform.parent = settings.Parent;
                    go.SetActive(false);
            
                    var chunkRenderer = go.GetComponent<ChunkRenderer>();
                    chunkRenderer.SetRenderSettings(settings.Material, settings.CastShadows);

                    return chunkRenderer;
                },
                renderer => renderer.gameObject.SetActive(true),
                renderer => renderer.gameObject.SetActive(false)
            );
        }

        public void AddToBuildQueue(ColoredChunk chunk) => _buildQueue.Enqueue(chunk);

        public void ProcessBuildQueue() {
            switch (RendererSettings.BuildMethod) {
                case BuildMethod.MultiThreaded:
                    BuildQueueMultiThread().Forget();

                    break;
                case BuildMethod.SingleThreaded:
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
                var data = mesher.GenerateMesh(chunk, _world.GetNeighbor(chunk));
                
                var renderer = _rendererPool.Claim();
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
                () => new ColoredGreedyMesher().GenerateMesh(chunk, _world.GetNeighbor(chunk))
            );
            watch.Stop();

            var renderer = _rendererPool.Claim();
            renderer.transform.position = chunk.Position;
            renderer.name += $" {chunk.ID}";
            renderer.Render(data);
            
            return watch.ElapsedMilliseconds;
        }
        
        private int GetPoolSize(int input) => (2 * input + 1) * (2 * input + 1) + 1;

        [Serializable]
        public class ChunkRendererSettings {

            [NonSerialized] public Transform Parent;
            public Material Material;
            public bool CastShadows;
            public BuildMethod BuildMethod;

        }
        
        public enum BuildMethod {

            MultiThreaded,
            SingleThreaded
            
        }

    }

}