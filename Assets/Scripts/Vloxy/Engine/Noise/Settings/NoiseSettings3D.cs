using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Noise.Settings {

    [CreateAssetMenu(fileName = "NoiseSettings3D", menuName = "Vloxy/NoiseSettings/3D", order = 0)]
    public class NoiseSettings3D : INoiseSettings {

        public int Seed = 1337;
        public FastNoiseLite.NoiseType NoiseType = FastNoiseLite.NoiseType.Perlin;
        public float Frequency = 0.003f;
        public FastNoiseLite.FractalType FractalType = FastNoiseLite.FractalType.FBm;
        public float Gain = 0.5f;
        public float Lacunarity = 2f;
        public int Octaves = 5;

    }

}