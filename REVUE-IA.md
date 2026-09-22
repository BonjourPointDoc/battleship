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