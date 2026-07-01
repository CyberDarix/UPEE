# UPEE - Universal Polyglot Execution Engine

## Vue d'ensemble

UPEE est un moteur d'exécution universel et décentralisé conçu pour offrir un contrôle total sur les ressources matérielles d'un système. Agissant comme un "Runtime Système Intelligent", il permet à l'utilisateur de définir, via des scripts polyglottes, ses propres processus d'automatisation, de calcul ou d'optimisation.

## Architecture

### Modules Principaux

#### 1. **UPEE.Core** - Fondations du moteur
- **DirectiveHeader**: Parsing des en-têtes standardisés (`// UPEE_RUN_CPP`)
- **UniversalDataBus**: Bus de mémoire partagée JSON pour l'interopérabilité inter-processus
- **PriorityScheduler**: Gestionnaire de files avec hiérarchisation (Low/Medium/High/Critical)
- **SandboxEnvironment**: Environnements sécurisés pour l'exécution des scripts
- **UniversalLogger**: Système de traçage complet avec notifications en temps réel
- **PolyglotsExecutionEngine**: Orchestrateur principal d'exécution multi-langages

#### 2. **UPEE.CLI** - Interface en ligne de commande
```bash
upee connect <path>              # Initialiser un workspace
upee execute -s <script>         # Exécuter un script spécifique
upee execute -w                  # Surveiller les modifications
upee status                      # Afficher l'état du service
```

#### 3. **UPEE.Service** - Service Windows
Enregistrement comme service système persistant avec monitoring automatique.

#### 4. **UPEE.Runtime** - Gestionnaire d'exécution
Gestionnaire centralisé des composants UPEE avec API d'utilisation programmatique.

#### 5. **UPEE.Tests** - Suite de tests
14 tests unitaires validant tous les composants critiques.

## Installation

### Prérequis
- .NET 10.0
- Windows 10+
- PowerShell 5.0+

### Compilation
```bash
dotnet build UPEE.sln --configuration Release
```

### Installation du Service
```bash
dotnet UPEE.Service\bin\Release\net10.0\UPEE.Service.dll install
```

## Utilisation

### Initialiser un Workspace
```bash
upee connect "C:\Mon\Workspace"
```

Cela crée la structure:
```
C:\Mon\Workspace\UPEE\
├── scripts/         # Vos scripts polyglottes
├── libs/            # Bibliothèques partagées
├── logs/            # Historique d'exécution
├── bin/             # Binaires compilés
└── core/            # Configuration système
```

### Créer un Script Exécutable

**exemple.py** avec directive UPEE:
```python
// UPEE_RUN_PYTHON_PRIORITY=2

def main():
	print("Hello from UPEE!")

if __name__ == "__main__":
	main()
```

### Exécuter un Script
```bash
upee execute -s exemple.py
```

### Mode Surveillance
```bash
upee execute -w
# Surveille le dossier scripts/ et exécute automatiquement les nouveaux fichiers
```

## Format des Directives

```
// UPEE_<COMMAND>_<MODULE>_<OPTIONS>
```

### Exemples
- `// UPEE_RUN_CPP` - Exécuter du code C++
- `// UPEE_COMPILE_RUST_PRIORITY=3` - Compiler du Rust avec priorité Critical
- `// UPEE_EXEC_PYTHON` - Exécuter du Python
- `// UPEE_RUN_GO_TIMEOUT=30` - Exécuter du Go avec timeout de 30s

### Modules Supportés
- `CPP` - C/C++
- `RUST` - Rust
- `PYTHON` - Python
- `GO` - Go
- `JS` - JavaScript/Node.js
- `LUA` - Lua
- `SHELL` - Bash/Sh
- `DEFAULT` - Exécuteur par défaut

### Commandes
- `RUN` - Exécuter directement
- `COMPILE` - Compiler puis exécuter
- `EXEC` - Exécuter un binaire compilé

### Priorités
- `0` ou `Low` - Tâches en arrière-plan
- `1` ou `Medium` - Tâches standard (défaut)
- `2` ou `High` - Tâches prioritaires
- `3` ou `Critical` - Tâches système critiques

## Logging

Les logs sont stockés dans `/logs/history.log` avec le format:
```
[2024-07-01 14:30:45.123] [ERROR   ] [Engine               ] Description de l'erreur
```

## API Programmatique

```csharp
using UPEE.Runtime;

var manager = new UPEERuntimeManager("C:\\UPEE\\Workspace");

// Exécuter un script
var result = await manager.ExecuteScriptAsync("script.py");
Console.WriteLine(result.Success ? "OK" : $"Error: {result.Error}");

// Variables globales
manager.SetGlobalVariable("myVar", "value");
var value = manager.GetGlobalVariable<string>("myVar");

// Ordonnancer
manager.ScheduleScript("script.py", ExecutionPriority.High);
```

## Système de Sandbox

Chaque script s'exécute dans un environnement isolé:
- Mémoire limitée (configurable)
- Timeout d'exécution (défaut: 30s)
- Accès fichiers limité au workspace
- Isolation processus complète

## Performance

- **Scheduler**: Gère jusqu'à 4 tâches concurrentes
- **Mémoire partagée**: JSON avec lock-free reads
- **Logging asynchrone**: Buffer 1 seconde avant écriture
- **Sandbox**: Process isolation avec nettoyage automatique

## État de la Compilation

✅ **BUILD SUCCESS** - Tous les projets compilent avec succès
- UPEE.Core: ✓
- UPEE.CLI: ✓
- UPEE.Service: ✓
- UPEE.Runtime: ✓
- UPEE.Tests: ✓ (14/14 tests réussis)

## Limitations Actuelles

- Pas de multi-nœud distribué (prévu v2.0)
- Pas de persistance de base de données (utiliser JSON files)
- Sandbox limité au processus local (pas de conteneurs)

## Roadmap

- v1.1: Support Docker container execution
- v1.2: Distributed task orchestration
- v2.0: Multi-machine cluster mode
- v2.1: Web dashboard + REST API

## Licence

UPEE est fourni tel quel à titre d'exemple d'architecture système professionnelle.

## Support

Pour plus d'informations sur l'architecture:
- Lisez les commentaires XML dans les fichiers source
- Consultez les tests unitaires pour les cas d'usage
- Analysez les logs dans `/logs/history.log`

---

**Version**: 1.0.0  
**Date**: 2024-07-01  
**Status**: Production Ready ✓
