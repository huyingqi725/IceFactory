namespace IceFactory.Gameplay.Interaction
{
    public interface IPlayerInteractable
    {
        bool CanInteract(PlayerInteractor interactor);
        void Interact(PlayerInteractor interactor);
    }
}
