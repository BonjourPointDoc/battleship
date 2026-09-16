public enum ShipType
{
    Carrier,
    Battleship,
    Cruiser,
    Submarine,
    Destroyer
}

public static class ShipTypeExtensions
{
    public static int Size(this ShipType type) =>
        type switch
        {
            ShipType.Carrier => 5,
            ShipType.Battleship => 4,
            ShipType.Cruiser => 3,
            ShipType.Submarine => 3,
            ShipType.Destroyer => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
}