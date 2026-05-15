# 🚀 Démarrage Rapide - Système Multi-Utilisateur

## Prérequis

- .NET 10.0 SDK
- Ports disponibles: 5000 (Backend), 3000 (Frontend - optionnel)

## 1️⃣ Installation

```bash
# Backend
cd Backend
dotnet restore
dotnet run

# Frontend (autre terminal)
cd Frontend
dotnet run
```

## 2️⃣ Configuration JWT (Production)

Modifiez `Backend/appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "YOUR-VERY-LONG-SECRET-KEY-MIN-32-CHARS",
    "ExpirationMinutes": 1440
  }
}
```

## 3️⃣ Premiers Pas

```
1. Ouvrir http://localhost:5000
2. Clic "S'enregistrer"
3. Remplir les informations du couple
4. Application redirige vers le dashboard
```

## 📋 Flux d'Authentification

```
Utilisateur → Page Login
    ↓
[S'enregistrer] → POST /api/auth/register
    ↓
JWT Token + CoupleId stockés
    ↓
Dashboard (toutes les requêtes incluent le token)
    ↓
Backend filtre par CoupleId
```

## 🔑 Points Clés

| Aspect | Détail |
|--------|--------|
| **Authentification** | JWT (24h par défaut) |
| **Chiffrement Mot de passe** | BCrypt |
| **Isolation des données** | Par CoupleId |
| **Database** | SQLite (configurable) |
| **CORS** | Activé pour frontend |

## ⚙️ Architecture Multi-Utilisateur

### Backend

- **AuthController** → Gère register/login
- **JwtService** → Génère tokens
- **Middleware CoupleId** → Extrait coupleId du JWT
- **Tous les contrôleurs** → Filtrent par CoupleId
- **DBContext** → Tables associées à Couple

### Frontend

- **AuthService** → Gère l'authentification
- **ApiClient** → Ajoute token JWT aux requêtes
- **Login.razor** → Formulaire de login/register
- **App.razor** → Redirection automatique

## 🧪 Test

### Enregistrement

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@example.com",
    "password": "password123",
    "name1": "Alice",
    "name2": "Bob"
  }'
```

### Connexion

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@example.com",
    "password": "password123"
  }'
```

### Requête Authentifiée

```bash
# Remplacer TOKEN par le JWT reçu
curl -X GET http://localhost:5000/api/chat \
  -H "Authorization: Bearer TOKEN"
```

## 📊 Base de Données

**Nouvelle structure:**
- `Users` - Comptes utilisateurs (email, mot de passe)
- `Couples` - Groupes de couples
- `Messages`, `Expenses`, `Responses`, `DomestiqueResponses` - Tous avec `CoupleId`

**Isolation:** Chaque couple ne voit que ses données via le filtrage `WHERE CoupleId = ...`

## 🔒 Sécurité

✅ Mots de passe hashés (BCrypt)
✅ Tokens JWT signés
✅ Filtrage au niveau DB
✅ CORS configuré
✅ Headers Authorize sur endpoints protégés

## 📝 Modification de la Base de Données

Si vous aviez déjà une base de données, elle sera écrasée. Pour migrer l'ancienne db:

1. Sauvegardez `chat.db`
2. Supprimez `chat.db`
3. Redémarrez (nouvelle structure créée automatiquement)
4. Les données ne seront pas perdues (EF Core migration nécessaire)

## 💡 Exemples d'Utilisation

### Créer un Couple

```
Email: alice@example.com
Name1: Alice
Email2: bob@example.com (optionnel)
Name2: Bob
Password: secure!
```

### Accès Concurrent

- Alice et Bob reçoivent le même `coupleId`
- Leurs données sont isolées des autres couples
- Ils peuvent utiliser les mêmes emails si dans un couple différent

## 🆘 Problèmes Courants

| Problème | Solution |
|----------|----------|
| 401 Unauthorized | Vérifiez le token dans Authorization header |
| "CoupleId not found" | Middleware non exécuté, vérifiez Program.cs |
| 400 Bad Request register | Email unique requis, tous les champs requis |
| Database locked | Fermez les autres instances, supprimez chat.db |

## 📚 Documentation Complète

Voir [IMPLEMENTATION_SETUP.md](./IMPLEMENTATION_SETUP.md) pour:
- Configuration JWT détaillée
- Schéma complet de la BD
- Guide de production
- Troubleshooting avancé

---

**Status**: ✅ Prêt pour développement/test  
**Note**: N'oubliez pas de changer la clé JWT avant production!
