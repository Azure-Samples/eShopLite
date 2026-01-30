# Agent Assistant d'Achat - Vue d'ensemble

## Introduction

L'Agent Assistant d'Achat est une solution de commerce conversationnel alimentée par l'IA qui aide les utilisateurs à découvrir des produits, à obtenir des informations détaillées et à gérer leur panier d'achat grâce à des interactions en langage naturel. Construit avec le Framework Agent de Microsoft, il démontre comment les agents d'IA modernes peuvent améliorer l'expérience e-commerce.

## Qu'est-ce que l'Agent Assistant d'Achat ?

L'Assistant d'Achat est un agent intelligent qui comprend l'intention de l'utilisateur et fournit des réponses contextuelles aux requêtes liées aux achats. Il peut :

- **Rechercher des Produits :** Trouver des produits basés sur des descriptions en langage naturel
- **Fournir des Détails de Produits :** Récupérer et présenter des informations détaillées sur les produits
- **Gérer le Panier d'Achat :** Ajouter des articles au panier via des commandes conversationnelles
- **Faire des Recommandations :** Suggérer des produits basés sur les préférences et le contexte de l'utilisateur

## Capacités Clés

### Compréhension du Langage Naturel

L'agent utilise les modèles GPT d'Azure OpenAI pour comprendre l'intention de l'utilisateur à partir de requêtes en langage naturel. Il peut gérer :

- Requêtes simples : "Montre-moi des chaussures de randonnée"
- Demandes complexes : "J'ai besoin de chaussures de randonnée imperméables pour l'hiver, de préférence moins de 150€"
- Questions de suivi : "Parle-moi plus du premier"
- Actions de panier : "Ajoute ça à mon panier"

### Orchestration Multi-Outils

L'agent coordonne plusieurs outils spécialisés pour répondre aux demandes des utilisateurs :

1. **Outil SearchCatalog** - Recherche dans la base de données de produits
2. **Outil ProductDetails** - Récupère les informations détaillées des produits
3. **Outil AddToCart** - Gère les opérations du panier

### Réponses Contextuelles

L'agent maintient le contexte de la conversation pour fournir des réponses de suivi pertinentes et gérer les références aux articles précédemment discutés.

## Aperçu de l'Architecture

L'Assistant d'Achat est construit sur trois composants principaux :

1. **Service Agent** - La logique de base de l'agent utilisant Microsoft.Agents.Client
2. **Outils Agent** - Outils spécialisés pour différentes opérations
3. **Interface Chat** - Interface utilisateur de chat dans l'application Store

```
┌─────────────────┐
│   Interface     │
│   Store         │
│  (Panneau Chat) │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Assistant      │
│  d'Achat        │
│  Agent          │
└────────┬────────┘
         │
    ┌────┴────┐
    │ Couche  │
    │ Outils  │
    └────┬────┘
         │
         ▼
┌─────────────────┐
│  API Produits   │
│  & Base de      │
│  Données        │
└─────────────────┘
```

## Pile Technologique

- **Microsoft Agent Framework** - Orchestration des agents et intégration des outils
- **Azure OpenAI** - Compréhension et génération du langage naturel
- **ASP.NET Core** - Services API backend
- **Blazor** - Interface utilisateur de chat interactive
- **.NET Aspire** - Orchestration cloud-native
- **SQL Server** - Données produits et commandes

## Cas d'Utilisation

### Découverte de Produits

**Utilisateur :** "Je cherche des chaussures de course pour le trail"

**Agent :** Recherche dans le catalogue et retourne des chaussures de trail pertinentes avec de brèves descriptions et prix.

### Informations Détaillées

**Utilisateur :** "Dis-moi en plus sur les bleues"

**Agent :** Récupère et présente des informations détaillées sur le produit référencé, incluant spécifications, caractéristiques et avis clients.

### Gestion du Panier

**Utilisateur :** "Ajoute-les à mon panier"

**Agent :** Ajoute le produit référencé au panier de l'utilisateur et confirme l'action.

## Avantages

- **Expérience Utilisateur Améliorée :** La conversation naturelle est plus intuitive que la recherche traditionnelle
- **Engagement Accru :** Les utilisateurs passent plus de temps à explorer les produits via la conversation
- **Meilleure Découverte :** L'IA aide les utilisateurs à trouver des produits qu'ils n'auraient pas découverts via la recherche par mots-clés
- **Friction Réduite :** Ajouter des articles au panier est plus facile via la conversation
- **Personnalisation :** L'agent peut apprendre les préférences de l'utilisateur au fil du temps

## Démarrage

Pour commencer à utiliser l'Assistant d'Achat :

1. Déployez le scénario en suivant les instructions du README
2. Ouvrez l'application Store
3. Cliquez sur l'icône de chat pour ouvrir l'assistant
4. Commencez à discuter avec des requêtes en langage naturel

Pour des instructions de configuration détaillées, voir [Configuration - Développement Local](30_setup_local.md) ou [Configuration - Déploiement Azure](40_setup_azure.md).
