using Game.Shared.Enums;

namespace Game.Shared.Interfaces
{
    public interface IHexBridge
    {
        SlopeDir SlopeDir { get; set; }
        int Elevation { get; set; }
        int Version { get; set; }
    }
}
