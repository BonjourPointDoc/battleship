# Échanges décisifs avec l’IA

## Date et sujet

- Outil / modèle si connu :
- Contexte :
- Prompt réellement utilisé :
- Réponse et hypothèses résumées :
- Décision et justification :
- Scénario ou commande de vérification :
- Résultat attendu, puis résultat observé :
- Erreur que ce contrôle pourrait détecter :
- Preuves reproductibles et limites :

## 19/09/2026 - Génération initiale API

- Outil / modèle si connu : Gemini (3.6 Flash Extended)

- Contexte : A partir des modèles que nous avons créé, nous avons demandé à l'IA de générer l'API. 
**Note :** Ce prompt a été réalisé dans le même chat que celui définissant l'architecture et donnant notre diagramme fonctionnel afin que l'IA aie accès à ce contexte.

- Prompt réellement utilisé :
L'API doit être une API ASP.NET Core en Minimal API (.NET 10). Nous avons une bibliothèque de modèles 
avec ces modèles qui sera utilisée par le frontend et le backend :
public sealed class Game
{
    public Guid Id { get; init; }
    public Board PlayerBoard { get; init; } = new();
    public Board AiBoard { get; init; } = new();
    public Guid CurrentPlayerId { get; set; }
    public Guid PlayerId { get; init; }
    public Guid AiId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
public sealed class Board
{
    public IReadOnlyList<Ship> Ships { get; init; } = [];
    public IReadOnlyList<Position> Shots { get; init; } = [];
    bool IsShot(Position position){return Shots.Contains(position);}
    bool HasShip(Position position){return Ships.Any(ship => ship.GetPositions().Contains(position));}
    bool IsHit(Position position){
        return Ships.Any(ship =>
            ship.GetPositions().Contains(position))
            && Shots.Contains(position);
    }
    bool IsMiss(Position position){
        return Shots.Contains(position)
            && !HasShip(position);
    }
    bool IsSunk(Ship ship){
        return ship.GetPositions()
            .All(position => Shots.Contains(position));
    }
    bool IsGameOver(){
        return Ships.All(IsSunk);
    }
}
public readonly record struct Position(int Row, int Column);
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
Donne moi l'implémentation de l'API avec les routes proposées et adapte les modèles ci-besoin.

- Réponse et hypothèses résumées :
Gemini nous a fourni une API simple dans un seul fichier (Program.cs). L'API utilisait également des fonctions helpers directement dans le même fichier, que nous avons plus tard transformés en services injectables.

- Décision et justification :
Nous avons gardé cette base car elle contenait les routes nécessaires à l'implémentation de notre frontend. Nous l'avons plus tard améliorée pour valider les entrées, utiliser gRPC-web et utiliser des services.

- Scénario ou commande de vérification :
Test des routes obtenues (lancement du projet + scénario avec Battleship.API.http)

- Résultat attendu, puis résultat observé :
| Résultat attendu (route) | Objectif | Résultat observé |
| :--- | :--- | :--- |
| **GET /** | Récupérer toutes les parties | Route correcte | 
| **POST /** | Créer une nouvelle partie | Route correcte | 
| **GET /{id:guid}** | Récupérer une partie | Route correcte | 
| **POST /{id:guid}/board** | Placer les bateaux du joueur | Route correcte | 
| **POST /{id:guid}/shots** | Tirer un coup (joueur) | Route correcte | 


Nous avons ajoutées certaines routes plus tard : 
    - **Delete /** pour supprimer toutes les parties ;
    - **Delete /{id:guid}** pour supprimer une parties.

- Erreur que ce contrôle pourrait détecter :
Si le résultat renvoyé n'est pas celui souhaité, si la route n'est pas fonctionnelle.

- Preuves reproductibles et limites :
Preuves reproductibles : Utilisation du fichier BattleShip.API.http (si ordre des routes respectées, le resultat est reproductible).

Limites : Le contrôle doit réalisé manuellement (pas automatisé).

## 1 : Diagnostic des échecs de tests bUnit (BaseAddress et SupplyParameterFromQuery)
- Date : 22 septembre 2026
- Sujet : Résolution des erreurs de rendu bUnit (WaitForFailedException et ArgumentException) dans GameTests.cs et HomeTests.cs.
- Outil / modèle si connu : Gemini Flash extended
- Contexte : Les tests Blazor avec RichardSzalay.MockHttp échouaient lors du dotnet test. Les composants restaient bloqués en chargement ou levaient une exception lors de la lecture des paramètres d'URL.
- Prompt réellement utilisé : « Test de BattleShip.Tests net10.0 : a échoué avec 3 erreur(s) ... [logs bUnit] Pourquoi cette erreur arrive ? »
- Réponse et hypothèses résumées :

HttpClient généré par MockHttp ne possédait pas de BaseAddress, faisant planter silencieusement les requêtes relatives dans OnInitializedAsync.

bUnit interdit le passage direct de paramètres décorés avec [SupplyParameterFromQuery] via parameters.Add().

L'utilisation de WaitForAssertion() est obligatoire pour attendre la fin du cycle de vie d'un composant asynchrone.

- Décision et justification :

Assigner httpClient.BaseAddress = new Uri("http://localhost/"); dans le constructeur des classes de test.

Utiliser NavigationManager.NavigateTo() pour simuler les Query Strings dans bUnit.

Remplacer les sélecteurs directs par des blocs cut.WaitForAssertion().

Scénario ou commande de vérification : Exécution de la suite de tests unitaires via dotnet test.

- Résultat attendu, puis résultat observé :

    - Attendu : Tous les tests bUnit passent au vert.

    - Observé : 0 erreur, exécution complète en moins de 2 secondes.

- Erreur que ce contrôle pourrait détecter : Des faux négatifs dans la chaîne d'intégration continue (CI/CD) causés par des mocks mal configurés ou des composants bloqués à l'initialisation.

- Preuves reproductibles et limites : Exécution de dotnet test sur le projet BattleShip.Tests. Limite : Vérifie uniquement le rendu UI en mémoire, pas la connectivité API réelle.