import os

workspace_root = r"d:\DevData\ericksonlopez.dev"
target_repos = [
    "dotnet-shared-kernel",
    "dotnet-auditing",
    "dotnet-domain-primitives",
    "dotnet-events",
    "dotnet-mapper",
    "dotnet-mediator",
    "dotnet-messaging",
    "dotnet-multitenancy",
    "dotnet-pagination",
    "dotnet-value-objects"
]

for repo in target_repos:
    print(f"=== {repo} ===")
    repo_path = os.path.join(workspace_root, repo)
    gate_script = os.path.join(repo_path, "scripts", "verify-mutation-gate.js")
    publish_yml = os.path.join(repo_path, ".github", "workflows", "publish.yml")
    mutation_yml = os.path.join(repo_path, ".github", "workflows", "mutation-testing.yml")

    if os.path.isfile(gate_script):
        with open(gate_script, "r", encoding="utf-8", errors="ignore") as f:
            c = f.read()
            print(f"  gate_script: {len(c.splitlines())} lines | has needsStryker: {'needsStryker' in c or 'needs_stryker' in c}")
    else:
        print("  gate_script: MISSING")

    if os.path.isfile(publish_yml):
        with open(publish_yml, "r", encoding="utf-8", errors="ignore") as f:
            c = f.read()
            print(f"  publish_yml: {len(c.splitlines())} lines | has stryker-gate job: {'stryker-gate' in c}")
    else:
        print("  publish_yml: MISSING")

    if os.path.isfile(mutation_yml):
        with open(mutation_yml, "r", encoding="utf-8", errors="ignore") as f:
            c = f.read()
            print(f"  mutation_yml: {len(c.splitlines())} lines | has workflow_call: {'workflow_call:' in c}")
    else:
        print("  mutation_yml: MISSING")
