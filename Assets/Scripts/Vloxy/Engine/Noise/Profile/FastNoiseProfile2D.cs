using System;
using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public class FastNoiseProfile2D<B> : INoiseProfile<B> where B : IBlock {

        private FastNoiseLite _noise;
        private int _heightHalf;

        private Dictionary<Vector2Int, int> _heightMap;
        
        protected virtual B GetBlock(int heightMapValue, int blockHeight) => default;
        
        public FastNoiseProfile2D(NoiseSettings2D settings) {
            _heightHalf = settings.Height / 2;
            _noise = new FastNoiseLite(settings.Seed);
            _noise.SetNoiseType(settings.NoiseType);
            _noise.SetFrequency(settings.Frequency);
            _noise.SetFractalType(settings.FractalType);
            _noise.SetFractalGain(settings.Gain);
            _noise.SetFractalLacunarity(settings.Lacunarity);
            _noise.SetFractalOctaves(settings.Octaves);
        }

        public void Generate(VoxelSettings settings) {
            var sizeX = settings.Chunk.ChunkPageSize * settings.Chunk.ChunkSize.x;
            var sizeZ = settings.Chunk.ChunkPageSize * settings.Chunk.ChunkSize.z;
            
            _heightMap = new Dictionary<Vector2Int, int>();

            for (int x = -sizeX; x < sizeX; x++) {
                for (int z = -sizeZ; z < sizeZ; z++) {
                    _heightMap.Add(new Vector2Int(x,z), GetHeight(x, z));
                }
            }
        }

        public void Fill(Chunk<B> chunk) {
            var pos = chunk.Position;
            var size = chunk.Size;
            
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z < size.z; z++) {
                    var height = _heightMap[new Vector2Int(pos.x + x, pos.z + z)];
                    
                    for (int y = 0; y < size.y; y++) {
                        chunk.SetBlock(GetBlock(height, pos.y + y), x,y,z);
                    }
                }
            }
        }

        public void Clear() {
            _heightMap.Clear();
        }

        private int GetHeight(int x, int z) {
            return Mathf.Clamp(Mathf.RoundToInt(_noise.GetNoise(x, z) * _heightHalf ), -_heightHalf, _heightHalf);
        }

    }

}