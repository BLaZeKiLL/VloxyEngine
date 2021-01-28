using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public class FastNoiseProfile2D<B> : INoiseProfile<B> where B : IBlock {

        private FastNoiseLite _noise;
        private int _heightHalf;
        
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