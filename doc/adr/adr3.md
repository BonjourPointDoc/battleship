# ADR 3 : Découpage des composants du plateau

## Statut et date

**Statut :** Accepté  
**Date :** 22/09/2026

Nous avons choisi de séparer l'affichage du plateau de jeu en deux composants Blazor : `Board.razor` pour le plateau et `Cell.razor` pour les cases individuelles.

## Contexte

Le plateau de BattleShip est constitué d'un ensemble de cases.

Une première possibilité aurait été de gérer directement l'ensemble du plateau dans un seul composant. Cependant, chaque case possède également son propre état visuel : case normale, case survolée, prévisualisation d'un bateau ou prévisualisation invalide.

Nous avons donc choisi de séparer les responsabilités entre plusieurs composants.

## Options envisagées

### Option 1 — Un seul composant pour le plateau

Le composant `Board` aurait été responsable de la génération du plateau et de toute la logique d'affichage des cases.

**Avantages :**

- moins de fichiers ;
- structure initiale plus simple ;
- logique regroupée dans un seul composant.

**Limites :**

- composant plus important ;
- gestion de l'affichage des cases mélangée avec celle du plateau ;
- évolution plus difficile si les cases ont davantage de comportements.

### Option 2 — Séparer `Board` et `Cell`

Le composant `Board` est responsable de la génération et de l'organisation du plateau.

Le composant `Cell` représente une case individuelle et gère son affichage.

La structure retenue est donc :

```text
Board
 ├── Cell
 ├── Cell
 ├── Cell
 ├── ...
 └── Cell
```

**Avantages :**

- responsabilités mieux séparées ;
- composant `Cell` réutilisable ;
- affichage d'une case centralisé ;
- code du plateau plus lisible ;
- ajout plus simple de nouveaux états visuels.

**Limites :**

- ajout d'un composant supplémentaire ;
- nécessité de transmettre certaines informations entre `Board` et `Cell`.

## Décision

Nous avons retenu **l'option 2 : séparer le plateau et les cases en deux composants**.

`Board.razor` est chargé de construire le plateau et de transmettre aux cellules les informations nécessaires.

`Cell.razor` est chargé de représenter une case et de déterminer son apparence en fonction de son état.

Cette séparation permet notamment de gérer les différents états visuels liés au placement des bateaux, comme la prévisualisation normale ou invalide.

## Conséquences

Cette organisation rend les composants plus simples et permet de limiter leurs responsabilités.

Le composant `Board` s'occupe principalement de la structure du plateau, tandis que `Cell` s'occupe de l'affichage d'une case.

Les styles sont également séparés :

```text
Board.razor
Board.razor.css

Cell.razor
Cell.razor.css
```

Cela permet de modifier l'apparence d'une cellule sans modifier directement la logique de construction du plateau.

En contrepartie, certaines informations doivent être transmises de `Board` vers `Cell`.

## Vérification et réexamen

La séparation est vérifiée directement dans l'application en affichant le plateau et en utilisant les différents états de placement.

Les cellules doivent notamment pouvoir afficher :

- une case normale ;
- une case survolée ;
- une prévisualisation de bateau ;
- une prévisualisation invalide.

La décision pourra être réexaminée si le plateau devient suffisamment complexe pour nécessiter une nouvelle séparation des responsabilités.

## Références

- `Board.razor` : génération du plateau et création des cellules.
- `Board.razor.css` : styles du plateau.
- `Cell.razor` : représentation d'une case.
- `Cell.razor.css` : styles et états visuels d'une case.