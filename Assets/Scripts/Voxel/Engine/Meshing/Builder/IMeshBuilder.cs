using CodeBlaze.Voxel.Engine.Chunk;

namespace CodeBlaze.Voxel.Engine.Meshing.Builder {

    public interface IMeshBuilder<B> where B : IBlock {

        MeshData GenerateMesh(Chunk<B> Chunk, NeighborChunks<B> Neighbor);

        void Clear();

    }

}