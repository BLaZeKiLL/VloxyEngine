#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
using System;
#endif

using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;

#if VLOXY_LOGGING
using CodeBlaze.Vloxy.Engine.Utils.Logger;
#endif

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public class FastNoiseProfile2D : INoiseProfile {

        private FastNoiseLite _noise;
        private int _heightHalf;

        private Dictionary<int2, int> _heightMap;
        private ChunkSettings _chunkSettings;
        
        protected virtual int GetBlock(int heightMapValue, int blockHeight) => default;
        
        public FastNoiseProfile2D(INoiseSettings settings, ChunkSettings chunkSettings) {
            var _settings = (NoiseSettings2D) settings;
            
            _heightHalf = _settings.Height / 2;
            
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
            var sizeZ = _chunkSettings.ChunkPageSize * _chunkSettings.ChunkSize.z;
            var sizeX = _chunkSettings.ChunkPageSize * _chunkSettings.ChunkSize.x;
            
            _heightMap = new Dictionary<int2, int>((2 * sizeX + 1) * (2 * sizeZ + 1));
            
            for (int z = -sizeZ; z <= sizeZ + _chunkSettings.ChunkSize.z; z++) {
                for (int x = -sizeX; x <= sizeX + _chunkSettings.ChunkSize.x; x++) {
                    _heightMap.Add(new int2(x,z), GetHeight(x, z));
                }
            }

#if VLOXY_LOGGING
            VloxyLogger.Info<FastNoiseProfile2D>("Height Map Generated");
#endif
        }

        public ChunkData GenerateChunkData(int3 pos) {
            var data = VloxyProvider.Current.CreateChunkData();

            int current_block = GetBlock(_heightMap[new int2(pos.x, pos.z)], pos.y);
            int count = 0;

            // Loop order should be same as flatten order for AddBlocks to work properly
            for (int y = 0; y < _chunkSettings.ChunkSize.y; y++) {
                for (int z = 0; z < _chunkSettings.ChunkSize.z; z++) {
                    for (int x = 0; x < _chunkSettings.ChunkSize.x; x++) {
                        var height = _heightMap[new int2(pos.x + x, pos.z + z)];
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
            
            data.AddBlocks(current_block, count); // Finale interval
            
            return data;
        }

        public List<Vector3> GetHeightMap() {
            return _heightMap.Select(pair => new Vector3(pair.Key.x, pair.Value, pair.Key.y)).ToList();
        }

        public void Dispose() {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
            _heightMap.Clear();
            GC.Collect();
#endif
        }

        private int GetHeight(int x, int z) {
            return Mathf.Clamp(Mathf.RoundToInt(_noise.GetNoise(x, z) * _heightHalf ), -_heightHalf, _heightHalf);
        }

    }

}