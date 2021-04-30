using CodeBlaze.Vloxy.Colored.Data.Block;
using CodeBlaze.Vloxy.Colored.Data.Chunk;
using CodeBlaze.Vloxy.Colored.Meshing.Builder;
using CodeBlaze.Vloxy.Colored.Noise;

using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Colored {

    public class ColoredVoxelProvider : VoxelProvider<ColoredBlock> {
        
        public override IMesher<ColoredBlock> MeshBuilder() => new ColoredGreedyMesher();

        public override INoiseProfile<ColoredBlock> NoiseProfile() => new ColoredNoiseProfile2D(Settings.NoiseSettings as NoiseSettings2D, Settings.Chunk);

        public override Chunk<ColoredBlock> CreateChunk(Vector3Int position) => new ColoredChunk(position, Color.red);

    }

}