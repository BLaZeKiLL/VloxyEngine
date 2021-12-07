using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public interface INoiseProfile {

        void GenerateHeightMap();
        
        IChunkData GenerateChunkData(Chunk chunk);

        void Clear();

    }
    
    

}