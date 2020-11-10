using CodeBlaze.Voxel.Engine.Chunk;

namespace CodeBlaze.Voxel.Engine.Meshing.Builder {

    public interface IMeshBuilder<T> where T : IBlock {

        MeshData GenerateMesh(Chunk<T> chunk, NeighborChunks<T> neighbors);

        void Clear();

    }

}