using System;
using System.Collections.Generic;

using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Engine.Core;

using UnityEngine;
using UnityEngine.Profiling;

namespace CodeBlaze.Voxel.Colored.World {

    public class SimpleWorld : MonoBehaviour {

        [SerializeField] private WorldSettings _worldSettings;
        [SerializeField] private WorldBuildCoordinator.ChunkRendererSettings _rendererSettings;

        public WorldSettings Settings => _worldSettings;

        private Dictionary<Vector3Int, ColoredChunk> _chunks;

        private WorldBuildCoordinator _coordinator;
        private bool _started;

        private void Awake() {
            _chunks = new Dictionary<Vector3Int, ColoredChunk>();
            
            _rendererSettings.Parent = transform;
            _coordinator = new WorldBuildCoordinator(this, _rendererSettings);
        }

        private void Start() {
            var id = 0;

            for (int x = -Settings.DrawSize; x <= Settings.DrawSize; x++) {
                for (int z = -Settings.DrawSize; z <= Settings.DrawSize; z++) {
                    var chunk = CreateChunk(Settings.ChunkSize, new Vector3Int(Settings.ChunkSize.x * x, 0, Settings.ChunkSize.z * z),
                        ++id);
                    _chunks.Add(chunk.Position, chunk);
                    _coordinator.AddToBuildQueue(chunk);
                }
            }
            
#if !BLOXY_BUILD_ONUPDATE
            _coordinator.ProcessBuildQueue();
#endif
            Debug.Log("World Start Done");
        }

#if BLOXY_BUILD_ONUPDATE
        private void Update() {
            if (_started || !Input.GetKeyDown(KeyCode.Space)) return;

            _started = true;
            _coordinator.ProcessBuildQueue();
        }
#endif
        
        public NeighborChunks<ColoredBlock> GetNeighbor(ColoredChunk chunk) {
            var position = chunk.Position;

            var px = position + Vector3Int.right * Settings.ChunkSize;
            var py = position + Vector3Int.up * Settings.ChunkSize;
            var pz = position + new Vector3Int(0, 0, 1) * Settings.ChunkSize;
            var nx = position + Vector3Int.left * Settings.ChunkSize;
            var ny = position + Vector3Int.down * Settings.ChunkSize;
            var nz = position + new Vector3Int(0, 0, -1) * Settings.ChunkSize;
            
            return new NeighborChunks<ColoredBlock> {
                ChunkPX = _chunks.ContainsKey(px) ? _chunks[px] : null,
                ChunkPY = _chunks.ContainsKey(py) ? _chunks[py] : null,
                ChunkPZ = _chunks.ContainsKey(pz) ? _chunks[pz] : null,
                ChunkNX = _chunks.ContainsKey(nx) ? _chunks[nx] : null,
                ChunkNY = _chunks.ContainsKey(ny) ? _chunks[ny] : null,
                ChunkNZ = _chunks.ContainsKey(nz) ? _chunks[nz] : null
            };
        }

        private ColoredChunk CreateChunk(Vector3Int size, Vector3Int position, int id) {
            var chunk = new ColoredChunk(size, position, id);
            
            var block = ColoredBlockTypes.RandomSolid();
            
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z <size.z; z++) {
                    var height = Mathf.FloorToInt(
                        Mathf.PerlinNoise((position.x + x) * Settings.Frequency, (position.z + z) * Settings.Frequency) * size.y
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
            [Range(0.01f, 0.99f)] public float Frequency = 0.15f;
            public Vector3Int ChunkSize = 16 * Vector3Int.one; // this is serialized, rider issue

        }
        
    }

}