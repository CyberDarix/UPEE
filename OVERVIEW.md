# 🚀 UPEE - Présentation Générale

## Qu'est-ce que UPEE ?

**UPEE** (Universal Polyglot Execution Engine) est un moteur d'exécution universal conçu pour offrir un **contrôle total** sur les ressources matérielles d'un système Windows.

Agissant comme un *"Runtime Système Intelligent"*, UPEE permet aux utilisateurs de:
- 📝 Définir leurs propres processus d'automatisation
- 🧮 Exécuter du calcul multi-langages
- ⚡ Optimiser dynamiquement les ressources système
- 🔐 Isoler et sécuriser l'exécution de scripts

## Vision

Une **infrastructure logicielle invisible** où:
- Les ingénieurs definissent la logique via scripts
- Le système exécute automatiquement sans interface GUI
- Les ressources matérielles répondent directement aux commandes
- La stabilité du système est garantie quoi qu'il advienne

## Architecture Core (5 Piliers)

### 1️⃣ Bus de Données Universel
```
Mémoire Partagée JSON
├─ Accessible de tout langage
├─ Thread-safe
└─ Événements temps-réel
```
Les scripts échangent des données sans copying grâce à un système d'interopérabilité unifié.

### 2️⃣ Scheduler de Priorité CPU
```
File d'Attente Intelligente
├─ Low      (0)  ← Tâches arrière-plan
├─ Medium   (1)  ← Tâches standard
├─ High     (2)  ← Priorité élevée
└─ Critical (3)  ← Système critique
```
Optimise les ressources CPU en hiérarchisant intelligemment.

### 3️⃣ Isolation Sécurisée (Sandbox)
```
Environnement Virtuel
├─ Process isolation
├─ Memory limits
├─ Timeout enforcement
└─ No system impact
```
Une erreur de script n'affecte JAMAIS la stabilité du système.

### 4️⃣ Logging Complet & Temps-Réel
```
Traçage Intelligent
├─ Fichier: /logs/history.log
├─ Console: Notifications immédates
├─ Format: [TIMESTAMP] [LEVEL] [COMPONENT] Message
└─ Async: Non-blocking
```
Débogage immédiat sans impact sur les performances.

### 5️⃣ Exécuteur Polyglotte
```
Support Multi-Langages
├─ C/C++ (Performance)
├─ Python (Rapidité dev)
├─ Rust (Sécurité)
├─ Go (Parallélisme)
├─ JavaScript (Flexibilité)
├─ Lua (Scripting léger)
└─ Shell (Automatisation)
```

## Format Directive (En-Tête Standardisé)

```
// UPEE_<COMMAND>_<MODULE>_[OPTIONS]

Exemple:
// UPEE_RUN_PYTHON_PRIORITY=2_TIMEOUT=60
```

**Commandes**: RUN, COMPILE, EXEC  
**Modules**: CPP, RUST, PYTHON, GO, JS, LUA, SHELL  
**Priorités**: Low(0), Medium(1), High(2), Critical(3)

## Installation Rapide (3 étapes)

### 1. Initialiser Workspace
```bash
upee connect "C:\Mon\Dossier"
```
→ Crée la structure:
```
C:\Mon\Dossier\UPEE\
├── scripts/    # Vos scripts
├── libs/       # Bibliothèques
├── logs/       # Historique
├── bin/        # Compilés
└── core/       # Configuration
```

### 2. Créer un Script
```python
// UPEE_RUN_PYTHON

def main():
	print("Bonjour depuis UPEE!")

if __name__ == "__main__":
	main()
```

### 3. Exécuter
```bash
upee execute -s mon_script.py
```

## Commandes CLI

```bash
# Initialisation
upee connect <path>              # Créer workspace

# Exécution
upee execute -s <script>         # Un script spécifique
upee execute -w                  # Surveiller les modifications

# Information
upee status                      # État du service
```

## API Programmatique

```csharp
using UPEE.Runtime;

// Initialiser le gestionnaire
var manager = new UPEERuntimeManager("C:\\UPEE");

// Exécuter un script
var result = await manager.ExecuteScriptAsync("script.py");
Console.WriteLine(result.Success ? "✓ OK" : $"✗ {result.Error}");

// Partager des données
manager.SetGlobalVariable("config", new { debug = true });
var config = manager.GetGlobalVariable<dynamic>("config");

// Ordonnancer avec priorité
manager.ScheduleScript("task.rs", ExecutionPriority.Critical);
```

## Flux d'Exécution Complet

