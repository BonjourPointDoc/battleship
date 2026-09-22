# Échanges décisifs avec l’IA

## Date et sujet

- Outil / modèle si connu :
- Contexte :
- Prompt réellement utilisé :
- Réponse et hypothèses résumées :
- Décision et justification :
- Scénario ou commande de vérification :
- Résultat attendu, puis résultat observé :
- Erreur que ce contrôle pourrait détecter :
- Preuves reproductibles et limites :


## 1 : Diagnostic des échecs de tests bUnit (BaseAddress et SupplyParameterFromQuery)
- Date : 22 septembre 2026
- Sujet : Résolution des erreurs de rendu bUnit (WaitForFailedException et ArgumentException) dans GameTests.cs et HomeTests.cs.
- Outil / modèle si connu : Gemini Flash extended
- Contexte : Les tests Blazor avec RichardSzalay.MockHttp échouaient lors du dotnet test. Les composants restaient bloqués en chargement ou levaient une exception lors de la lecture des paramètres d'URL.
- Prompt réellement utilisé : « Test de BattleShip.Tests net10.0 : a échoué avec 3 erreur(s) ... [logs bUnit] Pourquoi cette erreur arrive ? »
- Réponse et hypothèses résumées :

HttpClient généré par MockHttp ne possédait pas de BaseAddress, faisant planter silencieusement les requêtes relatives dans OnInitializedAsync.

bUnit interdit le passage direct de paramètres décorés avec [SupplyParameterFromQuery] via parameters.Add().

L'utilisation de WaitForAssertion() est obligatoire pour attendre la fin du cycle de vie d'un composant asynchrone.

- Décision et justification :

Assigner httpClient.BaseAddress = new Uri("http://localhost/"); dans le constructeur des classes de test.

Utiliser NavigationManager.NavigateTo() pour simuler les Query Strings dans bUnit.

Remplacer les sélecteurs directs par des blocs cut.WaitForAssertion().

Scénario ou commande de vérification : Exécution de la suite de tests unitaires via dotnet test.

- Résultat attendu, puis résultat observé :

    - Attendu : Tous les tests bUnit passent au vert.

    - Observé : 0 erreur, exécution complète en moins de 2 secondes.

- Erreur que ce contrôle pourrait détecter : Des faux négatifs dans la chaîne d'intégration continue (CI/CD) causés par des mocks mal configurés ou des composants bloqués à l'initialisation.

- Preuves reproductibles et limites : Exécution de dotnet test sur le projet BattleShip.Tests. Limite : Vérifie uniquement le rendu UI en mémoire, pas la connectivité API réelle.