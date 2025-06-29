namespace DonkeyKong.Model
{
    public interface IMoveable: IDisplayItem
    {
        bool CanMove(int x, int y);

        void Move(int x, int y);
    }
}