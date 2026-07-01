# UPEE - Notes d'Architecture Technique

## Piliers Fondamentaux

### 1. Bus de Données Universel (UniversalDataBus)
**Fichier**: `UPEE.Core/Bus/UniversalDataBus.cs`

- **Thread-safe**: Utilise `ReaderWriterLockSlim` pour les lectures concurrentes
- **Sérialisation JSON**: Tous les objets sont sérialisés en JSON
- **Événements**: `DataChanged` déclenche sur Set/Remove/Clear
- **Performance**: O(1) pour Get/Set via `ConcurrentDictionary`

**Cas d'usage**:
```csharp
using UPEE.Core.Bus;

var bus = new UniversalDataBus();
bus.SetValue("user_data", new { name = "Alice", age = 30 });
var data = bus.GetValue<dynamic>("user_data");
```

### 2. Scheduler de Priorité (PriorityScheduler)
**Fichier**: `UPEE.Core/Scheduler/PriorityScheduler.cs`

- **4 niveaux**: Low (0), Medium (1), High (2), Critical (3)
- **Concurrence**: Par défaut 4 tâches simultanées
- **Queue pattern**: PriorityQueue<T, int> pour ordonnancement
- **Auto-cleanup**: Les tâches terminées libèrent les ressources

**Flux d'exécution**:
1. `EnqueueTask(task)` → Statut: Queued
2. Timer 100ms vérifie les disponibilités
3. `ExecuteTaskAsync()` → Statut: Running
4. Completion → Statut: Completed/Failed
5. Event `TaskCompleted` déclenché

### 3. Isolation - Sandbox (SandboxEnvironment)
**Fichier**: `UPEE.Core/Sandbox/SandboxEnvironment.cs`

- **Process-level isolation**: Chaque script = nouveau Process
- **Redirection stdio**: Capture Output/Error streams
- **Timeout enforcement**: `WaitForExit()` avec timeout
- **Cleanup automatique**: Suppression des répertoires temp

**Exécuteurs supportés**:
- `.py` → `python`
- `.cpp`/`.c` → `gcc`
- `.rs` → `rustc`
- `.sh` → `bash`
- `.lua` → `lua`
- `.js` → `node`

### 4. Logging Asynchrone (UniversalLogger)
**Fichier**: `UPEE.Core/Logging/UniversalLogger.cs`

- **Buffer asynchrone**: Flush toutes les 1 secondes
- **5 niveaux**: Debug, Info, Warning, Error, Critical
- **Format standardisé**: `[TIMESTAMP] [LEVEL] [COMPONENT] Message`
- **Couleurs console**: Différentes couleurs par niveau

**Performance**:
- Logs en mémoire → Buffer → Disque (non-blocking)
- Événement `LogEntryAdded` déclenché immédiatement
- Disque écrit par un Timer, pas par appel synchrone

### 5. Directive Header Parser (DirectiveHeader)
**Fichier**: `UPEE.Core/Models/DirectiveHeader.cs`

**Format**:
```
// UPEE_<COMMAND>_<MODULE>_[PRIORITY=X]_[TIMEOUT=Y]_[OPTIONS]
```

**Parsing**:
1. Vérifie le format `// UPEE_`
2. Split par `_`
3. Extrait Command et Module
4. Parse les options optionnelles
5. Retourne `DirectiveHeader` ou `null`

**Exemple**:
```
// UPEE_RUN_PYTHON_PRIORITY=3_TIMEOUT=60
→ Command: "RUN", Module: "PYTHON", Priority: 3, Timeout: "60"
```

### 6. Exécuteur Polyglotte (PolyglotsExecutionEngine)
**Fichier**: `UPEE.Core/Runtime/PolyglotsExecutionEngine.cs`

**Workflow complet**:

```
Script déposé dans /scripts/
	↓
Détecteur FileSystemWatcher
	↓
Parse directive header
	↓
Ordonnance dans PriorityScheduler
	↓
Création sandbox
	↓
Exécution du script
	↓
Capture stdout/stderr
	↓
Logging résultat
	↓
Stockage dans DataBus
	↓
Event TaskCompleted
```

## Performance & Optimisations

### Mémoire Partagée
- Pas de copying de données entre langages
- JSON comme format d'échange universel
- Sérialisation lazy (uniquement GetValue)

