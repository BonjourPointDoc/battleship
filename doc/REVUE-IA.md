# Revues de propositions IA

Trois revues argumentées minimum. Aucune erreur n’est exigée ; chaque conclusion doit être étayée.

---

## Revue 1 — Modélisation des données

### Revue : sujet du projet

**Sujet :** Modélisation des données du jeu BattleShip.

### Proposition et référence dans le dépôt

L'IA a proposé un ensemble de modèles permettant de représenter les différents éléments du jeu : position, bateau, plateau, partie, joueur, attaques et états de jeu.

La première proposition comprenait notamment :

```text
Position.cs
Ship.cs
Board.cs
Game.cs
Player.cs
Attack.cs
AttackResult.cs
ShipPlacement.cs
GameState.cs
BoardView.cs
TurnResult.cs
```

ainsi que plusieurs énumérations :

```text
CellState.cs
Direction.cs
GameStatus.cs
PlayerType.cs
ShipType.cs
AttackResultType.cs
```

La proposition concernait la bibliothèque `BattleShip.Models`.

### Hypothèse à vérifier

Les modèles proposés doivent être cohérents avec le besoin fonctionnel du projet tout en restant suffisamment simples.

Le projet nécessite une API ASP.NET, un frontend Blazor WebAssembly et une bibliothèque de modèles partagée entre les projets.

### Scénario, données ou commande

Le projet a été initialisé avec :

```bash
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
```

Puis la demande suivante a été faite à l'IA :

> « Avec ce contexte, donne-moi les modèles nécessaires. »

### Résultat attendu avant exécution

Obtenir un ensemble de modèles de données cohérents avec le fonctionnement d'un jeu de bataille navale.

### Erreur que ce contrôle pourrait détecter

- Modèle incohérent avec les besoins du jeu.
- Données inutiles.
- Duplication d'informations.
- Modèle trop complexe.
- Erreur de syntaxe ou de compilation.

### Résultat réellement observé

La première proposition de l'IA fournissait un ensemble complet de modèles, mais celui-ci s'est révélé plus complexe que nécessaire.

Certaines informations pouvaient être calculées à partir d'autres données plutôt que stockées explicitement.

### Décision et justification

La proposition initiale n'a donc pas été conservée telle quelle.

Les modèles ont été simplifiés afin de ne conserver que les informations nécessaires au fonctionnement du jeu. Cette décision permet de réduire la quantité d'état stocké et d'éviter de dupliquer des informations calculables.

Le choix final a donc été de faire réviser les modèles proposés par l'IA plutôt que de les intégrer directement.

### Preuves reproductibles et liens vers les commits

Les modèles simplifiés sont présents dans le dépôt dans le commit :

https://gitlab.etu.mines-ales.fr/thomas.humbert/battleship/-/commit/456018eb4fcfba2b275e8618d9065e50abbcd8d1

Le commit contient les modèles simplifiés conservés dans le dépôt.

### Après correction éventuelle : résultat avant / après

**Avant :**

Un ensemble de modèles assez important était proposé, avec notamment `Player`, `Attack`, `AttackResult`, `ShipPlacement`, `GameState`, `BoardView` et `TurnResult`.

**Après :**

Les modèles ont été simplifiés pour éviter de stocker des informations pouvant être déduites à partir des données existantes.

### Limites et points non vérifiés

Cette revue porte principalement sur la cohérence des modèles.

Elle ne vérifie pas le comportement complet du jeu, la communication frontend/backend ou la persistance des parties.

---

## Revue 2 — Architecture du placement des bateaux

### Revue : sujet du projet

**Sujet :** Représentation d'un bateau et calcul des cases occupées.

### Proposition et référence dans le dépôt

L'IA a proposé de représenter un bateau avec :

```csharp
public sealed record Ship(
    ShipType Type,
    Position Position,
    Direction Direction);
```

Les cases occupées par le bateau sont ensuite calculées à partir de son type, de sa position et de sa direction avec `GetPositions()`.

Les éléments concernés sont :

```text
BattleShip.Models/Ship.cs
BattleShip.Models/Position.cs
BattleShip.Models/Direction.cs
BattleShip.Models/ShipType.cs
BattleShip.Models/ShipExtensions.cs
```

### Hypothèse à vérifier

La position de départ et la direction doivent suffire pour déterminer toutes les cases occupées par un bateau.

Il ne doit donc pas être nécessaire de stocker individuellement chaque case occupée.

### Scénario, données ou commande

Tester les différents types de bateaux et leurs tailles :

```text
Carrier      → 5 cases
Battleship   → 4 cases
Cruiser      → 3 cases
Submarine    → 3 cases
Destroyer    → 2 cases
```

