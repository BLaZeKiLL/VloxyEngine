using CodeBlaze.Voxel.Engine.Data;

namespace CodeBlaze.Voxel.Noise.Voxel.Noise {

    public interface INoiseProfile<B> where B : IBlock {

        void Fill(Chunk<B> chunk);

    }

}