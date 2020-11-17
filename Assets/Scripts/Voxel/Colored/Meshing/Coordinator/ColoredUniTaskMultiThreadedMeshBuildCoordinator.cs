using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing;
using CodeBlaze.Voxel.Engine.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine.World;

namespace CodeBlaze.Voxel.Colored.Meshing.Coordinator {

    public class ColoredUniTaskMultiThreadedMeshBuildCoordinator : UniTaskMultiThreadedMeshBuildCoordinator<ColoredBlock> {

        public ColoredUniTaskMultiThreadedMeshBuildCoordinator(World<ColoredBlock> world) : base(world) { }
        
        protected override void Render(Chunk<ColoredBlock> chunk, MeshData data) {
            World.ChunkPool.Claim(chunk).Render(data);
        }

    }

}