Tester également une même position de départ avec les deux directions :

```text
Position : (2, 3)
Direction : Horizontal
```

puis :

```text
Position : (2, 3)
Direction : Vertical
```

### Résultat attendu avant exécution

Pour un Carrier horizontal en `(2,3)`, les positions attendues sont :

```text
(2,3)
(2,4)
(2,5)
(2,6)
(2,7)
```

Pour le même Carrier vertical :

```text
(2,3)
(3,3)
(4,3)
(5,3)
(6,3)
```

### Erreur que ce contrôle pourrait détecter

- Mauvais calcul de la taille du bateau.
- Inversion entre ligne et colonne.
- Mauvaise gestion de la direction.
- Nombre incorrect de cases occupées.

### Résultat réellement observé

La représentation permet de calculer les positions occupées à partir du type, de la position et de la direction.

Aucune correction n'a été nécessaire sur cette proposition.

### Décision et justification

**Proposition conservée.**

Cette représentation permet de conserver uniquement les informations nécessaires à la définition d'un bateau et de calculer ses cases occupées lorsque cela est nécessaire.

Elle est également réutilisable pour le placement et pour les contrôles de collision.

### Preuves reproductibles et liens vers les commits

Les fichiers concernés sont :

```text
BattleShip.Models/Ship.cs
BattleShip.Models/Position.cs
BattleShip.Models/Direction.cs
BattleShip.Models/ShipType.cs
BattleShip.Models/ShipExtensions.cs
```

La vérification peut être reproduite en utilisant différentes positions et directions et en contrôlant les positions retournées par `GetPositions()`.

**Commit :** à renseigner avec le hash du commit contenant ces modèles.

### Après correction éventuelle : résultat avant / après

Aucune correction nécessaire.

**Avant :** proposition de l'IA.

**Après :** proposition intégrée dans le projet et vérifiée.

### Limites et points non vérifiés

Cette revue ne vérifie pas :

- la sérialisation des modèles ;
- leur utilisation par l'API ;
- leur persistance ;
- la logique de tir.

---

## Revue 3 — Validation du placement des bateaux

### Revue : sujet du projet

**Sujet :** Vérification qu'un bateau ne dépasse pas du plateau et ne chevauche pas un autre bateau.

### Proposition et référence dans le dépôt

L'IA a proposé deux contrôles dans `BattleShip.App/Pages/Game.razor`.

Le premier vérifie qu'une position reste dans le plateau :

```csharp
private bool IsInsideBoard(
    IEnumerable<Position> positions)
{
    return positions.All(position =>
        position.Row >= 0 &&
        position.Row < 10 &&
        position.Column >= 0 &&
        position.Column < 10);
}
```

Le second vérifie le chevauchement :

```csharp
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

**Référence :**

```text
BattleShip.App/Pages/Game.razor
```

### Hypothèse à vérifier

Le joueur ne doit jamais pouvoir placer un bateau :

- en dehors des 10×10 cases ;
- sur une case déjà occupée par un autre bateau.

### Scénario, données ou commande

Tester les situations suivantes.

#### Cas 1 — position valide

```text
Carrier
Position : (0,0)
Direction : Horizontal
```

Le bateau occupe les cases :

```text
(0,0) → (0,4)
```

#### Cas 2 — dépassement du plateau

```text
Carrier
Position : (0,7)
Direction : Horizontal
```

Le bateau dépasserait du plateau.

#### Cas 3 — chevauchement

Placer un Carrier :

```text
(0,0) → (0,4)
```

Puis essayer de placer un Destroyer :

```text
(0,3) → (0,4)
```

### Résultat attendu avant exécution

| Cas | Résultat attendu |
|---|---|
| Position valide | Placement accepté |
| Dépassement du plateau | Placement refusé |
| Chevauchement | Placement refusé |

### Erreur que ce contrôle pourrait détecter

- Bateau dépassant du plateau.
- Deux bateaux occupant une même case.
- Validation ne prenant en compte que la position de départ du bateau.

### Résultat réellement observé

Les contrôles permettent de refuser les positions invalides.

Une amélioration a également été apportée à l'interface : une position invalide est maintenant représentée visuellement en rouge avant la validation du placement.

### Décision et justification

**Proposition conservée.**

Les deux contrôles couvrent les deux contraintes principales du placement :

```text
IsInsideBoard()
        +
