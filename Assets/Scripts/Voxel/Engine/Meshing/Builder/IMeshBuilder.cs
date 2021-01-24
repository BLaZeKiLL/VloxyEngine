using CodeBlaze.Voxel.Engine.Data;

namespace CodeBlaze.Voxel.Engine.Meshing.Builder {

    public interface IMeshBuilder<B> where B : IBlock {

        MeshData GenerateMesh(ChunkJobData<B> data);

        void Clear();

    }

}