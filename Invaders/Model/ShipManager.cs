namespace Invaders.Model
{
    public abstract class ShipManager
    {
        protected readonly OnShipChangedDelegate OnShipChanged;

        protected ShipManager(OnShipChangedDelegate onShipChanged)
        {
            OnShipChanged = onShipChanged;
        }

        public delegate void OnShipChangedDelegate(Ship ship);
    }
}