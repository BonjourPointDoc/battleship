# ADR 2 : Prévisualisation du placement des bateaux

## Statut et date

**Statut :** Accepté  
**Date :** 22/09/2026

Nous avons choisi d'ajouter une prévisualisation du bateau lors du déplacement de la souris afin que le joueur puisse visualiser son placement avant de cliquer.

## Contexte

Lors du placement des bateaux, le joueur doit pouvoir choisir la position et la direction de chaque bateau.

Une validation uniquement après le clic ne permet pas au joueur de savoir à l'avance si le placement choisi est valide.

Nous avons donc voulu fournir un retour visuel avant la validation du placement.

## Options envisagées

### Option 1 — Ne rien afficher avant le clic

Le bateau n'est affiché qu'après avoir été placé.

**Avantages :**

- implémentation plus simple ;
- moins de logique d'affichage.

**Limites :**

- le joueur ne sait pas si le placement est valide avant de cliquer ;
- les erreurs de placement sont moins visibles ;
- l'expérience utilisateur est moins intuitive.

### Option 2 — Afficher une prévisualisation du bateau

Le bateau est affiché lorsque le joueur déplace sa souris sur une case.

La prévisualisation utilise les mêmes règles que le placement réel afin de déterminer si la position est valide.

Un placement valide est affiché normalement tandis qu'un placement invalide est affiché en rouge.

**Avantages :**

- retour visuel immédiat ;
- le joueur peut anticiper les erreurs ;
- les règles de validation sont réutilisées ;
- le placement est plus intuitif.

**Limites :**

- nécessite de gérer l'état de survol ;
- nécessite de calculer les positions du bateau avant son placement ;
- ajoute une logique supplémentaire dans le composant d'affichage.

## Décision

Nous avons retenu **l'option 2 : afficher une prévisualisation du bateau**.

Lorsqu'une case est survolée, les positions que le bateau occuperait sont calculées.

La validité de la prévisualisation est ensuite vérifiée en utilisant les mêmes règles que pour le placement :

- le bateau doit rester dans les limites du plateau ;
- le bateau ne doit pas chevaucher un autre bateau.

Une propriété permet de déterminer si la prévisualisation est valide :

```csharp
private bool IsPreviewValid
{
    get
    {
        if (PlacementFinished || HoveredPosition is null)
            return false;

        var positions = PreviewShipPositions;

        return IsInsideBoard(positions)
            && !IsOverlapping(positions);
    }
}
```

Lorsque le placement est invalide, la case est affichée en rouge afin de fournir immédiatement un retour au joueur.

## Conséquences

Cette décision améliore la lisibilité du placement et permet au joueur de détecter une position incorrecte avant de cliquer.

Elle permet également de réutiliser les règles de validation déjà présentes dans le jeu au lieu de créer une logique différente uniquement pour l'affichage.

En contrepartie, le frontend doit gérer l'état de survol et recalculer les positions du bateau pendant le déplacement de la souris.

## Vérification et réexamen

La fonctionnalité peut être vérifiée manuellement en déplaçant la souris sur différentes zones du plateau.

Les scénarios vérifiés sont notamment :

- survol d'une position valide → prévisualisation normale ;
- survol d'une position qui dépasse du plateau → prévisualisation rouge ;
- survol d'une position qui chevauche un bateau existant → prévisualisation rouge ;
- changement de direction → prévisualisation recalculée ;
- après la fin du placement → aucune nouvelle prévisualisation.

La décision pourra être réexaminée si le système de placement évolue ou si une autre représentation visuelle devient nécessaire.

## Références

- `Game.razor` : calcul et validation de la prévisualisation.
- `Board.razor` : transmission de l'état de validité aux cellules.
- `Cell.razor` : affichage de la prévisualisation.
- `Cell.razor.css` : affichage différent d'une prévisualisation invalide.