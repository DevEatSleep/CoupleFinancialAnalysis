# 🚀 Déploiement Azure - Plan Gratuit (F1)

## Étape 1 : Créer une ressource App Service sur Azure

### Via le portail Azure (simple)
1. Allez sur [portal.azure.com](https://portal.azure.com)
2. Cliquez sur **+ Create a resource**
3. Recherchez **App Service**
4. Cliquez **Create**

### Configuration recommandée (FREE tier)
- **Name** : `couple-financial-app` (ou autre, doit être unique)
- **Runtime stack** : `.NET 10`
- **Operating System** : `Linux`
- **Region** : Europe (ex: France Central)
- **App Service Plan** : Créer nouveau
  - **Sku and size** : **Free (F1)** ← Gratuit !
    - 60 min/jour de compute
    - 1 GB RAM
    - 10 GB stockage

⚠️ **Limitation Free tier** : L'app s'endort après inactivité, redémarrage lent. OK pour développement.

---

## Étape 2 : Configurer les secrets GitHub

### 2.1 Récupérer le profil de publication Azure

1. Dans [portal.azure.com](https://portal.azure.com), allez à votre **App Service**
2. À gauche, cliquez **Deployment center**
3. Sous **Deployment Center**, sélectionnez :
   - **Source** : GitHub
   - **Organization/Account** : Sélectionnez votre compte
   - **Repository** : Sélectionnez `CoupleFinancialAnalysis`
   - **Branch** : `main` (ou `master`)
4. **Authoriser Azure** (il va vous faire login GitHub)
5. Cliquez **Save** — Azure va créer le workflow automatiquement

**OU** (méthode manuelle) :

1. App Service → **Overview** → Bouton **Download publish profile** (en haut à droite)
2. Ouvrez le fichier `.publishsettings` téléchargé

### 2.2 Ajouter les secrets dans GitHub

1. Allez sur votre repo GitHub → **Settings** → **Secrets and variables** → **Actions**
2. Cliquez **New repository secret** et ajoutez :

| Secret | Valeur |
|--------|--------|
| `AZURE_APP_NAME` | Nom de votre App Service (ex: `couple-financial-app`) |
| `AZURE_PUBLISH_PROFILE` | ✅ Contenu du fichier `.publishsettings` entier (copier/coller) |

---

## Étape 3 : Vérifier la configuration de l'app

### Web.config (Pour App Service sur Windows)

Si vous ciblez Windows, créez `Backend/web.config` :

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\CoupleChat.dll" stdoutLogEnabled="false" />
  </system.webServer>
</configuration>
```

**OU** sur Linux (recommandé, plus léger) : Pas besoin, créez `.deployment` à la racine :

```
[config]
command = dotnet publish Backend/CoupleChat.csproj --output ./backend
```

### Variables d'environnement Azure

1. App Service → **Configuration** → **Application settings**
2. Ajoutez :

| Name | Value |
|------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:80` |
| `DATABASE_URL` | (optionnel : chemin BD sur Azure) |

**Ou** laissez les defaults — L'app créera `chat.db` dans son répertoire.

---

## Étape 4 : Tester le déploiement

1. Poussez un commit sur `main` :
   ```bash
   git add .
   git commit -m "Add Azure deployment workflow"
   git push origin main
   ```

2. Allez sur GitHub → **Actions** → Vérifiez le workflow qui tourne
   - ✅ Verte = déploiement réussi
   - ❌ Rouge = erreur (consultez les logs)

3. Une fois déployé, accédez à : `https://couple-financial-app.azurewebsites.net`

---

## Limitations du plan Free (F1)

| Limite | Free (F1) | Standard (S1) |
|--------|-----------|---------------|
| **Compute** | 60 min/jour | Illimité |
| **RAM** | 1 GB | 1.75 GB |
| **Stockage** | 10 GB | 50 GB |
| **Instances** | 1 | Jusqu'à 10 |
| **Domaine personnalisé** | Non | Oui |
| **SSL gratuit** | Non | Oui |
| **Coût/mois** | **0€** | ~15€ |

**Tipster** : À 60 min/jour, c'est parfait pour un dev/test, mais limité pour une vraie utilisation en production.

---

## Dépannage

### ❌ Erreur : "App Service not found"
- Vérifiez le nom dans le secret `AZURE_APP_NAME`
- Vérifiez que l'App Service est en région `Linux` et `.NET 10`

### ❌ Build échoue
- Consultez les logs du workflow GitHub (Actions → le run échoué)
- Vérifiez que `dotnet build` fonctionne localement

### ❌ App Service red/offline
- Peut être endormi (Free tier). Accédez à l'URL pour réveiller.
- Vérifiez les logs : App Service → **Log stream**

### 💾 Données supprimées après redéploiement
- `chat.db` (SQLite) est inclus dans le répertoire de publish, donc persiste.
- Pour une BD persistante, utilisez **Azure SQL Database** (nécessite plan payant) ou **Azure Blob Storage**.

---

## Alternatives gratuites sur Azure

| Option | Gratuit ? | Limitations |
|--------|----------|------------|
| **App Service (F1)** | ✅ | 60 min/jour |
| **Static Web Apps** | ✅ | Frontend + Functions uniquement |
| **Azure Container Registry** | ✅ 12 mois | Pour Docker |
| **Azure SQL Database** | ❌ | À partir de 5€/mois |
| **Azure Blob Storage** | ✅ (limite) | 5 GB gratuits/mois |

---

## Prochaines étapes

1. **Ajouter un domaine personnalisé** (nécessite plan payant)
2. **Activer HTTPS** (libre avec managed certificate sur App Service)
3. **Scale up vers Standard (S1)** si you dépassez 60 min/jour (15€/mois)
4. **Migrer vers Azure Database** pour la BD (Azure SQL = ~5€/mois)

