using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public class FastNoiseProfile2D<B> : INoiseProfile<B> where B : IBlock {

        private FastNoiseLite _noise;
        private int _heightHalf;

        private Dictionary<Vector2Int, int> _heightMap;
        private ChunkSettings _chunkSettings;
        
        protected virtual B GetBlock(Chunk<B> chunk, int heightMapValue, int blockHeight) => default;
        
        public FastNoiseProfile2D(NoiseSettings2D settings, ChunkSettings chunkSettings) {
            _heightHalf = settings.Height / 2;
            _noise = new FastNoiseLite(settings.Seed);
            _noise.SetNoiseType(settings.NoiseType);
            _noise.SetFrequency(settings.Frequency);
            _noise.SetFractalType(settings.FractalType);
            _noise.SetFractalGain(settings.Gain);
            _noise.SetFractalLacunarity(settings.Lacunarity);
            _noise.SetFractalOctaves(settings.Octaves);

            _chunkSettings = chunkSettings;
        }

        public void GenerateHeightMap() {
            var sizeX = _chunkSettings.ChunkPageSize * _chunkSettings.ChunkSize.x;
            var sizeZ = _chunkSettings.ChunkPageSize * _chunkSettings.ChunkSize.z;
            
            _heightMap = new Dictionary<Vector2Int, int>();

            for (int x = -sizeX; x <= sizeX + _chunkSettings.ChunkSize.x; x++) {
                for (int z = -sizeZ; z <= sizeZ + _chunkSettings.ChunkSize.z; z++) {
                    _heightMap.Add(new Vector2Int(x,z), GetHeight(x, z));
                }
            }

            CBSL.Logging.Logger.Info<FastNoiseProfile2D<B>>("Height Map Generated");
        }

        public IChunkData<B> GenerateChunkData(Chunk<B> chunk) {
            var blocks = new B[_chunkSettings.ChunkSize.Size()];
            var pos = chunk.Position;

            for (int x = 0; x < _chunkSettings.ChunkSize.x; x++) {
                for (int z = 0; z < _chunkSettings.ChunkSize.z; z++) {
                    var height = _heightMap[new Vector2Int(pos.x + x, pos.z + z)];
                    
                    for (int y = 0; y < _chunkSettings.ChunkSize.y; y++) {
                        blocks[_chunkSettings.ChunkSize.Flatten(x, y, z)] = GetBlock(chunk, height, pos.y + y);
                    }
                }
            }

            return VoxelProvider<B>.Current.CreateChunkData(blocks);
        }

        public void Clear() {
            _heightMap.Clear();
        }

        private int GetHeight(int x, int z) {
            return Mathf.Clamp(Mathf.RoundToInt(_noise.GetNoise(x, z) * _heightHalf ), -_heightHalf, _heightHalf);
        }

    }

}