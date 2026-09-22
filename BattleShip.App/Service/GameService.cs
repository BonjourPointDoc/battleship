using System.Net.Http.Json;
using BattleShip.Models;

namespace BattleShip.App.Services;

public class GameService
{
    private readonly HttpClient _http;

    public event Action? OnStateChanged;

    public static readonly ShipType[] ShipTypes =
    [
        ShipType.Carrier,
        ShipType.Battleship,
        ShipType.Cruiser,
        ShipType.Submarine,
        ShipType.Destroyer
    ];

    public List<Ship> PlacedShips { get; } = [];
    public List<string> TurnHistory { get; } = [];
    public List<Position> KnownAiSunkPositions { get; } = [];

    public int CurrentShipIndex { get; private set; }
    public Direction CurrentDirection { get; private set; } = Direction.Horizontal;
    public Position? HoveredPosition { get; private set; }

    public bool PlacementFinished { get; private set; }
    public bool ShowStartingPlayerPopup { get; private set; }
    public string StartingPlayerMessage { get; private set; } = string.Empty;
    public bool GameStarted { get; private set; }
    public bool IsLoading { get; private set; } = true;
    public bool IsSubmittingPlacement { get; private set; }
    public string? ErrorMessage { get; private set; }

    public GameStateDto? CurrentGame { get; private set; }

    public GameService(HttpClient http)
    {
        _http = http;
    }

    public bool IsPlayerTurn =>
        CurrentGame != null && CurrentGame.CurrentPlayerId == CurrentGame.PlayerId;

    public ShipType CurrentShipType => ShipTypes[CurrentShipIndex];

    public IReadOnlyList<Position> PlacedShipPositions =>
        PlacedShips.SelectMany(ship => ship.GetPositions()).ToList();

    public IReadOnlyList<Position> PreviewShipPositions =>
        HoveredPosition is null || PlacementFinished
            ? []
            : GetPreviewPositions(HoveredPosition.Value).ToList();

    public IReadOnlyList<Position> PlayerHits => CurrentGame?.PlayerBoard?.Hits ?? [];
    public IReadOnlyList<Position> PlayerMisses => CurrentGame?.PlayerBoard?.Misses ?? [];
    public IReadOnlyList<Position> PlayerShotsReceived => CurrentGame?.PlayerBoard?.Shots ?? [];

    public IReadOnlyList<Position> AiHits => CurrentGame?.AiBoard?.Hits ?? [];
    public IReadOnlyList<Position> AiMisses => CurrentGame?.AiBoard?.Misses ?? [];
    public IReadOnlyList<Position> AiShotsReceived => CurrentGame?.AiBoard?.Shots ?? [];

    public IReadOnlyList<Position> PlayerSunk =>
        CurrentGame?.PlayerBoard?.Ships?
            .Where(s => s.GetPositions().All(p => PlayerHits.Any(h => h.Row == p.Row && h.Column == p.Column)))
            .SelectMany(s => s.GetPositions())
            .ToList() ?? [];

    public IReadOnlyList<Position> AiSunk
    {
        get
        {
            var sunkFromBackend = CurrentGame?.AiBoard?.Ships?
                .Where(s => s.GetPositions().All(p => AiHits.Any(h => h.Row == p.Row && h.Column == p.Column)))
                .SelectMany(s => s.GetPositions())
                .ToList() ?? [];

            var result = new List<Position>();
            foreach (var pos in sunkFromBackend.Concat(KnownAiSunkPositions))
            {
                if (!result.Any(p => p.Row == pos.Row && p.Column == pos.Column))
                {
                    result.Add(pos);
                }
            }
            return result;
        }
    }

    public bool IsPreviewValid =>
        !PlacementFinished && HoveredPosition is not null && IsInsideBoard(PreviewShipPositions) && !IsOverlapping(PreviewShipPositions);

    public async Task InitializeGameAsync(Guid? id, Action<Guid> onGameCreated)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            if (!id.HasValue)
            {
                var createResponse = await _http.PostAsync("api/games", null);
                if (!createResponse.IsSuccessStatusCode)
                {
                    ErrorMessage = "Impossible de créer une nouvelle partie.";
                    return;
                }

                CurrentGame = await createResponse.Content.ReadFromJsonAsync<GameStateDto>();
                if (CurrentGame != null)
                {
                    onGameCreated(CurrentGame.Id);
                }
            }
            else
            {
                CurrentGame = await _http.GetFromJsonAsync<GameStateDto>($"api/games/{id.Value}");
            }

