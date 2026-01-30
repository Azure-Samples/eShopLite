# Agent Assistant d'Achat - Guide de Configuration Locale

Ce guide vous accompagne dans la configuration du scénario Agent Assistant d'Achat sur votre machine de développement locale.

## Prérequis

Avant de commencer, assurez-vous d'avoir installé les éléments suivants :

### Logiciels Requis

1. **.NET 8 SDK ou ultérieur**
   - Télécharger depuis [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
   - Vérifier l'installation : `dotnet --version`

2. **Docker Desktop** ou **Podman**
   - Docker Desktop : [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
   - Podman : [https://podman.io/](https://podman.io/)
   - Requis pour exécuter le conteneur SQL Server

3. **Workload .NET Aspire**
   ```bash
   dotnet workload install aspire
   ```

4. **Git**
   - Télécharger depuis [https://git-scm.com/downloads](https://git-scm.com/downloads)

### Outils Recommandés

- **Visual Studio 2022** (17.8 ou ultérieur) ou **Visual Studio Code** avec C# Dev Kit
- **Azure Developer CLI (azd)** - Pour le déploiement Azure
  ```bash
  # Windows (PowerShell)
  winget install microsoft.azd
  
  # macOS
  brew tap azure/azd && brew install azd
  
  # Linux
  curl -fsSL https://aka.ms/install-azd.sh | bash
  ```

### Ressources Azure

Vous aurez besoin d'un accès à :

- **Service Azure OpenAI** avec :
  - Déploiement `gpt-4o-mini`
  - Déploiement `text-embedding-ada-002`
- **Abonnement Azure** avec les autorisations appropriées

## Étape 1 : Cloner le Dépôt

```bash
git clone https://github.com/Azure-Samples/eShopLite.git
cd eShopLite/scenarios/13-AgentFramework
```

## Étape 2 : Configurer Azure OpenAI

### Option A : Utilisation d'une Ressource Azure OpenAI Existante

Si vous avez une ressource Azure OpenAI existante :

1. Naviguez vers le projet ShoppingAssistantAgent :
   ```bash
   cd src/ShoppingAssistantAgent
   ```

2. Initialisez les secrets utilisateur :
   ```bash
   dotnet user-secrets init
   ```

3. Définissez votre configuration Azure OpenAI :
   ```bash
   dotnet user-secrets set "OpenAI:Endpoint" "https://votre-ressource.openai.azure.com/"
   dotnet user-secrets set "OpenAI:ApiKey" "votre-cle-api-ici"
   dotnet user-secrets set "OpenAI:DeploymentName" "gpt-4o-mini"
   ```

4. Configurez également l'API Produits :
   ```bash
   cd ../Products
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://votre-ressource.openai.azure.com/;Key=votre-cle-api-ici;"
   dotnet user-secrets set "AI_ChatDeploymentName" "gpt-4o-mini"
   dotnet user-secrets set "AI_embeddingsDeploymentName" "text-embedding-ada-002"
   ```

### Option B : Laisser Aspire Créer les Ressources

Si vous n'avez pas de ressources Azure OpenAI, Aspire peut les créer pour vous :

1. Assurez-vous d'être connecté à Azure :
   ```bash
   azd auth login
   ```

2. Sautez la configuration des secrets - Aspire provisionnera les ressources au premier lancement

## Étape 3 : Construire la Solution

Naviguez vers le répertoire src et construisez :

```bash
cd src
dotnet restore
dotnet build
```

Vérifiez que la construction se termine sans erreurs.

## Étape 4 : Démarrer l'Application

### Utilisation de .NET Aspire AppHost

1. Naviguez vers le projet AppHost :
   ```bash
   cd eShopAppHost
   ```

2. Exécutez l'application :
   ```bash
   dotnet run
   ```

3. Le Tableau de Bord Aspire s'ouvrira automatiquement dans votre navigateur, affichant :
   - Tous les services en cours d'exécution
   - Points de terminaison des services
   - Journaux et traces
   - État des ressources

### Utilisation de Visual Studio

1. Ouvrez `src/eShopLite-AgentFramework.sln` dans Visual Studio

2. Définissez `eShopAppHost` comme projet de démarrage

3. Appuyez sur F5 pour exécuter

### Utilisation de Visual Studio Code

1. Ouvrez le dossier `scenarios/13-AgentFramework` dans VS Code

2. Ouvrez la Palette de Commandes (Ctrl+Shift+P / Cmd+Shift+P)

3. Sélectionnez `.NET: Run Aspire App`

4. Choisissez `eShopAppHost`

## Étape 5 : Accéder à l'Application

Une fois l'application démarrée, vous verrez une sortie similaire à :

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Points d'Accès

- **Interface Store** : `https://localhost:7001`
  - Application eCommerce principale
  - Interface de chat de l'Assistant d'Achat

- **Tableau de Bord Aspire** : `https://localhost:17001`
  - Gestion des services
  - Journaux et traces
  - Surveillance des ressources

- **Agent Assistant d'Achat** : `https://localhost:7002`
  - Interface Swagger à `/swagger`
  - API Chat à `/api/agent/chat`

- **API Produits** : `https://localhost:7003`
  - Interface Swagger à `/swagger`
  - Points de terminaison produits

## Étape 6 : Tester l'Assistant d'Achat

1. Ouvrez l'interface Store dans votre navigateur

2. Recherchez l'icône de chat (généralement en haut à droite ou en bas à droite)

3. Cliquez pour ouvrir le panneau de chat

4. Essayez ces exemples de requêtes :
   - "Montre-moi des chaussures de randonnée"
   - "Parle-moi plus du produit 1"
   - "Ajoute ça à mon panier"
   - "Quel équipement d'extérieur avez-vous ?"

## Dépannage

### Problème : Port Déjà Utilisé

**Solution :** Changez les ports dans `eShopAppHost/Properties/launchSettings.json`

### Problème : Le Conteneur SQL Server Ne Démarre Pas

**Solution :** 
1. Assurez-vous que Docker Desktop est en cours d'exécution
2. Vérifiez les ports disponibles : `netstat -an | findstr :1433`
3. Essayez de redémarrer Docker Desktop

### Problème : Limites de Débit Azure OpenAI

**Solution :**
1. Vérifiez votre quota dans le Portail Azure
2. Réduisez les demandes concurrentes
3. Envisagez de passer à un quota supérieur

### Problème : L'Agent Ne Répond Pas

**Solution :**
1. Vérifiez les erreurs dans le Tableau de Bord Aspire
2. Vérifiez la configuration OpenAI dans les secrets utilisateur
3. Vérifiez la connectivité réseau vers Azure OpenAI
4. Consultez les journaux dans Application Insights

### Problème : Erreurs de Construction

**Solution :**
1. Nettoyer la solution : `dotnet clean`
2. Supprimer les dossiers bin/obj : `rm -rf **/bin **/obj`
3. Restaurer les packages : `dotnet restore`
4. Reconstruire : `dotnet build`

## Étapes de Vérification

Après la configuration, vérifiez :

1. ✅ Tous les services affichent "En cours d'exécution" dans le Tableau de Bord Aspire
2. ✅ L'interface Store se charge sans erreurs
3. ✅ Le panneau de chat s'ouvre et accepte les entrées
4. ✅ L'agent répond aux requêtes simples
5. ✅ Les produits apparaissent dans les résultats de recherche
6. ✅ La fonctionnalité d'ajout au panier fonctionne

## Prochaines Étapes

- Consultez la [Documentation d'Architecture](20_architecture.md)
- Lisez le [Guide Utilisateur](50_user_guide.md) pour des conseils d'utilisation
- Voir le [Guide Admin](60_admin_guide.md) pour les options de configuration
- Essayez de déployer sur Azure avec le [Guide de Configuration Azure](40_setup_azure.md)

## Configuration Supplémentaire

### Activer la Journalisation Détaillée

Ajoutez à `appsettings.Development.json` :

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ShoppingAssistantAgent": "Debug"
    }
  }
}
```

### Configurer CORS pour les Tests Locaux

Dans `ShoppingAssistantAgent/Program.cs`, CORS est déjà configuré pour le développement local.

### Personnaliser le Comportement de l'Agent

Modifiez les descriptions d'outils dans :
- `Tools/SearchCatalogTool.cs`
- `Tools/ProductDetailsTool.cs`
- `Tools/AddToCartTool.cs`

## Flux de Travail de Développement

1. Effectuez des modifications de code
2. Aspire reconstruira et redémarrera automatiquement les services affectés
3. Actualisez le navigateur pour voir les modifications
4. Vérifiez les journaux et erreurs dans le Tableau de Bord Aspire
5. Itérez

## Conseils de Performance

- Utilisez `dotnet build` au lieu de `dotnet run --no-build` lors des itérations
- Allouez les ressources Docker Desktop de manière appropriée
- Surveillez l'utilisation de tokens dans Azure OpenAI pour gérer les coûts
- Utilisez la mise en cache locale pour les données fréquemment consultées

## Obtenir de l'Aide

Si vous rencontrez des problèmes :

1. Consultez la section [Dépannage](#dépannage) ci-dessus
2. Consultez les journaux dans le Tableau de Bord Aspire
3. Recherchez les problèmes GitHub existants
4. Créez un nouveau problème avec :
   - Étapes pour reproduire
   - Messages d'erreur
   - Détails de l'environnement
   - Captures d'écran du Tableau de Bord Aspire
