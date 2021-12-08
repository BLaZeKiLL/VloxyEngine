using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public class FastNoiseProfile2D : INoiseProfile {

        private FastNoiseLite _noise;
        private int _heightHalf;

        private Dictionary<Vector2Int, int> _heightMap;
        private ChunkSettings _chunkSettings;
        
        protected virtual int GetBlock(int heightMapValue, int blockHeight) => default;
        
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

            CBSL.Logging.Logger.Info<FastNoiseProfile2D>("Height Map Generated");
        }

        public NativeChunkData GenerateChunkData(int3 pos) {
            var data = VoxelProvider.Current.CreateChunkData();

            int current_block = -1;
            int count = 0;

            for (int y = 0; y < _chunkSettings.ChunkSize.y; y++) {
                for (int x = 0; x < _chunkSettings.ChunkSize.x; x++) {
                    for (int z = 0; z < _chunkSettings.ChunkSize.z; z++) {
                        var height = _heightMap[new Vector2Int(pos.x + x, pos.z + z)];
                        var block = GetBlock(height, pos.y + y);

                        if (block == current_block) {
                            count++;
                        } else {
                            data.AddBlocks(current_block, count);
                            current_block = block;
                            count = 1;
                        }
                    }
                }
            }

            return data;
        }

        public void Clear() {
            _heightMap.Clear();
        }

        private int GetHeight(int x, int z) {
            return Mathf.Clamp(Mathf.RoundToInt(_noise.GetNoise(x, z) * _heightHalf ), -_heightHalf, _heightHalf);
        }

    }

}