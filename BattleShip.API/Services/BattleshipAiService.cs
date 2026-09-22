using Battleship.Contracts;
using BattleShip.Models;

public class BattleshipAiService : IBattleshipAiService
{
    private readonly IGameEngineService _engine;

    public BattleshipAiService(IGameEngineService engine)
    {
        _engine = engine;
    }

    public Board GenerateRandomBoard()
    {
        var ships = new List<Ship>();
        var shipTypes = Enum.GetValues<ShipType>();

        foreach (var type in shipTypes)
        {
            bool placed = false;
            while (!placed)
            {
                var dir = (Direction)Random.Shared.Next(2);
                var row = Random.Shared.Next(0, 10);
                var col = Random.Shared.Next(0, 10);

                var candidate = new Ship(type, new Position(row, col), dir);
                var positions = candidate.GetPositions().ToList();

                if (positions.Any(p => !_engine.IsValidPosition(p)))
                    continue;

                if (ships.SelectMany(s => s.GetPositions()).Any(p => positions.Contains(p)))
                    continue;

                ships.Add(candidate);
                placed = true;
            }
        }

        return new Board { Ships = ships };
    }

    public ShotResultDto ExecuteAiTurn(Game game)
    {
        Position aiTarget = SelectAiTarget(game.PlayerBoard);

        game.PlayerBoard = game.PlayerBoard.WithShot(aiTarget);
        var aiResult = _engine.ProcessShot(game.PlayerBoard, aiTarget);

        if (!game.PlayerBoard.IsGameOver())
        {
            game.CurrentPlayerId = game.PlayerId;
        }

        return aiResult;
    }

    public Position SelectAiTarget(Board board)
    {
        var potentialTargets = GetAdjacentTargetsToUnsunkHits(board);

        if (potentialTargets.Count > 0)
        {
            return potentialTargets[Random.Shared.Next(potentialTargets.Count)];
        }

        return GetRandomUnshotPosition(board);
    }

    private List<Position> GetAdjacentTargetsToUnsunkHits(Board board)
    {
        var candidates = new List<Position>();
        var unsunkHits = board.GetUnsunkHits();

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        foreach (var hit in unsunkHits)
        {
            for (int i = 0; i < 4; i++)
            {
                var neighbor = new Position(hit.Row + dx[i], hit.Column + dy[i]);
                if (_engine.IsValidPosition(neighbor) && !board.IsShot(neighbor) && !candidates.Contains(neighbor))
                {
                    candidates.Add(neighbor);
                }
            }
        }

        return candidates;
    }

    private static Position GetRandomUnshotPosition(Board board)
    {
        Position pos;
        do
        {
            pos = new Position(Random.Shared.Next(0, 10), Random.Shared.Next(0, 10));
        } while (board.IsShot(pos));

        return pos;
    }
}