using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Noise.Settings {

    [CreateAssetMenu(fileName = "NoiseSettings2D", menuName = "Voxel Engine/NoiseSettings/2D", order = 0)]
    public class NoiseSettings2D : INoiseSettings {

        public int Height = 256;
        public int Seed = 1337;
        public FastNoiseLite.NoiseType NoiseType = FastNoiseLite.NoiseType.Perlin;
        public float Frequency = 0.005f;
        public FastNoiseLite.FractalType FractalType = FastNoiseLite.FractalType.FBm;
        public float Gain = 0.5f;
        public float Lacunarity = 2f;
        public int Octaves = 5;

    }

}