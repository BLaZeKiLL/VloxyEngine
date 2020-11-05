using System;
using System.Collections.Generic;

using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Colored.Mesher;
using CodeBlaze.Voxel.Engine.Core.Renderer;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored.World {

    public class SimpleWorld : MonoBehaviour {

        [SerializeField] private Vector3 _chunkSize = new Vector3(16,16,16);
        [SerializeField] private int _drawSize;
        [SerializeField] [Range(0.01f, 0.99f)] private float _frequency = 0.15f;
        [SerializeField] private Material _material;
        [SerializeField] private bool _chunkShadows;

        private Queue<ColoredChunk> _chunks;
        private ColoredGreedyMesher _mesher;

        private void Awake() {
            _chunks = new Queue<ColoredChunk>();
            _mesher = new ColoredGreedyMesher();
        }

        private void Start() {
            var chunkSizeInt = Vector3Int.FloorToInt(_chunkSize);
            var id = 0;
            
            for (int x = -_drawSize; x <= _drawSize; x++) {
                for (int z = -_drawSize; z <= _drawSize; z++) {
                    _chunks.Enqueue(CreateChunk(chunkSizeInt, new Vector3Int(chunkSizeInt.x * x, 0, chunkSizeInt.z * z), ++id));
                }
            }
        }

        private void Update() {
            if (_chunks.Count <= 0) return;

            // convert to courotine
            SpawnChunk();
        }

        private void SpawnChunk() {
            var chunk = _chunks.Dequeue();

            var go = new GameObject($"Chunk-{chunk.ID}", typeof(ChunkRenderer));
            go.transform.parent = transform;
            go.transform.position = chunk.Position;
            
            var chunkRenderer = go.GetComponent<ChunkRenderer>();
            chunkRenderer.SetRenderSettings(_material, _chunkShadows);
            chunkRenderer.Render(_mesher.GenerateMesh(chunk));
            
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