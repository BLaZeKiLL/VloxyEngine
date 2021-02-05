using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;

namespace CodeBlaze.Vloxy.Engine.Meshing.Builder {

    public interface IMeshBuilder<B> where B : IBlock {

        MeshData GenerateMesh(MeshBuildJobData<B> data);

        void Clear();

    }

}