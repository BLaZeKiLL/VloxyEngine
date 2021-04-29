using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Schedular;

namespace CodeBlaze.Vloxy.Engine.Mesher {

    public interface IMesher<B> where B : IBlock {

        MeshData GenerateMesh(MeshBuildJobData<B> data);

        void Clear();

    }

}