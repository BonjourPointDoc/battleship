# Contexte du projet

## Contraintes du cours

- .NET 10 stable ; vérifier global.json et dotnet --version.
- API ASP.NET Core Minimal API, Blazor WebAssembly, modèles partagés, tests.
- FluentValidation sur les entrées serveur ; au moins un échange gRPC-Web fonctionnel depuis le navigateur.
- Règles vérifiées côté serveur ; informations adverses cachées préservées.
- Projet attendu au-delà du socle : ambition, pertinence et qualité des extensions évaluées.
- Résultat compris et défendable par chaque membre du binôme.

## À compléter par le binôme

- Vision du projet, règles et expérience visée :
L'objectif est la réalisation d'une bataille navale en ligne respectant les règles d'origine et intégrant un adversaire autonome.
- Backlog, priorités et périmètre retenu :
**perimetre retenu**:
* Règles & Gameplay : Implémentation complète et fidèle des règles classiques de la bataille navale.
* Intelligence Artificielle : Adversaire autonome intégrant une logique de tir ciblée (IA non purement aléatoire).
* Gestion des parties : Sauvegarde automatique et chargement des parties en cours.
* Suivi & Historique : Consultation de l'historique et des résultats des parties précédentes.

Expérience Utilisateur : Direction artistique unifiée et cohérente sur l'ensemble du jeu.
- Organisation du code et contrats :
- Commandes, ports et environnement :
| Service | Dossier | Commande | URL / Port |
| :--- | :--- | :--- | :--- |
| **Tests** | `BattleShip.Tests` | `dotnet test` | — |
| **API** | `BattleShip.API` | `dotnet run` | `http://localhost:5120` |
| **App Web** | `BattleShip.App` | `dotnet run` | `http://localhost:5229` |

- Conventions et méthode de collaboration :
Le projet est centralisé et structuré sur un dépôt Git. Après une phase initiale de modélisation réalisée en équipe, nous avons réparti le développement entre le front-end et le back-end. Chaque développeur a pris en charge l'écriture des tests correspondant à son périmètre.
- Décisions structurantes et références des ADR :
- Vérifications réalisées et limites connues :
- Arbitrages et évolution du périmètre :
    * Algorithme de traque : Implémentation d'une IA semi-aléatoire. Elle effectue des tirs aléatoires jusqu'à toucher un navire, puis cible les cases adjacentes jusqu'à le couler.
    * Gestion de la difficulté : Intégration de niveaux de difficulté sélectionnables lors de la création de la partie, appuyés par une IA probabiliste plus poussée.
    * Refonte visuelle : Amélioration du rendu graphique grâce à l'utilisation de sprites pour représenter les navires.

