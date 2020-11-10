using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Builder;
using CodeBlaze.Voxel.Engine.World;

namespace CodeBlaze.Voxel.Engine.Meshing.Coordinator {

    public abstract class MeshBuildCoordinator<T> where T : IBlock {

        protected readonly World<T> World; // circular reference

        protected MeshBuildCoordinator(World<T> world) {
            World = world;
        }

        public abstract void Add(Chunk<T> chunk);

        public abstract void Process();

        protected abstract IMeshBuilder<T> MeshBuilderProvider();

        protected abstract void Render(Chunk<T> chunk, MeshData data);
        
        protected virtual void PostProcess() { }

    }

}