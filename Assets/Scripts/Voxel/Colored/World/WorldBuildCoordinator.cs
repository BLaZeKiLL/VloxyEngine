using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Library.Collections.Pools;
using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Colored.Mesher;
using CodeBlaze.Voxel.Engine.Core;
using CodeBlaze.Voxel.Engine.Core.Renderer;

using Cysharp.Threading.Tasks;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CodeBlaze.Voxel.Colored.World {

    public class WorldBuildCoordinator {

        private SimpleWorld _world;
        private Queue<ColoredChunk> _buildQueue;
        private IObjectPool<ChunkRenderer> _rendererPool;
        private BuildMethod _buildMethod;
        
        public WorldBuildCoordinator(SimpleWorld world, int drawSize, ChunkRendererSettings settings) {
            _world = world;
            _buildMethod = settings.BuildMethod;
            _buildQueue = new Queue<ColoredChunk>();
            
            _rendererPool = new ObjectPool<ChunkRenderer>(
                GetPoolSize(drawSize),
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
            switch (_buildMethod) {
                case BuildMethod.MULTI_THREADED:
                    BuildQueueMultiThread().Forget();
                    break;
                case BuildMethod.SINGLE_THREADED:
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
            watch.Start();
            while (_buildQueue.Count > 0) {
                var chunk = _buildQueue.Dequeue();
                var data = mesher.GenerateMesh(chunk, GetNeighbor(chunk));
                
                var renderer = _rendererPool.Claim();
                renderer.transform.position = chunk.Position;
                renderer.name += $" {chunk.ID}";
                renderer.Render(data);
                
                mesher.Clear();
            }
            watch.Stop();
                    
            Debug.Log($"Average mesh build time : {(float)watch.ElapsedMilliseconds / _world.Chunks.Count} ms");
            Debug.Log($"Build queue process time : {watch.Elapsed:s\\.fff} sec");
            
            GC.Collect();
        }

        private async UniTask<long> Build(ColoredChunk chunk) {
            var watch = new Stopwatch();
            
            await UniTask.SwitchToThreadPool();
            watch.Start();
            var data = new ColoredGreedyMesher().GenerateMesh(chunk, GetNeighbor(chunk));
            watch.Stop();
            await UniTask.SwitchToMainThread();

            var renderer = _rendererPool.Claim();
            renderer.transform.position = chunk.Position;
            renderer.name += $" {chunk.ID}";
            renderer.Render(data);
            
            return watch.ElapsedMilliseconds;
        }
        
        private NeighborChunks<ColoredBlock> GetNeighbor(ColoredChunk chunk) {
            var position = chunk.Position;
            var size = _world.ChunkSizeInt;
            var chunks = _world.Chunks;

            var px = position + Vector3Int.right * size;
            var py = position + Vector3Int.up * size;
            var pz = position + new Vector3Int(0, 0, 1) * size;
            var nx = position + Vector3Int.left * size;
            var ny = position + Vector3Int.down * size;
            var nz = position + new Vector3Int(0, 0, -1) * size;
            
            return new NeighborChunks<ColoredBlock> {
                ChunkPX = chunks.ContainsKey(px) ? chunks[px] : null,
                ChunkPY = chunks.ContainsKey(py) ? chunks[py] : null,
                ChunkPZ = chunks.ContainsKey(pz) ? chunks[pz] : null,
                ChunkNX = chunks.ContainsKey(nx) ? chunks[nx] : null,
                ChunkNY = chunks.ContainsKey(ny) ? chunks[ny] : null,
                ChunkNZ = chunks.ContainsKey(nz) ? chunks[nz] : null
            };
        }
        
        private int GetPoolSize(int input) {
            int ans = 1;
            
            for (int i = input; i > 0; i--)
                ans *= i;

            return ans + 1;
        }

        [Serializable]
        public class ChunkRendererSettings {

            [NonSerialized] public Transform Parent;
            public Material Material;
            public bool CastShadows;
            public BuildMethod BuildMethod;

        }
        
        public enum BuildMethod {

            MULTI_THREADED,
            SINGLE_THREADED
            
        }

    }

}