using CodeBlaze.Vloxy.Colored.Data.Block;
using CodeBlaze.Vloxy.Colored.Data.Chunk;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;

namespace CodeBlaze.Vloxy.Colored.Noise {

    public class ColoredNoiseProfile2D : FastNoiseProfile2D<ColoredBlock> {

        public ColoredNoiseProfile2D(NoiseSettings2D settings, ChunkSettings chunkSettings) : base(settings, chunkSettings) { }
        
        protected override ColoredBlock GetBlock(Chunk<ColoredBlock> chunk, int heightMapValue, int blockHeight) {
            return heightMapValue >= blockHeight ? new ColoredBlock(((ColoredChunk) chunk).BlockColor) : ColoredBlockTypes.Air();
        }

    }

}