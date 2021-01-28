using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Settings;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public interface INoiseProfile<B> where B : IBlock {

        void Generate(VoxelSettings settings);
        
        void Fill(Chunk<B> chunk);

        void Clear();

    }
    
    

}