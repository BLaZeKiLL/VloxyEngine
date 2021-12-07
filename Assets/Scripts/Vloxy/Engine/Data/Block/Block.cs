namespace CodeBlaze.Vloxy.Engine.Data {

    public interface IBlock {

        bool IsOpaque();

        bool IsTransparent();

        bool IsTranslucent();

        int GetId();

        IBlock FromId(int id);

    }

}