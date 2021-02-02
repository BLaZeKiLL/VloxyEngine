namespace CodeBlaze.Vloxy.Engine.Data {

    public interface IChunkData<B> where B : IBlock {

        void SetBlock(B block, int x, int y, int z);

        B GetBlock(int x, int y, int z);

    }

}