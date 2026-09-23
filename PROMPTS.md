# Échanges décisifs avec l’IA

---

# 1. Génération initiale de l'API

## Date et sujet

- **Date :** 19/09/2026
- **Sujet :** Génération initiale de l'API
- **Outil / modèle :** Gemini 3.6 Flash Extended

## Contexte

À partir des modèles créés précédemment, nous avons demandé à l'IA de générer l'API.

Ce prompt a été réalisé dans le même chat que celui ayant permis de définir l'architecture et de fournir le diagramme fonctionnel. L'IA disposait donc de ce contexte pour proposer l'implémentation.

Les modèles utilisés étaient notamment les suivants :

```csharp
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

    bool IsShot(Position position)
    {
        return Shots.Contains(position);
    }

    bool HasShip(Position position)
    {
        return Ships.Any(
            ship => ship.GetPositions().Contains(position));
    }

    bool IsHit(Position position)
    {
        return Ships.Any(
            ship => ship.GetPositions().Contains(position))
            && Shots.Contains(position);
    }

    bool IsMiss(Position position)
    {
        return Shots.Contains(position)
            && !HasShip(position);
    }

    bool IsSunk(Ship ship)
    {
        return ship.GetPositions()
            .All(position => Shots.Contains(position));
    }

    bool IsGameOver()
    {
        return Ships.All(IsSunk);
    }
}

public readonly record struct Position(int Row, int Column);

public sealed record Ship(
    ShipType Type,
    Position Position,
    Direction Direction);
```

Les positions occupées par un bateau étaient calculées avec `GetPositions()` à partir de son type, de sa position et de sa direction.

## Prompt réellement utilisé

```text
L'API doit être une API ASP.NET Core en Minimal API (.NET 10).

Nous avons une bibliothèque de modèles avec ces modèles qui sera utilisée par le frontend et le backend :

[modèles Game, Board, Position, Ship et ShipExtensions]

Donne moi l'implémentation de l'API avec les routes proposées et adapte les modèles si besoin.
```

## Réponse et hypothèses résumées

Gemini a fourni une API simple dans un seul fichier `Program.cs`.

L'API utilisait également des fonctions helpers directement dans ce fichier. Ces fonctions ont ensuite été transformées en services injectables.

## Décision et justification

La base proposée par l'IA a été conservée car elle contenait les routes nécessaires à l'implémentation du frontend.

Cette base a ensuite été améliorée afin :

- de valider les entrées ;
- d'utiliser gRPC-Web ;
- d'utiliser des services injectables.

## Scénario ou commande de vérification

Les routes générées ont été testées en lançant le projet puis en utilisant le fichier :

```text
BattleShip.API.http
```

## Résultat attendu

Les routes proposées doivent être fonctionnelles et permettre les opérations principales nécessaires au frontend.

