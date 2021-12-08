using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;

namespace CodeBlaze.Vloxy.Colored.Noise {

    public class ColoredNoiseProfile2D : FastNoiseProfile2D {

        public ColoredNoiseProfile2D(NoiseSettings2D settings, ChunkSettings chunkSettings) : base(settings, chunkSettings) { }
        
        protected override int GetBlock(int heightMapValue, int blockHeight) {
            return heightMapValue >= blockHeight ? 1 : 0;
        }

    }

}