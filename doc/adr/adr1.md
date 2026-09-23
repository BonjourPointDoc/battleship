# ADR 1 : Architecture initiale

## Statut et date

**Statut :** Rejeté  
**Date :** 19/09/2026

Nous avons étudié une première proposition d'architecture des modèles générée par l'IA. Cette proposition a finalement été rejetée car nous souhaitions conserver une architecture plus simple et éviter de stocker en mémoire des informations qui pouvaient être calculées à partir d'autres données.

## Contexte

Au début du projet, nous avons demandé à l'IA de proposer les modèles nécessaires pour représenter le fonctionnement du jeu BattleShip.

La première proposition était relativement complète et comprenait notamment des modèles pour représenter :

- les positions ;
- les bateaux ;
- le plateau ;
- la partie ;
- les joueurs ;
- les attaques ;
- les résultats des attaques ;
- les placements ;
- les différents états de la partie.

L'IA proposait notamment des modèles tels que :

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

ainsi que plusieurs énumérations permettant de représenter les différents états du jeu.

Après analyse, nous avons considéré que cette architecture stockait trop d'informations. Certaines données pouvaient être déduites ou calculées à partir d'informations déjà présentes dans les modèles.

Par exemple, pour un bateau, il n'était pas nécessaire de stocker en mémoire l'ensemble des cases qu'il occupe. Sa position de départ, sa direction et son type suffisent pour calculer ses différentes positions.

## Options envisagées

### Option 1 — Conserver tous les modèles proposés par l'IA

Cette option consistait à conserver l'ensemble des modèles proposés afin de représenter explicitement chaque élément du jeu.

**Avantages :**

- représentation très détaillée du jeu ;
- informations directement accessibles ;
- séparation explicite des différentes notions du domaine.

**Limites :**

- architecture plus complexe ;
- davantage de données stockées en mémoire ;
- risque de duplication d'informations ;
- certaines informations peuvent être calculées à partir d'autres données.

### Option 2 — Simplifier les modèles et calculer les informations dérivables

Cette option consiste à conserver uniquement les informations nécessaires et à calculer les données pouvant être déduites.

Par exemple, un bateau est représenté avec :

```csharp
public sealed record Ship(
    ShipType Type,
    Position Position,
    Direction Direction);
```

Les positions occupées sont ensuite calculées à partir de ces trois informations.

**Avantages :**

- modèle plus simple ;
- moins de données stockées ;
- absence de duplication ;
- cohérence entre les informations stockées et les informations calculées.

**Limites :**

- certaines informations doivent être recalculées lorsqu'elles sont nécessaires ;
- la logique de calcul doit être correctement implémentée.

## Décision

Nous avons retenu **l'option 2 : simplifier les modèles et calculer les informations dérivables**.

Nous avons donc supprimé de la conception initiale les informations qui pouvaient être calculées à partir des données existantes.

Pour les bateaux, nous avons notamment retenu le principe suivant :

```text
Type + Position + Direction
            ↓
     Positions occupées
```

Les positions occupées par un bateau ne sont donc pas stockées directement en mémoire.

Elles sont calculées à partir du type du bateau, de sa position de départ et de sa direction.

Cette décision permet de garder une architecture plus simple et d'éviter de maintenir plusieurs représentations d'une même information.

## Conséquences

Cette décision entraîne plusieurs conséquences positives :

- réduction du nombre de modèles nécessaires ;
- réduction des informations stockées en mémoire ;
- diminution du risque d'incohérence entre des données stockées plusieurs fois ;
- logique métier plus centralisée ;
- modèles plus simples à utiliser entre le frontend et le backend.

En contrepartie, les positions occupées d'un bateau doivent être recalculées lorsqu'elles sont nécessaires.

Cette approche implique donc de disposer d'une méthode permettant de calculer ces positions à partir du bateau.

## Vérification et réexamen

La décision a été vérifiée en utilisant les modèles simplifiés dans le projet et en calculant les positions occupées des bateaux à partir de leur type, leur position et leur direction.

Le principe retenu permet notamment de déterminer les cases occupées sans avoir à les conserver directement dans le modèle.

La décision pourra être réexaminée si les besoins du projet évoluent et nécessitent de conserver explicitement des informations supplémentaires qui ne peuvent plus être calculées simplement.

## Références

Commit contenant les modèles simplifiés :

https://gitlab.etu.mines-ales.fr/thomas.humbert/battleship/-/commit/456018eb4fcfba2b275e8618d9065e50abbcd8d1