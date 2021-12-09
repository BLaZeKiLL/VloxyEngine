using CodeBlaze.Vloxy.Engine.Data;

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public interface INoiseProfile {

        void GenerateHeightMap();
        
        ChunkData GenerateChunkData(int3 pos);

        void Dispose();

    }
    
    

}