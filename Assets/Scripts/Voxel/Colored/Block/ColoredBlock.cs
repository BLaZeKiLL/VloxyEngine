using CodeBlaze.Voxel.Engine.Core;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored.Block {

    public readonly struct ColoredBlock : IBlock {

        public Color32 Color { get; }

        public ColoredBlock(Color32 color) {
            Color = color;
        }

        public bool IsOpaque() => Color.a == byte.MaxValue;

        public bool IsTransparent() => Color.a == byte.MinValue;

        public bool IsTranslucent() => throw new System.NotImplementedException();

        
    }
    
    public static class ColoredBlockTypes {

        public static ColoredBlock Air() => new ColoredBlock(new Color32(0, 0, 0, 0));

        public static ColoredBlock Red() => new ColoredBlock(new Color32(255, 0, 0, 255));

        public static ColoredBlock Green() => new ColoredBlock(new Color32(0, 255, 0, 255));

        public static ColoredBlock Blue() => new ColoredBlock(new Color32(0, 0, 255, 255));
        
        public static ColoredBlock Null() => new ColoredBlock(new Color32(255,0,255,255));

        public static ColoredBlock RandomSolid() {
            var r = (byte) Random.Range(0, 256);
            var g = (byte) Random.Range(0, 256);
            var b = (byte) Random.Range(0, 256);

            return new ColoredBlock(new Color32(r, g, b, 255));
        }

    }

}