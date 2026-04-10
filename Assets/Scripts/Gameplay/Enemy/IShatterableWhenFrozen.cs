namespace IceFactory.Gameplay.Enemy
{
    public interface IShatterableWhenFrozen
    {
        bool IsFrozen { get; }
        void TryShatter(ShatterSourceType sourceType);
    }
}
