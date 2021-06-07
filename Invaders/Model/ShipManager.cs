namespace Invaders.Model
{
    public abstract class ShipManager
    {
        protected readonly OnShipChangedDelegate OnShipChanged;

        public delegate void OnShipChangedDelegate(Ship ship);

        protected ShipManager(OnShipChangedDelegate onShipChanged)
        {
            OnShipChanged = onShipChanged;
        }
    }
}