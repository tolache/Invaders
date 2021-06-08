namespace Invaders.Model
{
    public abstract class ShipManager
    {
        protected readonly OnShipChangedCallback OnShipChanged;

        protected ShipManager(OnShipChangedCallback onShipChanged)
        {
            OnShipChanged = onShipChanged;
        }

        public delegate void OnShipChangedCallback(Ship ship);
    }
}