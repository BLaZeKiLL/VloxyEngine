namespace CodeBlaze.Voxel.Engine.Core.Mesher {

    public interface IMesher<T> where T : IBlock {

        MeshData GenerateMesh(Chunk<T> chunk, NeighborChunks<T> neighbors);

    }

}