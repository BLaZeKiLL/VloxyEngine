using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Vloxy.Engine.Meshing.Builder {

    public interface IMeshBuilder<B> where B : IBlock {

        MeshData GenerateMesh(ChunkJobData<B> data);

        void Clear();

    }

}