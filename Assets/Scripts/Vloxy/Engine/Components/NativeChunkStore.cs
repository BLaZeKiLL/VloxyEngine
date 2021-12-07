using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Components {

    public class NativeChunkStore {

        protected NativeHashMap<int3, Chunk> Chunks;
        
        private INoiseProfile _NoiseProfile;
        private ChunkSettings _ChunkSettings;
        private int _ViewRegionSize;

        public NativeChunkStore(INoiseProfile noiseProfile, ChunkSettings chunkSettings) {
            _NoiseProfile = noiseProfile;
            _ChunkSettings = chunkSettings;

            _ViewRegionSize = _ChunkSettings.DrawDistance.CubedSize();
            Chunks = new NativeHashMap<int3, Chunk>(_ChunkSettings.ChunkPageSize.CubedSize(), Allocator.Persistent);
        }

        internal void GenerateChunks() {
            for (int x = -_ChunkSettings.ChunkPageSize; x <= _ChunkSettings.ChunkPageSize; x++) {
                for (int z = -_ChunkSettings.ChunkPageSize; z <= _ChunkSettings.ChunkPageSize; z++) {
                    for (int y = -_ChunkSettings.ChunkPageSize; y <= _ChunkSettings.ChunkPageSize; y++) {
                        var pos = new int3(x, y, z) * _ChunkSettings.ChunkSize;
                        var chunk = VoxelProvider.Current.CreateChunk(pos);
                        chunk.Data = _NoiseProfile.GenerateChunkData(chunk);

                        Chunks.Add(pos, chunk);
                    }
                }
            }

            CBSL.Logging.Logger.Info<NativeChunkStore>("Chunks Created : " + Chunks.Count());
        }
        
        public Chunk GetChunk(int3 coord) => Chunks[coord];

        public bool ContainsChunk(int3 coord) => Chunks.ContainsKey(coord);

    }

}