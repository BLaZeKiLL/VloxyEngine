using System.Runtime.InteropServices;

using Unity.Burst;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Noise {

    /// <summary>
    /// ! Height is shifted by h - h/2 to make 0 actual 0
    /// </summary>
    [BurstCompile]
    public struct NoiseProfile {

        private Settings _Settings;

        private int _HalfHeight;
        private int _WaterLevel;

        public NoiseValue GetNoise(int3 position) => new() {
            Position = position,
            WaterLevel = _WaterLevel,
            Height = ComputeNoise(position),
        };

        public NoiseProfile(Settings settings) {
            _Settings = settings;

            if (_Settings.Scale <= 0) {
                _Settings.Scale = 0.0001f;
            }

            _HalfHeight = _Settings.Height / 2;
            _WaterLevel = _Settings.WaterLevel - _HalfHeight;
        }

        private int ComputeNoise(int3 position) {
            float amplitude = 1;
            float frequency = 1;
            float height = 0;

            float sampleX = (position.x + _Settings.Seed) / _Settings.Scale;
            float sampleZ = (position.z + _Settings.Seed) / _Settings.Scale;

            for (int i = 0; i < _Settings.Octaves; i++) {
                float noise = Unity.Mathematics.noise.cnoise(new float2(sampleX * frequency, sampleZ * frequency));

                height += noise * amplitude;

                amplitude *= _Settings.Persistance;
                frequency *= _Settings.Lacunarity;
            }

            // Note the height shift here
            return math.clamp((int) math.round(height * _HalfHeight), -_HalfHeight, _HalfHeight);
        }
        
        public struct Settings {
            public int Height;
            public int WaterLevel;
            public int Seed;
            public float Scale;
            public float Persistance;
            public float Lacunarity;
            public int Octaves;
        }

    }
    
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public struct NoiseValue {
        public int3 Position;
        public int WaterLevel;
        public int Height;
    }

}