            if (CurrentGame != null)
            {
                SyncGameState();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de l'initialisation : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    public void OnCellMouseEnter(Position position)
    {
        if (PlacementFinished) return;
        HoveredPosition = position;
        NotifyStateChanged();
    }

    public void OnCellMouseLeave()
    {
        HoveredPosition = null;
        NotifyStateChanged();
    }

    public void OnCellRightClicked(Position position)
    {
        if (PlacementFinished) return;
        CurrentDirection = CurrentDirection == Direction.Horizontal ? Direction.Vertical : Direction.Horizontal;
        HoveredPosition = position;
        NotifyStateChanged();
    }

    public async Task OnCellClickedAsync(Position position)
    {
        if (PlacementFinished) return;

        var positions = GetPreviewPositions(position).ToList();

        if (!IsInsideBoard(positions) || IsOverlapping(positions))
            return;

        var ship = new Ship(CurrentShipType, position, CurrentDirection);
        PlacedShips.Add(ship);
        HoveredPosition = null;

        if (CurrentShipIndex < ShipTypes.Length - 1)
        {
            CurrentShipIndex++;
            CurrentDirection = Direction.Horizontal;
            NotifyStateChanged();
            return;
        }

        PlacementFinished = true;
        await FinishPlacementAsync();
    }

    public async Task OnAiCellClickedAsync(Position position)
    {
        if (!GameStarted || CurrentGame == null || CurrentGame.Status != 1 || !IsPlayerTurn) return;
        if (AiShotsReceived.Any(s => s.Row == position.Row && s.Column == position.Column)) return;

        try
        {
            var request = new TakeShotRequest { Target = position };
            var response = await _http.PostAsJsonAsync($"api/games/{CurrentGame.Id}/shots", request);

            if (response.IsSuccessStatusCode)
            {
                var turnResult = await response.Content.ReadFromJsonAsync<TurnResponseDto>();
                if (turnResult != null)
                {
                    CurrentGame.Status = turnResult.GameStatus;
                    CurrentGame.CurrentPlayerId = turnResult.CurrentPlayerId;
                    CurrentGame.WinnerId = turnResult.WinnerId;

                    CurrentGame.AiBoard.Shots.Add(turnResult.PlayerShotResult.Position);
                    if (turnResult.PlayerShotResult.IsHit)
                        CurrentGame.AiBoard.Hits.Add(turnResult.PlayerShotResult.Position);
                    else
                        CurrentGame.AiBoard.Misses.Add(turnResult.PlayerShotResult.Position);

                    if (turnResult.PlayerShotResult.IsSunk)
                    {
                        FindAndRecordAiSunkShip(turnResult.PlayerShotResult.Position);
                    }

                    string playerMsg = $"[Vous] Tir en ({GetCoordText(position)}) : {(turnResult.PlayerShotResult.IsHit ? "TOUCHÉ !" : "MANQUÉ.")}";
                    if (turnResult.PlayerShotResult.IsSunk) playerMsg += " (Coulé !)";

                    TurnHistory.Add(playerMsg);

                    if (turnResult.AiShotResult != null)
                    {
                        CurrentGame.PlayerBoard.Shots.Add(turnResult.AiShotResult.Position);
                        if (turnResult.AiShotResult.IsHit)
                            CurrentGame.PlayerBoard.Hits.Add(turnResult.AiShotResult.Position);
                        else
                            CurrentGame.PlayerBoard.Misses.Add(turnResult.AiShotResult.Position);

                        string aiMsg = $"[IA] Tir en ({GetCoordText(turnResult.AiShotResult.Position)}) : {(turnResult.AiShotResult.IsHit ? "TOUCHÉ !" : "MANQUÉ.")}";
                        if (turnResult.AiShotResult.IsSunk) aiMsg += $" (Coulé : {turnResult.AiShotResult.SunkShipType})";

                        TurnHistory.Add(aiMsg);
                    }

                    NotifyStateChanged();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du tir : {ex.Message}");
        }
    }

    public void CloseStartingPlayerPopup()
    {
        ShowStartingPlayerPopup = false;
        NotifyStateChanged();
    }

    private async Task FinishPlacementAsync()
    {
        if (CurrentGame == null || IsSubmittingPlacement) return;
        IsSubmittingPlacement = true;

        try
        {
            var request = new PlaceShipsRequest { Ships = PlacedShips };
            var response = await _http.PostAsJsonAsync($"api/games/{CurrentGame.Id}/board", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PlaceShipsResponseDto>();
                if (result?.Game != null)
                {
                    CurrentGame = result.Game;
                    GameStarted = true;

                    bool playerStarts = result.InitialAiShot == null;
                    DisplayStartingPlayerPopup(playerStarts);

                    if (result.InitialAiShot != null)
                    {
                        var shot = result.InitialAiShot;
                        TurnHistory.Add($"[IA] Tir en ({GetCoordText(shot.Position)}) : {(shot.IsHit ? "TOUCHÉ !" : "MANQUÉ.")}");
                    }
                }
            }
            else
            {
                ErrorMessage = "Erreur lors de la validation du placement des navires.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur : {ex.Message}";
        }
        finally
        {
            IsSubmittingPlacement = false;
            NotifyStateChanged();
        }
    }

    private IEnumerable<Position> GetPreviewPositions(Position startPosition)
    {
        var size = CurrentShipType.Size();
        for (var i = 0; i < size; i++)
        {
            yield return CurrentDirection switch
            {
                Direction.Horizontal => new Position(startPosition.Row, startPosition.Column + i),
                Direction.Vertical => new Position(startPosition.Row + i, startPosition.Column),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private void FindAndRecordAiSunkShip(Position lastHit)
    {
        var hits = CurrentGame!.AiBoard.Hits;
        var sunkPositions = new List<Position> { lastHit };

        for (int i = 1; i < 10; i++)
        {
            var p = new Position(lastHit.Row, lastHit.Column + i);
            if (hits.Any(h => h.Row == p.Row && h.Column == p.Column)) sunkPositions.Add(p); else break;
        }
        for (int i = 1; i < 10; i++)
        {
            var p = new Position(lastHit.Row, lastHit.Column - i);
            if (hits.Any(h => h.Row == p.Row && h.Column == p.Column)) sunkPositions.Add(p); else break;
        }

        if (sunkPositions.Count > 1)
        {
            KnownAiSunkPositions.AddRange(sunkPositions);
            return;
        }

        sunkPositions = [lastHit];
        for (int i = 1; i < 10; i++)
        {
            var p = new Position(lastHit.Row + i, lastHit.Column);
            if (hits.Any(h => h.Row == p.Row && h.Column == p.Column)) sunkPositions.Add(p); else break;
        }
        for (int i = 1; i < 10; i++)
        {
            var p = new Position(lastHit.Row - i, lastHit.Column);
            if (hits.Any(h => h.Row == p.Row && h.Column == p.Column)) sunkPositions.Add(p); else break;
        }

        KnownAiSunkPositions.AddRange(sunkPositions);
    }

    private void DisplayStartingPlayerPopup(bool playerStarts)
    {
        StartingPlayerMessage = playerStarts
            ? "C'est vous qui commencez !"
            : "C'est l'IA qui commence ! Elle a déjà effectué son premier tir.";
        ShowStartingPlayerPopup = true;
    }

    private bool IsInsideBoard(IEnumerable<Position> positions) =>
        positions.All(p => p.Row >= 0 && p.Row < 10 && p.Column >= 0 && p.Column < 10);

    private bool IsOverlapping(IEnumerable<Position> positions)
    {
        var occupied = PlacedShips.SelectMany(s => s.GetPositions()).ToHashSet();
        return positions.Any(occupied.Contains);
    }

    private void SyncGameState()
    {
        if (CurrentGame == null) return;
        if (CurrentGame.Status == 1 || CurrentGame.Status == 2)
        {
            GameStarted = true;
            PlacementFinished = true;
            if (CurrentGame.PlayerBoard?.Ships != null && CurrentGame.PlayerBoard.Ships.Count > 0)
            {
                PlacedShips.Clear();
                PlacedShips.AddRange(CurrentGame.PlayerBoard.Ships);
            }
            RebuildHistoryFromGame();
        }
    }

    private void RebuildHistoryFromGame()
    {
        if (CurrentGame == null || TurnHistory.Count > 0) return;
        if (CurrentGame.AiBoard != null)
        {
            foreach (var pos in CurrentGame.AiBoard.Shots)
            {
                bool isHit = CurrentGame.AiBoard.Hits.Any(h => h.Row == pos.Row && h.Column == pos.Column);
                TurnHistory.Add($"[Vous] Tir en ({GetCoordText(pos)}) : {(isHit ? "TOUCHÉ !" : "MANQUÉ.")}");
            }
        }
        if (CurrentGame.PlayerBoard != null)
        {
            foreach (var pos in CurrentGame.PlayerBoard.Shots)
            {
                bool isHit = CurrentGame.PlayerBoard.Hits.Any(h => h.Row == pos.Row && h.Column == pos.Column);
                TurnHistory.Add($"[IA] Tir en ({GetCoordText(pos)}) : {(isHit ? "TOUCHÉ !" : "MANQUÉ.")}");
            }
        }
    }

    private static string GetCoordText(Position pos) => $"{(char)('A' + pos.Column)}{pos.Row + 1}";

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}

public class GameStateDto
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid AiId { get; set; }
    public Guid CurrentPlayerId { get; set; }
    public int Status { get; set; }
    public Guid? WinnerId { get; set; }
    public BoardDto PlayerBoard { get; set; } = new();
    public BoardDto AiBoard { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
}

public class TurnResponseDto
{
    public ShotResultDto PlayerShotResult { get; set; } = default!;
    public ShotResultDto? AiShotResult { get; set; }
    public int GameStatus { get; set; }
    public Guid CurrentPlayerId { get; set; }
    public Guid? WinnerId { get; set; }
}

public class BoardDto
{
    public List<Position> Shots { get; set; } = [];
    public List<Position> Hits { get; set; } = [];
    public List<Position> Misses { get; set; } = [];
    public List<Ship>? Ships { get; set; }
}

public class PlaceShipsRequest
{
    public List<Ship> Ships { get; set; } = [];
}

public class PlaceShipsResponseDto
{
    public GameStateDto Game { get; set; } = default!;
    public ShotResultDto? InitialAiShot { get; set; }
}

public class TakeShotRequest
{
    public Position Target { get; set; }
}

public class ShotResultDto
{
    public Position Position { get; set; }
    public bool IsHit { get; set; }
    public bool IsSunk { get; set; }
    public ShipType? SunkShipType { get; set; }
}