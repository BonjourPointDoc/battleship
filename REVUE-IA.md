# Revues de propositions IA

Trois revues argumentées minimum. Aucune erreur n’est exigée ; chaque conclusion doit être étayée.

## Revue : sujet du projet

- Proposition et référence dans le dépôt :
- Hypothèse à vérifier :
- Scénario, données ou commande :
- Résultat attendu avant exécution :
- Erreur que ce contrôle pourrait détecter :
- Résultat réellement observé :
- Décision et justification :
- Preuves reproductibles et liens vers les commits :
- Après correction éventuelle : résultat avant / après :
- Limites et points non vérifiés :


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