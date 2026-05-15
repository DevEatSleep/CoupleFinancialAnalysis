# 🔐 Implémentation Multi-Utilisateur - Guide de Déploiement

## Vue d'ensemble

L'application a été transformée en un système multi-utilisateur avec authentification JWT. Chaque couple crée un compte, s'authentifie, et accède à ses données privées.

## ✅ Modifications Effectuées

### Backend

#### 1. **Modèles (Models)**
- ✅ `User.cs` - Nouveau modèle utilisateur avec email, mot de passe hashé, et CoupleId
- ✅ `Couple.cs` - Nouveau modèle couple pour regrouper les utilisateurs
- ✅ Modèles existants modifiés pour ajouter `CoupleId` :
  - `Message.cs` - Ajout CoupleId
  - `Response.cs` - Ajout CoupleId
  - `Expense.cs` - Ajout CoupleId
  - `DomestiqueResponse.cs` - Ajout CoupleId

#### 2. **Services d'Authentification**
- ✅ `JwtService.cs` - Génération et validation des tokens JWT
- ✅ `PasswordService.cs` - Hachage et vérification des mots de passe (BCrypt)

#### 3. **Contrôleurs**
- ✅ `AuthController.cs` - Nouveaux endpoints:
  - `POST /api/auth/register` - Créer un couple avec 1 ou 2 utilisateurs
  - `POST /api/auth/login` - Se connecter avec email/mot de passe
  - `POST /api/auth/verify` - Vérifier la validité d'un token
- ✅ `ChatController.cs` - Filtrage par CoupleId + Authorize
- ✅ `BotController.cs` - Filtrage par CoupleId + Authorize
- ✅ `DomestiqueController.cs` - Filtrage par CoupleId + Authorize

#### 4. **Middleware**
- ✅ `CoupleIdMiddleware.cs` - Extraction du CoupleId du JWT et injection dans HttpContext

#### 5. **Configurations**
- ✅ `Program.cs` - Configuration JWT, authentification, et middleware
- ✅ `appsettings.json` - Paramètres JWT (secret key, issuer, audience, expiration)
- ✅ `ChatDbContext.cs` - Nouvelles relations et configurations pour User/Couple

#### 6. **Dépendances**
- ✅ `CoupleChat.csproj` - Ajout des packages:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `System.IdentityModel.Tokens.Jwt`
  - `BCrypt.Net-Core`

### Frontend

#### 1. **Modèles**
- ✅ `AuthModel.cs` - Modèles pour authentification:
  - `AuthUser` - Utilisateur authentifié
  - `LoginRequest` / `LoginResponse`
  - `RegisterRequest` / `AuthResponse`

#### 2. **Services**
- ✅ `AuthService.cs` - Gestion de l'authentification:
  - Méthodes `RegisterAsync()`, `LoginAsync()`, `VerifyTokenAsync()`
  - Gestion du stockage du token et des données utilisateur
- ✅ `SessionStorage.cs` - Stockage centralisé des données d'authentification
- ✅ `ApiClient.cs` - Modification pour inclure le token JWT dans les headers

#### 3. **Pages**
- ✅ `Login.razor` - Nouvelle page avec formulaires de login/registration

#### 4. **Configuration**
- ✅ `Program.cs` - Enregistrement des services d'authentification
- ✅ `App.razor` - Redirection automatique vers /login si non authentifié

#### 5. **Constantes**
- ✅ `Shared/Constants.cs` - Ajout des endpoints d'authentification

## 🚀 Étapes de Déploiement

### 1. **Préparation**

```bash
cd Backend
dotnet restore
```

### 2. **Configurer JWT (IMPORTANT pour Production)**

Modifiez `Backend/appsettings.json` ou créez `appsettings.Production.json`:

```json
{
  "Jwt": {
    "SecretKey": "CHANGEZ-MOI-!!!-MINIMUM-32-CARACTERES-DE-CLÉ-SECRÈTE",
    "Issuer": "CoupleChat",
    "Audience": "CoupleChat",
    "ExpirationMinutes": 1440
  }
}
```

⚠️ **SÉCURITÉ**: Utilisez une clé secrète forte (au moins 32 caractères) en production!

### 3. **Initialiser la Base de Données**

La base de données sera créée automatiquement au premier démarrage grâce à `EnsureCreatedAsync()`.

**IMPORTANT**: Si vous aviez une base de données existante (`chat.db`), elle sera recréée avec le nouveau schéma. Les données existantes seront perdues.

Pour conserver les données existantes, vous devez créer une migration EF Core manuelle.

### 4. **Démarrer l'Application**

```bash
# Terminal 1 - Backend
cd Backend
dotnet run

# Terminal 2 - Frontend (optionnel si déployé avec Backend)
cd Frontend
dotnet run
```

### 5. **Test Automatisé**

```bash
# Backend devrait afficher:
# - Listening on http://0.0.0.0:5000
# - Database initialized
# - JWT configured

# Frontend devrait afficher:
# - Listening on http://localhost:3000
# - Redirects to /login automatically
```

## 📝 Flux d'Utilisation

### Premier Accès

