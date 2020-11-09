using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Engine.Core.World;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored.World {

    public class ColoredWorld : World<ColoredBlock> {

        private ChunkMeshBuildQueue _buildQueue;

        protected override void Awake() {
            base.Awake();

            _buildQueue = new ChunkMeshBuildQueue(this);
        }

        private void Start() {
            var id = 0;

            for (int x = -WorldSettings.DrawSize; x <= WorldSettings.DrawSize; x++) {
                for (int z = -WorldSettings.DrawSize; z <= WorldSettings.DrawSize; z++) {
                    var chunk = CreateChunk(WorldSettings.ChunkSize, new Vector3Int(WorldSettings.ChunkSize.x * x, 0, WorldSettings.ChunkSize.z * z),
                        ++id);
                    Chunks.Add(chunk.Position, chunk);
                    _buildQueue.AddToBuildQueue(chunk);
                }
            }
            
            #if !BLOXY_BUILD_ONUPDATE
            _buildQueue.Process();
            #endif
            
            Debug.Log("World Start Done");
        }

        #if BLOXY_BUILD_ONUPDATE
        private bool _started;
        private void Update() {
            if (_started || !Input.GetKeyDown(KeyCode.Space)) return;

            _started = true;
            _coordinator.ProcessBuildQueue();
        }
        #endif

        private ColoredChunk CreateChunk(Vector3Int size, Vector3Int position, int id) {
            var chunk = new ColoredChunk(size, position, id);
            
            var block = ColoredBlockTypes.RandomSolid();
            
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z <size.z; z++) {
                    var height = Mathf.FloorToInt(
                        Mathf.PerlinNoise((position.x + x) * WorldSettings.Frequency, (position.z + z) * WorldSettings.Frequency) * size.y
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