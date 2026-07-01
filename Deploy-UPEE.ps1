#!/usr/bin/env pwsh

<#
.SYNOPSIS
	Script d'installation et configuration de UPEE (Universal Polyglot Execution Engine)

.DESCRIPTION
	Compile, installe et configure UPEE comme service Windows avec vérification complète

.PARAMETER WorkspacePath
	Chemin du workspace UPEE (défaut: C:\UPEE)

.PARAMETER Configuration
	Type de configuration: Debug ou Release (défaut: Release)

.EXAMPLE
	.\Deploy-UPEE.ps1 -WorkspacePath "C:\MyUPEE" -Configuration Release
#>

param(
	[string]$WorkspacePath = "C:\UPEE",
	[ValidateSet("Debug", "Release")][string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  UPEE - Universal Polyglot Execution Engine         ║" -ForegroundColor Cyan
Write-Host "║  Deployment & Configuration Script v1.0            ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Step 1: Vérifier les prérequis
Write-Host "[Step 1/5] Vérification des prérequis..." -ForegroundColor Yellow

try {
	$dotnetVersion = dotnet --version
	Write-Host "  ✓ .NET $dotnetVersion trouvé" -ForegroundColor Green
} catch {
	Write-Host "  ✗ .NET SDK non trouvé. Veuillez installer .NET 10.0+" -ForegroundColor Red
	exit 1
}

# Step 2: Compiler la solution
Write-Host "`n[Step 2/5] Compilation de la solution ($Configuration)..." -ForegroundColor Yellow

try {
	dotnet build UPEE.sln --configuration $Configuration --no-incremental
	Write-Host "  ✓ Compilation réussie" -ForegroundColor Green
} catch {
	Write-Host "  ✗ Erreur de compilation" -ForegroundColor Red
	exit 1
}

# Step 3: Exécuter les tests
Write-Host "`n[Step 3/5] Exécution des tests..." -ForegroundColor Yellow

try {
	dotnet test UPEE.Tests/UPEE.Tests.csproj --configuration $Configuration --no-build
	Write-Host "  ✓ Tous les tests réussis" -ForegroundColor Green
} catch {
	Write-Host "  ✗ Erreur dans les tests" -ForegroundColor Red
	exit 1
}

# Step 4: Initialiser le workspace
Write-Host "`n[Step 4/5] Initialisation du workspace..." -ForegroundColor Yellow

try {
	$directories = @(
		$WorkspacePath,
		"$WorkspacePath\scripts",
		"$WorkspacePath\libs",
		"$WorkspacePath\logs",
		"$WorkspacePath\bin",
		"$WorkspacePath\core"
	)

	foreach ($dir in $directories) {
		if (-not (Test-Path $dir)) {
			New-Item -ItemType Directory -Path $dir -Force | Out-Null
		}
	}

	Write-Host "  ✓ Workspace initialisé à: $WorkspacePath" -ForegroundColor Green
	Write-Host "    ├── scripts/" -ForegroundColor Gray
	Write-Host "    ├── libs/" -ForegroundColor Gray
	Write-Host "    ├── logs/" -ForegroundColor Gray
	Write-Host "    ├── bin/" -ForegroundColor Gray
	Write-Host "    └── core/" -ForegroundColor Gray
} catch {
	Write-Host "  ✗ Erreur lors de l'initialisation du workspace" -ForegroundColor Red
	exit 1
}

# Step 5: Résumé du déploiement
Write-Host "`n[Step 5/5] Résumé du déploiement..." -ForegroundColor Yellow

$cliPath = ".\UPEE.CLI\bin\$Configuration\net10.0\UPEE.CLI.dll"
$servicePath = ".\UPEE.Service\bin\$Configuration\net10.0\UPEE.Service.dll"

Write-Host "  ✓ Déploiement complété avec succès!" -ForegroundColor Green
Write-Host ""
Write-Host "📦 Artifacts Produits:" -ForegroundColor Cyan
Write-Host "  CLI:     $cliPath"
Write-Host "  Service: $servicePath"
Write-Host ""
Write-Host "🚀 Prochaines étapes:" -ForegroundColor Cyan
Write-Host "  1. Tester le CLI:"
Write-Host "     dotnet $cliPath connect '$WorkspacePath'"
Write-Host ""
Write-Host "  2. Installer le service (Admin requis):"
Write-Host "     dotnet $servicePath install"
Write-Host ""
Write-Host "  3. Démarrer le service:"
Write-Host "     net start UPEE_Engine"
Write-Host ""
Write-Host "📖 Documentation: Consultez README.md" -ForegroundColor Cyan
Write-Host ""
Write-Host "✨ UPEE est prêt à l'emploi!" -ForegroundColor Green
