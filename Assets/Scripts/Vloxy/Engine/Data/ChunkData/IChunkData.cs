using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public interface IChunkData {

        void SetBlock(int block, int3 pos) {
            SetBlock(block, pos.x, pos.y, pos.z);
        }
        
        void SetBlock(int block, int x, int y, int z);

        int GetBlock(int3 pos) {
            return GetBlock(pos.x, pos.y, pos.z);
        }
        
        int GetBlock(int x, int y, int z);

        void AddBlocks(int block, int count);

    }

}