```
1. Utilisateur accède à http://localhost:5000
2. App.razor redirige vers /login (non authentifié)
3. Page Login.razor affiche deux options:
   - "Se connecter" (si compte existant)
   - "S'enregistrer" (nouveau couple)
```

### Inscription (Nouveau Couple)

```
POST /api/auth/register
{
  "email": "alice@example.com",
  "email2": "bob@example.com",     // Optionnel
  "password": "secure_password",
  "name1": "Alice",
  "name2": "Bob"
}

Réponse:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": 1,
  "coupleId": 1,
  "email": "alice@example.com",
  "name": "Alice"
}
```

### Connexion

```
POST /api/auth/login
{
  "email": "alice@example.com",
  "password": "secure_password"
}

Réponse:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": 1,
  "coupleId": 1,
  "email": "alice@example.com",
  "name": "Alice"
}
```

### Requêtes Authentifiées

Toutes les requêtes API incluent le token:

```
GET /api/chat
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

// Response retourne les messages du couple uniquement
```

## 🔒 Sécurité

### Points Clés

1. **JWT Tokens**
   - Expiration: 24h (configurable dans `appsettings.json`)
   - Tokens révoqués après logout
   - Signature HMAC-SHA256

2. **Mots de Passe**
   - Hashés avec BCrypt (11 rounds par défaut)
   - Jamais stockés en clair
   - Jamais retournés par l'API

3. **Isolation des Données**
   - Chaque couple ne voit que ses propres données
   - Filtrage au niveau contrôleur + base de données
   - Middleware vérifie le CoupleId dans le JWT

4. **CORS**
   - Configuré pour les origines autorisées
   - Credentials activés pour Blazor
   - À mettre à jour en production

### Checklist Production

- [ ] Changez la clé JWT secrète dans `appsettings.Production.json`
- [ ] Activez HTTPS (redirection http→https)
- [ ] Mettez à jour les AllowedOrigins dans Constants.cs
- [ ] Configurez une vraie base de données (PostgreSQL recommandé)
- [ ] Activez le logging et le monitoring
- [ ] Mettez en place une stratégie de backup

## 📊 Schéma de Base de Données

```
Couples
├── Id (PK)
├── CreatedAt
└── Users (1..N)

Users
├── Id (PK)
├── Email (Unique)
├── PasswordHash
├── Name
├── CoupleId (FK)
└── CreatedAt

Messages
├── Id (PK)
├── Sender
├── Content
├── CoupleId (FK)
├── CreatedAt
└── Index: (CoupleId)

Responses
├── Id (PK)
├── QuestionId
├── QuestionText
├── UserResponse
├── Category
├── Person
├── CoupleId (FK)
├── CreatedAt
└── Index: (CoupleId)

Expenses
├── Id (PK)
├── Label
├── Amount
├── PaidBy
├── CoupleId (FK)
├── CreatedAt
└── Index: (CoupleId)

DomestiqueResponses
├── Id (PK)
├── Person
├── Activite
├── HeuresParSemaine
├── InseeRefFemme
├── InseeRefHomme
├── ValeurMonetaire
├── CoupleId (FK)
├── CreatedAt
└── Index: (Person, Activite, CoupleId) [Unique]

TravailDomestique (Référence INSEE - Partagée)
├── Id (PK)
├── Sexe
├── Activite
├── TrancheAge
├── DureeMinutes
├── DureeHeures
├── CoutJour
└── Index: (Sexe, Activite, TrancheAge) [Unique]
```

## 🐛 Troubleshooting

### Erreur: "JwtSecretKey not configured"

✅ Vérifiez que `appsettings.json` contient la section `Jwt:`

### Erreur: "CoupleId not found in token"

✅ Middleware d'authentification ne s'a pas exécuté
- Vérifiez l'ordre du middleware dans Program.cs
- `UseAuthentication()` avant `UseCoupleIdMiddleware()`

### 401 Unauthorized sur endpoints protégés

✅ Token manquant ou expiré
- Vérifiez que le frontend envoie le header `Authorization: Bearer <token>`
- Vérifiez l'expiration du token

### 400 Bad Request sur /api/auth/register

✅ Validation des données
- Email requis et doit être unique
- Password, Name1, Name2 requis
- Email2 optionnel

## 📚 Ressources Supplémentaires

- [JWT.io - Token Debugger](https://jwt.io)
- [BCrypt Playground](https://bcrypt-generator.com/)
- [Microsoft JWT Docs](https://learn.microsoft.com/en-us/dotnet/api/system.identitymodel.tokens.jwt)

## ✨ Prochaines Étapes (Optionnelles)

1. **Refresh Tokens** - Implémenter les refresh tokens pour renouveler les sessions
2. **2FA** - Ajouter l'authentification à deux facteurs
3. **Invitation** - Permettre à un utilisateur d'inviter son partenaire
4. **Social Login** - Google/Facebook OAuth
5. **Audit Logs** - Logger les actions par couple
6. **Rate Limiting** - Limiter les tentatives de login

---

**Version**: 1.0  
**Date**: 2026-05-13  
**Status**: ✅ Implémentation Complète
