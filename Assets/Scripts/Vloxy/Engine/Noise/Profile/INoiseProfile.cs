using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Noise.Profile {

    public interface INoiseProfile<B> where B : IBlock {

        void Generate(VoxelSettings settings);
        
        B[] Fill(Vector3Int pos);

        void Clear();

    }
    
    

}