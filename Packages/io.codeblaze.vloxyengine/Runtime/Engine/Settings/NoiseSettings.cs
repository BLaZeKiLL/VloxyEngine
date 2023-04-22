using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [CreateAssetMenu(fileName = "NoiseSettings2D", menuName = "Vloxy/NoiseSettings/2D", order = 0)]
    public class NoiseSettings : ScriptableObject {

        public int Height = 256;
        public int WaterLevel = 96;
        public int Seed = 777;
        public float Scale = 256;
        public float Persistance = 0.5f;
        public float Lacunarity = 2f;
        public int Octaves = 4;

    }

}