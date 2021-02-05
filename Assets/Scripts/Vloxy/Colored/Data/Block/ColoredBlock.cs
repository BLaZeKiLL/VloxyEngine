using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Data;

using UnityEngine;

namespace CodeBlaze.Vloxy.Colored.Data.Block {

    public readonly struct ColoredBlock : IBlock {

        public Color32 Color { get; }

        public ColoredBlock(Color32 color) {
            Color = color;
        }

        public ColoredBlock(List<byte> bytes) {
            Color = new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        public bool IsOpaque() => Color.a == byte.MaxValue;

        public bool IsTransparent() => Color.a == byte.MinValue;

        public bool IsTranslucent() => throw new System.NotImplementedException();

        public byte[] GetBytes() => new[] { Color.r, Color.g, Color.b, Color.a };

        public bool Equals(ColoredBlock other) {
            var color2 = other.Color;

            return Color.r == color2.r && Color.g == color2.g && Color.b == color2.b && Color.a == color2.a;
        }

        public override bool Equals(object obj) {
            return obj is ColoredBlock other && Equals(other);
        }

        public override int GetHashCode() {
            return Color.GetHashCode();
        }

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