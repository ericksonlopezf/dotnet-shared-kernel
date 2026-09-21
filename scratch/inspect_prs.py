import urllib.request
import json
import os

token = ''
with open(os.path.expanduser('~/.git-credentials')) as f:
    for line in f:
        if 'github.com' in line:
            token = line.strip().split('//')[1].split('@')[0].split(':')[1]

headers = {'Authorization': f'Bearer {token}', 'Accept': 'application/vnd.github.v3+json', 'User-Agent': 'Python'}

prs = [19, 20, 21, 22, 28]
for num in prs:
    req = urllib.request.Request(f'https://api.github.com/repos/ericksonlopezf/dotnet-shared-kernel/pulls/{num}', headers=headers)
    with urllib.request.urlopen(req) as resp:
        p = json.loads(resp.read().decode())
    print(f"PR #{num}: {p['title']}")
    print(f"  Mergeable: {p.get('mergeable')}, MergeableState: {p.get('mergeable_state')}")
    print(f"  Head: {p['head']['ref']} ({p['head']['sha'][:7]})")
