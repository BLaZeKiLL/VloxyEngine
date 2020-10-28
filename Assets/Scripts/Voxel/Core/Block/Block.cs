using UnityEngine;

namespace CodeBlaze.Voxel.Core {

    public struct Block {

        public Color32 Color { get; }

        public Block(Color32 color) {
            Color = color;
        }

        public bool IsSolid() => Color.a == byte.MaxValue;

        public bool IsEmpty() => Color.a == byte.MinValue;

    }

    public static class BlockTypes {

        public static Block Air() => new Block(new Color32(0, 0, 0, 0));

        public static Block Red() => new Block(new Color32(255, 0, 0, 255));

        public static Block Green() => new Block(new Color32(0, 255, 0, 255));

        public static Block Blue() => new Block(new Color32(0, 0, 255, 255));

        public static Block RandomSolid() {
            var r = (byte) Random.Range(0, 256);
            var g = (byte) Random.Range(0, 256);
            var b = (byte) Random.Range(0, 256);

            return new Block(new Color32(r, g, b, 255));
        }

    }

}