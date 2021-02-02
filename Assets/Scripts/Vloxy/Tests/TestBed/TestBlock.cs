using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Voxel.Engine.Test.TestBed {

    public enum TestBlockType : byte {

        Air, Grass, Dirt, Stone

    }
    
    public struct TestBlock : IBlock {

        public TestBlockType Type { get; }

        public TestBlock(TestBlockType type) {
            Type = type;
        }

        public bool IsOpaque() => Type != TestBlockType.Air;

        public bool IsTransparent() => Type == TestBlockType.Air;

        public bool IsTranslucent() {
            throw new System.NotImplementedException();
        }

        public byte[] GetBytes() => new[] { (byte)Type };

    }

}