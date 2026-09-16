public sealed record Ship(
    ShipType Type,
    Position Position,
    Direction Direction
);

public static class ShipExtensions
{
    public static IEnumerable<Position> GetPositions(this Ship ship)
    {
        var size = ship.Type.Size();

        for (var i = 0; i < size; i++)
        {
            yield return ship.Direction switch
            {
                Direction.Horizontal =>
                    new Position(
                        ship.Position.Row,
                        ship.Position.Column + i),

                Direction.Vertical =>
                    new Position(
                        ship.Position.Row + i,
                        ship.Position.Column),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}