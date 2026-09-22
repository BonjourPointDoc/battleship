# Revues de propositions IA

Trois revues argumentées minimum. Aucune erreur n’est exigée ; chaque conclusion doit être étayée.

## Revue : sujet du projet

- Proposition et référence dans le dépôt : BattleShip.Model
- Hypothèse à vérifier : Les modèles de données proposées sont simples et cohérents avec notre diagramme fonctionnel.
Ils correspondent au besoin.
- Scénario, données ou commande :
Nous voulons créer un jeu de bataille navale en dotnet (C#).
Il nous faut une API en ASP.NET version 10, un front utilisant Blazer WebAssembly et
une bibliothèque de modèle échangée entre les projets.
Les échanges entre le frontend et backend se font en gRPC-web. Il faut aussi utiliser 
FluentValidation sur les entrées du serveur. J'ai init le projet avec
 
dotnet --version

dotnet new sln -n BattleShip
dotnet new webapi     -n BattleShip.API
dotnet new blazorwasm -n BattleShip.App
dotnet new classlib   -n BattleShip.Models
dotnet new xunit      -n BattleShip.Tests

dotnet sln add BattleShip.API BattleShip.App BattleShip.Models BattleShip.Tests
dotnet add BattleShip.API   reference BattleShip.Models
dotnet add BattleShip.App   reference BattleShip.Models
dotnet add BattleShip.Tests reference BattleShip.API

dotnet build
dotnet test

Avec ce contexte, donne-moi les modèles nécessaires.

- Résultat attendu avant exécution :
Un ensemble de modèles de données.
- Erreur que ce contrôle pourrait détecter :
Modèle non cohérent, variables inutiles, syntaxe fausse.

- Résultat réellement observé :
Modèles proposés :
├── Position.cs
├── Ship.cs
├── Board.cs
├── Game.cs
├── Player.cs
├── Attack.cs
├── AttackResult.cs
├── ShipPlacement.cs
├── GameState.cs
├── BoardView.cs
├── TurnResult.cs
│
└── Enums
    ├── CellState.cs
    ├── Direction.cs
    ├── GameStatus.cs
    ├── PlayerType.cs
    ├── ShipType.cs
    └── AttackResultType.cs

- Décision et justification :
L'ensemble de modèles de données fournit était cohérent, mais plus complexe que nécessaire.
Nous avons donc décidé de ne pas les conserver et de les faire réviser à l'IA pour les simplifier.
Beaucoup des données pouvaient être calculées à la volée au lieu d'être stockées en mémoire (ex: Player -> l'état du joeur peut être déterminé à partir de l'historique (ses 5 bateaux ont l'état "coulé" plutôt que stockés)).

- Preuves reproductibles et liens vers les commits :
Nous n'avons commit que les modèles simplifiés :
https://gitlab.etu.mines-ales.fr/thomas.humbert/battleship/-/commit/456018eb4fcfba2b275e8618d9065e50abbcd8d1

- Après correction éventuelle : résultat avant / après :
Voir le commit (ne contient que les modèles)

- Limites et points non vérifiés :
Non applicable.

## 1 : Architecture du placement des bateaux
Sujet : Représentation d'un bateau et calcul des cases occupées.

L'IA a proposé de représenter un bateau avec :
```cs
public sealed record Ship(
    ShipType Type,
    Position Position,
    Direction Direction);
```
et de calculer ses cases occupées à partir de sa taille et de sa direction avec GetPositions().
Références dans le dépôt :
```
BattleShip.Models/Ship.cs
BattleShip.Models/Position.cs
BattleShip.Models/Direction.cs
BattleShip.Models/ShipType.cs
BattleShip.Models/ShipExtensions.cs
```
Hypothèse à vérifier : La position de départ et la direction doivent permettre de déterminer exactement toutes les cases occupées par le bateau, sans avoir à stocker individuellement chaque case.

Erreur que ce contrôle pourrait détecter :
- Mauvais calcul de la taille du bateau.
- Inversion de ligne et colonne.
- Mauvaise prise en compte de la direction.
- Nombre incorrect de cases générées.

Résultat avant / après :
> Aucune correction nécessaire.
Avant : proposition de l'IA.
Après : modèle intégré et vérifié dans le projet.

## 2 : Validation du placement des bateaux

Sujet : Vérification qu'un bateau ne dépasse pas du plateau et ne chevauche pas un autre bateau.
L'IA a proposé deux contrôles dans Game.razor :
```cs
private bool IsInsideBoard(
    IEnumerable<Position> positions)
{
    return positions.All(position =>
        position.Row >= 0 &&
        position.Row < 10 &&
        position.Column >= 0 &&
        position.Column < 10);
}

private bool IsOverlapping(
    IEnumerable<Position> positions)
{
    var occupiedPositions =
        PlacedShips
            .SelectMany(ship => ship.GetPositions())
            .ToHashSet();

    return positions.Any(occupiedPositions.Contains);
}
```
Référence : `BattleShip.App/Pages/Game.razor`
Hypothèse à vérifier :
Le joueur ne doit jamais pouvoir placer un bateau :
- en dehors des 10×10 cases.
- sur une case déjà occupée par un autre bateau.

Scénario, données ou commande:

Tester les situations suivantes.
Cas 1 - position valide :
```
Carrier
Position : (0,0)
Direction : Horizontal
```
Résultat attendu : Valide
Cas 2 - dépassement :
```
Carrier
Position : (0,7)
Direction : Horizontal
```
Résultat attendu : Erreur
Cas 3 - chevauchement :
Placer un Carrier sur : `(0,0) → (0,4)`
Puis essayer de placer un Destroyer sur : `(0,3) → (0,4)`
Résultat attendu : Erreur


Résultat avant / après :
Aucune correction obligatoire.
Avant :
- Une position invalide était simplement refusée au clic.
Après :
- La position invalide est également signalée visuellement en rouge avant le clic.

Cette vérification ne contrôle pas :

une distance minimale entre les bateaux, car cette règle n'a pas été retenue ;
les tirs ;
la validation côté serveur ;
les données provenant de l'API.