IsOverlapping()
```

Cette séparation permet également de réutiliser la validation pour déterminer si l'aperçu du bateau est valide.

### Preuves reproductibles et liens vers les commits

Le contrôle peut être reproduit depuis `/game` :

1. Placer un bateau dans une position valide.
2. Tester une position qui dépasse du plateau.
3. Tester une position qui chevauche un bateau existant.
4. Vérifier que seules les positions valides peuvent être placées.

**Commit :** à renseigner avec le hash réel du commit correspondant.

### Après correction éventuelle : résultat avant / après

**Avant :**

Une position invalide était simplement refusée lors du clic.

**Après :**

La position invalide est également signalée visuellement en rouge avant le clic.

### Limites et points non vérifiés

Cette vérification ne contrôle pas :

- une éventuelle distance minimale entre les bateaux ;
- les tirs ;
- la validation côté serveur ;
- les données provenant de l'API.

Ces éléments sont hors du périmètre de cette revue.

---

## Revue 4 — Gestion de l'état après le placement

### Revue : sujet du projet

**Sujet :** Passage de la phase de placement à la phase de jeu.

### Proposition et référence dans le dépôt

L'IA a proposé d'introduire un état explicite :

```csharp
private bool PlacementFinished;
```

dans `BattleShip.App/Pages/Game.razor`.

Après le placement du dernier bateau :

```csharp
PlacementFinished = true;
DisplayStartingPlayerPopup(true);
```

Les actions de placement vérifient ensuite cet état :

```csharp
if (PlacementFinished)
    return;
```

### Hypothèse à vérifier

Après le placement du cinquième bateau, le joueur ne doit plus pouvoir modifier sa flotte.

La fermeture du popup indiquant qui commence ne doit pas permettre de recommencer le placement.

### Scénario, données ou commande

Effectuer le scénario suivant :

```text
1. Démarrer une nouvelle partie.
2. Placer Carrier.
3. Placer Battleship.
4. Placer Cruiser.
5. Placer Submarine.
6. Placer Destroyer.
7. Vérifier l'apparition du popup.
8. Fermer le popup.
9. Cliquer sur plusieurs cases.
10. Effectuer plusieurs clics droits.
```

### Résultat attendu avant exécution

Après le placement du Destroyer :

```text
5 bateaux placés
        ↓
Popup
        ↓
Fermeture du popup
        ↓
Aucune modification de la flotte
```

### Erreur que ce contrôle pourrait détecter

- Placement d'un sixième bateau.
- Repositionnement du dernier bateau.
- Réouverture du popup.
- Modification de l'orientation après la fin du placement.
- Modification accidentelle de la flotte pendant la partie.

### Résultat réellement observé

Après l'ajout de `PlacementFinished`, les clics et clics droits effectués après la fin du placement n'entraînent plus de modification des bateaux.

Le popup ne peut donc plus être déclenché une deuxième fois par un nouveau placement.

### Décision et justification

**Proposition conservée.**

L'utilisation d'un état explicite est plus claire que de déduire la fin du placement uniquement à partir de `CurrentShipIndex`.

Elle permet de distinguer les différentes phases :

```text
Placement
    ↓
PlacementFinished
    ↓
Popup
    ↓
GameStarted
    ↓
Tirs
```

### Preuves reproductibles et liens vers les commits

Le scénario peut être reproduit depuis la page `/game`.

Une vérification complémentaire peut être effectuée avec :

```bash
dotnet build
```

puis :

```bash
dotnet test
```

**Commit :** à renseigner avec le hash réel du commit contenant la correction.

### Après correction éventuelle : résultat avant / après

**Avant :**

Après avoir fermé le popup, le joueur pouvait replacer le dernier bateau.

La cause était que :

```text
CurrentShipIndex == 4
ShipTypes.Length == 5
```

Donc :

```text
CurrentShipIndex >= ShipTypes.Length
```

était faux.

**Après :**

Un état explicite `PlacementFinished` bloque définitivement les actions de placement après le cinquième bateau.

### Limites et points non vérifiés

Cette revue ne vérifie pas encore :

- la persistance de l'état de la partie ;
- la reprise d'une partie depuis la page d'accueil ;
- la synchronisation avec le serveur ;
- le tour réel du joueur ;
- les tirs de l'IA.

---

## Bilan des revues

Les quatre revues couvrent des décisions différentes du projet :

| Revue | Sujet | Conclusion |
|---|---|---|
| 1 | Modélisation des données | Proposition initiale simplifiée |
| 2 | Représentation des bateaux | Proposition conservée |
| 3 | Validation du placement | Proposition conservée + amélioration visuelle |
| 4 | Gestion de la fin du placement | Proposition conservée après correction d'un bug |

Ces revues montrent que les propositions de l'IA ont été analysées, vérifiées et parfois modifiées, plutôt que simplement intégrées telles quelles.

Chaque revue associe une proposition à une hypothèse, un scénario de vérification, un résultat observé et une décision justifiée.