using CodeBlaze.Voxel.Engine.Data;
using CodeBlaze.Voxel.Engine.Noise.Settings;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Noise.Profile {

    public class FastNoiseProfile2D<B> : INoiseProfile<B> where B : IBlock {

        private FastNoiseLite _noise;
        private NoiseSettings2D _settings;
        private int _heightHalf;
        
        protected virtual B GetBlock(int heightMapValue, int blockHeight) => default;
        
        public FastNoiseProfile2D(NoiseSettings2D settings) {
            _settings = settings;
            _heightHalf = settings.Height / 2;
            _noise = new FastNoiseLite(_settings.Seed);
            _noise.SetNoiseType(_settings.NoiseType);
            _noise.SetFrequency(_settings.Frequency);
            _noise.SetFractalType(_settings.FractalType);
            _noise.SetFractalGain(_settings.Gain);
            _noise.SetFractalLacunarity(_settings.Lacunarity);
            _noise.SetFractalOctaves(_settings.Octaves);
        }

        public void Fill(Chunk<B> chunk) {
            var pos = chunk.Position;
            var size = chunk.Size;
            
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z < size.z; z++) {
                    var height = GetHeight(pos.x + x, pos.z + z);

                    for (int y = 0; y < size.y; y++) {
                        chunk.SetBlock(GetBlock(height, pos.y + y), x,y,z);
                    }
                }
            }
        }

        private int GetHeight(int x, int z) {
            return Mathf.Clamp(Mathf.RoundToInt(_noise.GetNoise(x, z) * _heightHalf ), -_heightHalf, _heightHalf);
        }

    }

}