using CodeBlaze.Voxel.Engine.Data;

namespace CodeBlaze.Voxel.Engine.Noise.Profile {

    public interface INoiseProfile<B> where B : IBlock {

        void Fill(Chunk<B> chunk);

    }
    
    

}