### Concurrence
- `ReaderWriterLockSlim` = multiple readers, un writer
- `ConcurrentDictionary` = thread-safe sans lock
- `PriorityQueue` = O(log n) insertion/extraction

### I/O
- Logging asynchrone (Buffer 1s)
- FileSystemWatcher pour watch (pas polling)
- Process cleanup via `using` et finalizers

## Diagramme d'Intégration

```
┌─────────────────────────────────────────────────────┐
│              CLI / Service Windows                  │
└─────────────────┬───────────────────────────────────┘
				  │
┌─────────────────▼───────────────────────────────────┐
│       PolyglotsExecutionEngine (Runtime)            │
│  ┌──────────────────────────────────────────────┐   │
│  │    FileSystemWatcher (Watch /scripts)        │   │
│  └──────────┬───────────────────────────────────┘   │
│             │                                       │
│  ┌──────────▼───────────────────────────────────┐   │
│  │  DirectiveHeader Parser                      │   │
│  └──────────┬───────────────────────────────────┘   │
│             │                                       │
│  ┌──────────▼───────────────────────────────────┐   │
│  │  PriorityScheduler                           │   │
│  │  ├─ Queue[Low]                               │   │
│  │  ├─ Queue[Medium]                            │   │
│  │  ├─ Queue[High]                              │   │
│  │  └─ Queue[Critical]                          │   │
│  └──────────┬───────────────────────────────────┘   │
│             │                                       │
│  ┌──────────▼───────────────────────────────────┐   │
│  │  SandboxEnvironment (Process Isolation)      │   │
│  └──────────┬───────────────────────────────────┘   │
│             │                                       │
│  ┌──────────▼───────────────────────────────────┐   │
│  │  UniversalLogger                             │   │
│  │  → /logs/history.log                         │   │
│  └──────────────────────────────────────────────┘   │
│                                                     │
│  ┌──────────────────────────────────────────────┐   │
│  │  UniversalDataBus (Shared Memory)            │   │
│  │  → JSON Key-Value Store                      │   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

## Cycle de Vie d'une Tâche

```
1. CREATION
   ExecutionTask { Name, Priority, ExecutionDelegate }

2. QUEUEING
   Status = TaskStatus.Queued
   EnqueuedAt = DateTime.Now

3. WAITING
   Attente dans la queue appropriée

4. SCHEDULING
   PriorityScheduler détecte disponibilité

5. EXECUTION
   Status = TaskStatus.Running
   StartedAt = DateTime.Now
   ExecuteTaskAsync() invoque le delegate

6. COMPLETION
   Status = TaskStatus.Completed/Failed
   CompletedAt = DateTime.Now
   Event TaskCompleted déclenché

7. CLEANUP
   Ressources libérées
   Logs écrites
```

## Tests Unitaires

**14 tests réussis** couvrant:
- ✅ DirectiveHeader parsing (4 tests)
- ✅ UniversalDataBus (5 tests)
- ✅ PriorityScheduler (3 tests)
- ✅ UniversalLogger (2 tests)

## Limitations Actuelles & Améliorations Futures

### v1.0 (Actuel)
- ✅ Single-machine execution
- ✅ Process-level isolation
- ✅ Priority scheduling
- ✅ Shared memory (JSON)
- ✅ Async logging

### v1.1 (Prévu)
- [ ] Docker container support
- [ ] REST API endpoints
- [ ] Web dashboard
- [ ] Database persistence

### v2.0 (Long terme)
- [ ] Multi-node orchestration
- [ ] Kubernetes integration
- [ ] Distributed tracing
- [ ] Machine learning optimization

## Sécurité

1. **Process Isolation**: Chaque script en processus séparé
2. **Memory Limits**: Configurable par sandbox
3. **Timeout Enforcement**: Kill si dépassement
4. **File Access**: Limité au workspace
5. **No Shell Injection**: Utilise ProcessStartInfo avec redirects

## Conclusion

UPEE fournit une fondation robuste et extensible pour l'exécution multi-langages. Son architecture modulaire permet une adoption progressive des fonctionnalités avancées sans compromettre la stabilité du système.

**Production Ready**: ✅ Yes
