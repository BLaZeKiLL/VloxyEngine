using System;
using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;

#if VLOXY_LOGGING
using CodeBlaze.Vloxy.Engine.Utils.Logger;
#endif

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public class FastNoiseProfile3D : INoiseProfile {

        private FastNoiseLite _noise;

        private Dictionary<int3, byte> _heightMap;
        private ChunkSettings _chunkSettings;
        
        protected virtual int GetBlock(byte value) => default;

        public FastNoiseProfile3D(INoiseSettings settings, ChunkSettings chunkSettings) {
            var _settings = (NoiseSettings3D) settings;
            
            _noise = new FastNoiseLite(_settings.Seed);
            
            _noise.SetSeed(UnityEngine.Random.Range(10000, 100000));
            _noise.SetNoiseType(_settings.NoiseType);
            _noise.SetFrequency(_settings.Frequency);
            _noise.SetFractalType(_settings.FractalType);
            _noise.SetFractalGain(_settings.Gain);
            _noise.SetFractalLacunarity(_settings.Lacunarity);
            _noise.SetFractalOctaves(_settings.Octaves);

            _chunkSettings = chunkSettings;
        }

        public void GenerateHeightMap() {
            var sizeY = _chunkSettings.ChunkPageSize * _chunkSettings.ChunkSize.y;
            var sizeZ = _chunkSettings.ChunkPageSize * _chunkSettings.ChunkSize.z;
            var sizeX = _chunkSettings.ChunkPageSize * _chunkSettings.ChunkSize.x;

            _heightMap = new Dictionary<int3, byte>((2 * sizeX + 1) * (2 * sizeZ + 1) * (2 * sizeY + 1));

            for (int y = -sizeY; y < sizeY + _chunkSettings.ChunkSize.y; y++) {
                for (int z = -sizeZ; z <= sizeZ + _chunkSettings.ChunkSize.z; z++) {
                    for (int x = -sizeX; x <= sizeX + _chunkSettings.ChunkSize.x; x++) {
                        _heightMap.Add(new int3(x, y, z), GetNoise(x, y, z));
                    }
                }
            }


#if VLOXY_LOGGING
            VloxyLogger.Info<FastNoiseProfile2D>("Height Map Generated");
#endif
        }

        public ChunkData GenerateChunkData(int3 pos) {
            var data = VloxyProvider.Current.CreateChunkData();

            int current_block = GetBlock(_heightMap[pos]);
            int count = 0;

            // Loop order should be same as flatten order for AddBlocks to work properly
            for (int y = 0; y < _chunkSettings.ChunkSize.y; y++) {
                for (int z = 0; z < _chunkSettings.ChunkSize.z; z++) {
                    for (int x = 0; x < _chunkSettings.ChunkSize.x; x++) {
                        var value = _heightMap[new int3(pos.x + x, pos.y + y, pos.z + z)];
                        var block = GetBlock(value);

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
            
            data.AddBlocks(current_block, count); // Finale interval
            
            return data;
        }

        public void Dispose() {
// #if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
            _heightMap.Clear();
            GC.Collect();
// #endif
        }
        
        private byte GetNoise(int x, int y, int z) {
            return (byte) (_noise.GetNoise(x, y, z) > 0 ? 1 : 0);
        }

    }

}