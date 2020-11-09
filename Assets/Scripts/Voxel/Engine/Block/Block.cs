namespace CodeBlaze.Voxel.Engine {

    public interface IBlock {

        bool IsOpaque();

        bool IsTransparent();

        bool IsTranslucent();

    }

    public struct CoreBlock : IBlock {

        public byte ID { get; }

        public CoreBlock(byte id) {
            ID = id;
        }

        public bool IsOpaque() {
            return ID != 1;
        }

        public bool IsTransparent() {
            return ID == 1;
        }

        public bool IsTranslucent() {
            throw new System.NotImplementedException();
        }

    }
    
    public static class CoreBlockTypes {

        

    }

}