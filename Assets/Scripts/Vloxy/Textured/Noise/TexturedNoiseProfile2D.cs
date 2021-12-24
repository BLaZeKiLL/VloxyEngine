using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Textured.Data;

namespace CodeBlaze.Vloxy.Textured.Noise {

    public class TexturedNoiseProfile2D : FastNoiseProfile2D {

        public TexturedNoiseProfile2D(INoiseSettings settings, ChunkSettings chunkSettings) : base(settings, chunkSettings) { }

        protected override int GetBlock(int heightMapValue, int blockHeight) {
            //return (int)TexturedBlock.GRASS;
            
            if (blockHeight > heightMapValue ) return (int) TexturedBlock.AIR;
            if (blockHeight == heightMapValue) return (int) TexturedBlock.GRASS;
            if (blockHeight <= heightMapValue - 1 && blockHeight >= heightMapValue - 3) return (int)TexturedBlock.DIRT;

            return (int) TexturedBlock.STONE;
        }

    }

}