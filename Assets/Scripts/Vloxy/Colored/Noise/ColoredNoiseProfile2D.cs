using CodeBlaze.Vloxy.Colored.Data;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;

namespace CodeBlaze.Vloxy.Colored.Noise {

    public class ColoredNoiseProfile2D : FastNoiseProfile2D {

        private static readonly int COLOR = ColoredBlocks.RandomColor();
        
        public ColoredNoiseProfile2D(INoiseSettings settings, ChunkSettings chunkSettings) : base(settings, chunkSettings) { }
        
        protected override int GetBlock(int heightMapValue, int blockHeight) {
            return blockHeight > heightMapValue ? ColoredBlocks.Air() : COLOR;
        }

    }

}