| Route | Objectif |
|---|---|
| **GET /** | Récupérer toutes les parties |
| **POST /** | Créer une nouvelle partie |
| **GET /{id:guid}** | Récupérer une partie |
| **POST /{id:guid}/board** | Placer les bateaux du joueur |
| **POST /{id:guid}/shots** | Tirer un coup (joueur) |

## Résultat observé

Les routes principales étaient fonctionnelles :

| Route | Résultat observé |
|---|---|
| **GET /** | Route correcte |
| **POST /** | Route correcte |
| **GET /{id:guid}** | Route correcte |
| **POST /{id:guid}/board** | Route correcte |
| **POST /{id:guid}/shots** | Route correcte |

Certaines routes ont été ajoutées plus tard :

- **DELETE /** pour supprimer toutes les parties ;
- **DELETE /{id:guid}** pour supprimer une partie.

## Erreur que ce contrôle pourrait détecter

Ce contrôle permet de détecter :

- une route incorrecte ;
- une route inaccessible ;
- un résultat différent de celui attendu ;
- un comportement non fonctionnel lors de l'appel d'une route.

## Preuves reproductibles

Le contrôle peut être reproduit avec le fichier :

```text
BattleShip.API.http
```

En respectant l'ordre des requêtes, le scénario peut être reproduit manuellement.

## Limites

Le contrôle doit être réalisé manuellement.

Il ne constitue pas un test automatisé des routes.

---

# 2. Diagnostic des échecs de tests bUnit

## Date et sujet

- **Date :** 22/09/2026
- **Sujet :** Résolution des erreurs de rendu bUnit dans `GameTests.cs` et `HomeTests.cs`
- **Outil / modèle :** Gemini Flash Extended

## Contexte

Les tests Blazor utilisant `RichardSzalay.MockHttp` échouaient lors de l'exécution de `dotnet test`.

Les composants restaient bloqués pendant leur chargement ou levaient une exception lors de la lecture des paramètres d'URL.

Les erreurs concernaient notamment :

- `WaitForFailedException` ;
- `ArgumentException`.

## Prompt réellement utilisé

```text
Test de BattleShip.Tests net10.0 : a échoué avec 3 erreur(s) ... [logs bUnit]

Pourquoi cette erreur arrive ?
```

## Réponse et hypothèses résumées

L'IA a identifié plusieurs causes.

### 1. Absence de `BaseAddress`

Le `HttpClient` généré par `MockHttp` ne possédait pas de `BaseAddress`.

Les requêtes relatives effectuées dans `OnInitializedAsync` pouvaient donc échouer.

### 2. Utilisation de `[SupplyParameterFromQuery]`

bUnit interdit le passage direct de paramètres décorés avec `[SupplyParameterFromQuery]` via `parameters.Add()`.

### 3. Cycle de vie asynchrone

L'utilisation de `WaitForAssertion()` est nécessaire afin d'attendre la fin du cycle de vie asynchrone du composant.

## Décision et justification

Les corrections suivantes ont été appliquées :

```csharp
httpClient.BaseAddress = new Uri("http://localhost/");
```

Cette configuration a été ajoutée dans le constructeur des classes de test.

Pour les paramètres présents dans la query string, `NavigationManager.NavigateTo()` est utilisé afin de simuler les paramètres d'URL dans bUnit.

Enfin, les sélecteurs directs ont été remplacés par des blocs :

```csharp
cut.WaitForAssertion(() =>
{
    // vérification
});
```

Cette approche permet d'attendre que le composant ait terminé son initialisation asynchrone avant d'effectuer les vérifications.

## Scénario ou commande de vérification

Exécution de la suite de tests :

```bash
dotnet test
```

## Résultat attendu

Tous les tests bUnit doivent passer sans erreur.

## Résultat observé

Après les corrections :

```text
0 erreur
```

La suite de tests s'est exécutée complètement en moins de deux secondes.

## Erreur que ce contrôle pourrait détecter

Ce contrôle peut détecter des problèmes dans la chaîne d'intégration continue (CI/CD) causés notamment par :

- des mocks mal configurés ;
- des composants bloqués lors de leur initialisation ;
- des paramètres d'URL mal simulés ;
- des vérifications effectuées avant la fin du cycle de vie asynchrone.

## Preuves reproductibles

Le contrôle peut être reproduit avec :

```bash
dotnet test
```

sur le projet :

```text
BattleShip.Tests
```

## Limites

Les tests vérifient le rendu et le comportement des composants en mémoire.

Ils ne vérifient pas la connectivité réelle avec l'API.

---

# 3. Détection d'un bug après fermeture du popup

## Date et sujet

- **Date :** 22/09/2026
- **Sujet :** Blocage du placement après la fin du placement initial
- **Outil / modèle :** ChatGPT — GPT-5.6 Luna

## Contexte

Après avoir placé les cinq bateaux, un popup indique quel joueur commence la partie.

Après avoir fermé le popup, le joueur pouvait cependant cliquer à nouveau sur le plateau et replacer le dernier bateau.

Cela pouvait également provoquer la réouverture du popup.

## Prompt réellement utilisé

```text
il y a un bug, lorsque je referme le pop-up je peux placer à nouveau un dernier bateau.

Ce qui réouvre un pop-up qui me redonne la possibilité de replacer un bateau après l'avoir fermé
```

## Réponse et hypothèses résumées

L'IA a analysé l'état `CurrentShipIndex`.

Après le placement du dernier bateau, sa valeur était encore :

```text
CurrentShipIndex == 4
```

La condition suivante restait donc fausse :

```csharp
if (CurrentShipIndex >= ShipTypes.Length)
```

En effet :

```text
4 >= 5
```

renvoie :

```text
false
```

Le dernier bateau pouvait donc être replacé.

L'IA a proposé d'ajouter un état explicite :

```csharp
private bool PlacementFinished;
```

Puis de bloquer les interactions de placement lorsque cet état est vrai.

## Décision et justification

Un état explicite a été ajouté afin de distinguer clairement la fin de la phase de placement du simple indice du bateau courant.

Après le placement du dernier bateau :

```csharp
PlacementFinished = true;
```

Les actions de placement sont ensuite bloquées lorsque :

```csharp
if (PlacementFinished)
    return;
```

Cette solution empêche la modification de la flotte après sa validation.

## Scénario ou commande de vérification

Le scénario suivant a été utilisé :

1. Placer les quatre premiers bateaux.
2. Placer le Destroyer.
3. Vérifier l'apparition du popup.
4. Fermer le popup.
5. Cliquer plusieurs fois sur différentes cases.
6. Effectuer des clics droits.
7. Vérifier qu'aucun nouveau bateau n'est créé.

## Résultat attendu

Après la fermeture du popup, aucun nouveau placement ne doit être possible.

Les cinq bateaux doivent rester inchangés.

## Résultat observé

Après correction, les clics effectués après la fermeture du popup sont ignorés pour le placement.

Les cinq bateaux restent inchangés.

Le popup ne peut donc plus être déclenché une nouvelle fois par un placement supplémentaire.

## Erreur que ce contrôle pourrait détecter

Ce contrôle permet notamment de détecter :

- la duplication du dernier bateau ;
- la réouverture répétée du popup ;
- la modification accidentelle de la flotte après sa validation ;
- un état du jeu incohérent après la phase de placement.

## Preuves reproductibles

Le scénario peut être reproduit depuis l'interface utilisateur de la page de jeu.

## Limites

Ce contrôle ne teste pas encore les transitions de partie provenant du serveur.