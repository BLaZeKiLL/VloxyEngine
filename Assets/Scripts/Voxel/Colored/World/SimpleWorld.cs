using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Colored.Mesher;
using CodeBlaze.Voxel.Engine.Core.Renderer;

using UnityEngine;
using UnityEngine.Profiling;

using Debug = UnityEngine.Debug;

namespace CodeBlaze.Voxel.Colored.World {

    public class SimpleWorld : MonoBehaviour {

        [SerializeField] private Vector3 _chunkSize = new Vector3(16,16,16);
        [SerializeField] private int _drawSize;
        [SerializeField] [Range(0.01f, 0.99f)] private float _frequency = 0.15f;
        [SerializeField] private Material _material;
        [SerializeField] private bool _chunkShadows;

        //private Dictionary<Vector3Int, ColoredChunk> _chunks;
        private Queue<ColoredChunk> _buildQueue;
        private ColoredGreedyMesher _mesher;

        private Stopwatch _stopwatch;
        
        private void Awake() {
            //_chunks = new Dictionary<Vector3Int, ColoredChunk>();
            _buildQueue = new Queue<ColoredChunk>();
            _mesher = new ColoredGreedyMesher();
            
            _stopwatch = new Stopwatch();
        }

        private void Start() {
            var chunkSizeInt = Vector3Int.FloorToInt(_chunkSize);
            var id = 0;
            
            for (int x = -_drawSize; x <= _drawSize; x++) {
                for (int z = -_drawSize; z <= _drawSize; z++) {
                    var chunk = CreateChunk(chunkSizeInt, new Vector3Int(chunkSizeInt.x * x, 0, chunkSizeInt.z * z),
                        ++id);
                    //_chunks.Add(chunk.Position, chunk);
                    _buildQueue.Enqueue(chunk);
                }
            }

            StartCoroutine(SpawnChunks());
        }

        private IEnumerator SpawnChunks() {
            while (_buildQueue.Count > 0) {
                SpawnChunk();

                yield return null;
            }
            
            GC.Collect();
        }

        private void SpawnChunk() {
            var chunk = _buildQueue.Dequeue();

            var go = new GameObject($"Chunk-{chunk.ID}", typeof(ChunkRenderer));
            go.transform.parent = transform;
            go.transform.position = chunk.Position;
            
            var chunkRenderer = go.GetComponent<ChunkRenderer>();
            chunkRenderer.SetRenderSettings(_material, _chunkShadows);
            
            _stopwatch.Start();
            var data = _mesher.GenerateMesh(chunk);
            _stopwatch.Stop();
            
            Debug.Log($"CHUNK-{chunk.ID} MESH BUILD TIME : {_stopwatch.ElapsedMilliseconds}");
            
            _stopwatch.Reset();
            
            chunkRenderer.Render(data);
            _mesher.Clear();
        }
        
        private ColoredChunk CreateChunk(Vector3Int size, Vector3Int position, int id) {
            var chunk = new ColoredChunk(size, position, id);
            
            var block = ColoredBlockTypes.RandomSolid();
            
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z <size.z; z++) {
                    var height = Mathf.FloorToInt(
                        Mathf.PerlinNoise((position.x + x) * _frequency, (position.z + z) * _frequency) * size.y
                    );

                    height = Mathf.Clamp(height, 1, size.y - 1);

                    for (int y = 0; y < height; y++) {
                        chunk.SetBlock(block, x, y, z);
                    }

                    for (int y = height; y < size.y; y++) {
                        chunk.SetBlock(ColoredBlockTypes.Air(), x, y, z);
                    }
                }
            }
            
            return chunk;
        }

    }

}