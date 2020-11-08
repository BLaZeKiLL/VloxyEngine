using System;
using System.Collections.Generic;

using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored.World {

    public class SimpleWorld : MonoBehaviour {

        [SerializeField] private WorldSettings _worldSettings;
        [SerializeField] private WorldBuildCoordinator.ChunkRendererSettings _rendererSettings;

        public Dictionary<Vector3Int, ColoredChunk> Chunks { get; private set; }
        public Vector3Int ChunkSizeInt { get; private set; }

        private WorldBuildCoordinator _coordinator;

        private void Awake() {
            Chunks = new Dictionary<Vector3Int, ColoredChunk>();
            ChunkSizeInt = Vector3Int.FloorToInt(_worldSettings.ChunkSize);
            _rendererSettings.Parent = transform;
            _coordinator = new WorldBuildCoordinator(this, _worldSettings.DrawSize, _rendererSettings);
        }

        private void Start() {
            var id = 0;
            var drawSize = _worldSettings.DrawSize;
            
            for (int x = -drawSize; x <= drawSize; x++) {
                for (int z = -drawSize; z <= drawSize; z++) {
                    var chunk = CreateChunk(ChunkSizeInt, new Vector3Int(ChunkSizeInt.x * x, 0, ChunkSizeInt.z * z),
                        ++id);
                    Chunks.Add(chunk.Position, chunk);
                    _coordinator.AddToBuildQueue(chunk);
                }
            }
            
            _coordinator.ProcessBuildQueue();
            Debug.Log("World Start Done");
        }

        private ColoredChunk CreateChunk(Vector3Int size, Vector3Int position, int id) {
            var chunk = new ColoredChunk(size, position, id);
            var block = ColoredBlockTypes.RandomSolid();
            var frequency = _worldSettings.Frequency;
            
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z <size.z; z++) {
                    var height = Mathf.FloorToInt(
                        Mathf.PerlinNoise((position.x + x) * frequency, (position.z + z) * frequency) * size.y
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

        [Serializable]
        public class WorldSettings {

            public int DrawSize = 1;
            public Vector3 ChunkSize = 16 * Vector3.one;
            [Range(0.01f, 0.99f)] public float Frequency = 0.15f;

        }
        
    }

}