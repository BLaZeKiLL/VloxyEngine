using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public interface INoiseProfile<B> where B : IBlock {

        void GenerateHeightMap();
        
        IChunkData<B> GenerateChunkData(Chunk<B> chunk);

        void Clear();

    }
    
    

}