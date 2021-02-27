using CodeBlaze.Vloxy.Colored.Data.Block;
using CodeBlaze.Vloxy.Engine.Data;

using UnityEngine;

namespace CodeBlaze.Vloxy.Colored.Data.Chunk {

    public class ColoredChunk : Chunk<ColoredBlock> {

        public Color32 BlockColor { get; }
        
        public ColoredChunk(Vector3Int position, Color32 blockColor) : base(position) {
            BlockColor = blockColor;
        }

    }

}