```
Script déposé dans /scripts/
		↓
FileSystemWatcher détecte
		↓
Parse directive (// UPEE_RUN_...)
		↓
Consulte priorité (Medium par défaut)
		↓
Ajoute à PriorityScheduler
		↓
Crée SandboxEnvironment isolé
		↓
Exécute le script
		↓
Capture stdout/stderr
		↓
Enregistre dans UniversalLogger
		↓
Stocke résultat dans DataBus
		↓
Déclenche événement TaskCompleted
		↓
Nettoie ressources
```

## Cas d'Usage Réels

### 1. Automatisation Système
```python
// UPEE_RUN_PYTHON_PRIORITY=2

import subprocess
subprocess.run(["diskdefrag", "C:"])
print("Défragmentation terminée")
```

### 2. Calcul Intensif (Rust)
```rust
// UPEE_RUN_RUST_PRIORITY=1

fn main() {
	let result: u64 = (1..=1000000).map(|x| x*x).sum();
	println!("Résultat: {}", result);
}
```

### 3. Monitoring Temps-Réel
```javascript
// UPEE_RUN_JS_TIMEOUT=300

setInterval(() => {
	const usage = process.cpuUsage();
	console.log(`CPU: ${usage.user}µs`);
}, 1000);
```

### 4. Orchestration Multi-Langages
```bash
#!/bin/bash
# UPEE_RUN_SHELL_PRIORITY=3

# Étape 1: Préparer données (Python)
upee execute -s prepare_data.py

# Étape 2: Traiter (C++)
upee execute -s process.cpp

# Étape 3: Analyser résultats (Go)
upee execute -s analyze.go
```

## Avantages Clés

| Avantage | Description |
|----------|-------------|
| **Polyglot** | Code dans n'importe quel langage |
| **Sécurisé** | Isolation complète au niveau processus |
| **Performant** | Scheduling intelligent des ressources |
| **Transparent** | Pas d'interface GUI, API simple |
| **Robuste** | Erreurs isolées, pas d'impact système |
| **Observable** | Logging complet et notifications |
| **Portable** | Service Windows persistant |

## Limitations Actuelles (v1.0)

❌ Pas de support multi-machine (v2.0)  
❌ Pas de base de données persistante (utiliser fichiers)  
❌ Pas de conteneurs Docker (v1.1)  
❌ Pas d'API REST (v1.1)  

## Roadmap

```
v1.0 (✓ Disponible)
├─ Exécution single-machine
├─ Sandbox isolation
└─ Multi-language support

v1.1 (Planifiée Q4 2024)
├─ REST API endpoints
├─ Web dashboard
└─ Docker support

v2.0 (Planifiée 2025)
├─ Distributed execution
├─ Multi-machine orchestration
└─ Kubernetes integration
```

## Comparaison avec Alternatives

| Critère | UPEE | Docker | Kubernetes | Bash |
|---------|------|--------|------------|------|
| Polyglot | ✅ | ✅ | ✅ | ❌ |
| Léger | ✅ | ❌ | ❌ | ✅ |
| Sécurisé | ✅ | ✅ | ✅ | ❌ |
| Simple | ✅ | ❌ | ❌ | ✅ |
| Windows Native | ✅ | ⚠️ | ⚠️ | ✅ |
| Priorités | ✅ | ❌ | ✅ | ❌ |
| Logging | ✅ | ⚠️ | ✅ | ❌ |

## Getting Started

### Prérequis
- Windows 10+ 
- .NET 10.0
- PowerShell 5.0+

### Installation
```bash
# 1. Compiler
dotnet build UPEE.sln --configuration Release

# 2. Déployer
.\Deploy-UPEE.ps1 -WorkspacePath "C:\UPEE"

# 3. Tester
upee connect "C:\UPEE"
upee execute -s examples/hello_world.py
upee status
```

### Prochains Pas
1. 📖 Lire README.md pour la documentation complète
2. 🏗️ Consulter ARCHITECTURE.md pour les détails techniques
3. 💻 Explorer examples/ pour des cas d'usage
4. 🚀 Déployer votre premier script !

## Conclusion

**UPEE** est une solution moderne, sécurisée et performante pour:
- Exécuter du code multi-langages
- Automatiser des tâches système
- Optimiser l'utilisation des ressources
- Garantir la stabilité du système

**Status**: 🟢 Production Ready v1.0

---

*Pour plus d'informations:*
- 📄 README.md - Guide utilisateur
- 🏗️ ARCHITECTURE.md - Détails techniques
- 📋 DEPLOYMENT_REPORT.md - Rapport de déploiement
- 💡 upee.config.json - Configuration
