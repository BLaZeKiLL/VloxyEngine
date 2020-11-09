using CodeBlaze.Voxel.Engine.Chunk;

namespace CodeBlaze.Voxel.Engine.Mesher {

    public interface IMesher<T> where T : IBlock {

        MeshData GenerateMesh(Chunk<T> chunk, NeighborChunks<T> neighbors);

    }

}