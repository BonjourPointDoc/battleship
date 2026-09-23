# ADR 4 : Stratégie de tests unitaires d'interface avec bUnit et MockHttp

## Statut et date

**Statut :** Accepté  
**Date :** 22/09/2026

Nous avons choisi d'utiliser la bibliothèque **bUnit** combinée à **RichardSzalay.MockHttp** pour valider le comportement et le rendu des composants Blazor sans dépendre d'une API backend active.

## Contexte

Les composants Blazor (`Home.razor`, `Game.razor`) intègrent une logique d'affichage asynchrone dépendante de réponses HTTP en provenance de l'API REST.

Pour intégrer des tests automatisés fiables et rapides dans la chaîne d'intégration, il est nécessaire de tester ces composants en mode isolé.

## Options envisagées

### Option 1 — Tests de bout en bout (E2E) avec Selenium ou Playwright

L'application frontend et l'API backend sont lancées conjointement, puis pilotées via un véritable navigateur.

**Avantages :**

- test en conditions réelles d'utilisation ;
- validation de l'ensemble de la chaîne (UI + Réseau + API + Base de données).

**Limites :**

- exécution très lente ;
- tests fragiles et sujets aux faux négatifs (délais réseau, environnement) ;
- difficulté d'isoler les pannes côté frontend ou backend.

### Option 2 — Tests de composants ciblés avec bUnit et MockHttp

Les composants Blazor sont rendus en mémoire à l'aide du framework bUnit, et les requêtes `HttpClient` sont interceptées par `MockHttpMessageHandler`.

**Avantages :**

- exécution extrêmement rapide des tests ;
- contrôle total sur les données renvoyées par l'API (succès, erreurs HTTP 405/500, JSON malformé) ;
- aucun besoin d'exécuter l'API ou un serveur Web durant le test.

**Limites :**

- requiert une configuration rigoureuse du contexte bUnit (définition de `BaseAddress`, navigation simulée pour les paramètres de requête) ;
- ne valide pas la connectivité réseau réelle avec l'API.

## Décision

Nous avons retenu **l'option 2 : utiliser bUnit et RichardSzalay.MockHttp**.

`bUnit` prend en charge le cycle de vie du composant Blazor (`OnInitializedAsync`, rendus successifs), tandis que `MockHttp` simule le comportement du backend en renvoyant des payloads JSON ou des codes d'erreur prévisibles.

## Conséquences

Les suites de tests doivent instancier un `HttpClient` muni d'une `BaseAddress` valide (`http://localhost/`) pour éviter l'échec silencieux des requêtes HTTP relatives.

Pour tester des composants lisant des paramètres dans l'URL (`[SupplyParameterFromQuery]`), le test doit utiliser l'instance `NavigationManager` fournie par bUnit au lieu de passer directement des paramètres à `RenderComponent`.

L'attente des mises à jour asynchrones du DOM doit être gérée via `WaitForAssertion()` pour garantir la fiabilité des assertions.

## Vérification et réexamen

Cette approche est vérifiée en exécutant la commande :

```bash
dotnet test
```