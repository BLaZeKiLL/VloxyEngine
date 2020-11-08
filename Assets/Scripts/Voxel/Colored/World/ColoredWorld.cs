using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Engine.Core.World;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored.World {

    public class ColoredWorld : SingletonWorld<ColoredWorld, ColoredBlock> {

        [SerializeField] private WorldBuildCoordinator.ChunkRendererSettings _rendererSettings;

        private WorldBuildCoordinator _coordinator;

        protected override void Awake() {
            base.Awake();
            _rendererSettings.Parent = transform;
            _coordinator = new WorldBuildCoordinator(_rendererSettings);
        }

        private void Start() {
            var id = 0;

            for (int x = -CurrentSettings.DrawSize; x <= CurrentSettings.DrawSize; x++) {
                for (int z = -CurrentSettings.DrawSize; z <= CurrentSettings.DrawSize; z++) {
                    var chunk = CreateChunk(CurrentSettings.ChunkSize, new Vector3Int(CurrentSettings.ChunkSize.x * x, 0, CurrentSettings.ChunkSize.z * z),
                        ++id);
                    Chunks.Add(chunk.Position, chunk);
                    _coordinator.AddToBuildQueue(chunk);
                }
            }
            
            #if !BLOXY_BUILD_ONUPDATE
            _coordinator.ProcessBuildQueue();
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
                        Mathf.PerlinNoise((position.x + x) * CurrentSettings.Frequency, (position.z + z) * CurrentSettings.Frequency) * size.y
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