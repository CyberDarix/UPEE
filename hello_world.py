#!/usr/bin/env python3
# UPEE_RUN_PYTHON_PRIORITY=1

"""
Exemple de script UPEE - Hello World
Directive: UPEE_RUN_PYTHON_PRIORITY=1
- Commande: RUN (exécution directe)
- Module: PYTHON
- Priorité: 1 (Medium)
"""

import json
import sys
from datetime import datetime

def main():
	"""Point d'entrée du script"""

	print("=" * 60)
	print("🎉 UPEE Script Exécution - Hello World")
	print("=" * 60)

	# Informations d'exécution
	execution_info = {
		"timestamp": datetime.now().isoformat(),
		"script_name": "hello_world.py",
		"status": "success",
		"message": "Hello from UPEE!",
		"python_version": sys.version,
		"platform": sys.platform,
		"environment": {
			"UPEE_SANDBOX_ID": os.environ.get("UPEE_SANDBOX_ID", "N/A"),
			"UPEE_WORKSPACE": os.environ.get("UPEE_WORKSPACE", "N/A"),
		}
	}

	# Afficher les informations
	print("\n📋 Informations d'exécution:")
	print(json.dumps(execution_info, indent=2, ensure_ascii=False, default=str))

	print("\n✅ Exécution complétée avec succès")
	print("=" * 60)

if __name__ == "__main__":
	import os
	main()
