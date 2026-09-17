namespace BattleShip.Models;

public enum GameState
{
    WaitingForPlayerBoard, // Le joueur doit placer ses bateaux
    InProgress,            // La partie est en cours
    Finished               // Un joueur a gagné
}