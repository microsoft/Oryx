#!/usr/bin/env python3
"""
Auto-update stack versions in Oryx.
Checks upstream APIs for new releases, updates images/constants.yml
and versionsToBuild.txt files.

Usage:
  python auto-update-stack.py --stack node|python|php|dotnet [--dry-run]
"""

import argparse
import hashlib
import json
import os
import re
import sys
import urllib.request

REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
CONSTANTS_YML = os.path.join(REPO_ROOT, 'images', 'constants.yml')


def read_constants():
    with open(CONSTANTS_YML) as f:
        return f.read()


def write_constants(content):
    with open(CONSTANTS_YML, 'w') as f:
        f.write(content)


def fetch_json(url):
    req = urllib.request.Request(url, headers={'User-Agent': 'Oryx-AutoUpdate/1.0'})
    with urllib.request.urlopen(req, timeout=30) as resp:
        return json.loads(resp.read())


def version_tuple(v):
    return tuple(int(x) for x in re.split(r'[.\-]', v) if x.isdigit())


def get_current_version(content, key):
    m = re.search(rf'^\s*{re.escape(key)}:\s*["\']?([^"\'#\n]+)', content, re.MULTILINE)
    return m.group(1).strip() if m else None


def update_constant(content, key, value):
    pattern = rf'(^\s*{re.escape(key)}:\s*)["\']?[^"\'#\n]+["\']?'
    replacement = rf'\g<1>{value}'
    new_content, count = re.subn(pattern, replacement, content, count=1, flags=re.MULTILINE)
    if count == 0:
        print(f"  WARNING: Key '{key}' not found in constants.yml")
    return new_content


def get_os_flavors(content, stack, major):
    key = f"{stack}{major.replace('.', '')}osFlavors"
    val = get_current_version(content, key)
    return [flavor.strip() for flavor in val.split(',')] if val else []


def append_to_versions_to_build(stack, flavor, line):
    path = os.path.join(REPO_ROOT, 'platforms', stack, 'versions', flavor, 'versionsToBuild.txt')
    if not os.path.exists(path):
        print(f"  WARNING: {path} does not exist, skipping")
        return

    with open(path) as f:
        existing_lines = f.read().splitlines()

    version = line.split(',', 1)[0].strip()
    header_lines = []
    version_lines = []
    for existing_line in existing_lines:
        stripped = existing_line.strip()
        if not stripped or stripped.startswith('#'):
            header_lines.append(existing_line)
            continue
        version_lines.append(existing_line)

    updated = False
    for index, existing_line in enumerate(version_lines):
        existing_version = existing_line.split(',', 1)[0].strip()
        if existing_version == version:
            if existing_line.strip() == line.strip():
                print(f"  {flavor}: {version} already up to date in versionsToBuild.txt")
                return
            version_lines[index] = line
            updated = True
            break

    if not updated:
        version_lines.append(line)

    version_lines.sort(key=lambda item: version_tuple(item.split(',', 1)[0].strip()))

    with open(path, 'w') as f:
        output_lines = header_lines + version_lines
        if output_lines:
            f.write('\n'.join(output_lines))

    action = 'updated' if updated else 'inserted'
    print(f"  {flavor}: {action} {version} in ascending order")


# --- Node ---
def check_node(dry_run):
    print("=== Node.js ===")
    content = read_constants()
    releases = fetch_json("https://nodejs.org/dist/index.json")

    tracked = {}
    for major in ['18', '20', '22', '24']:
        tracked[major] = get_current_version(content, f'node{major}Version')

    updates = {}
    for rel in releases:
        ver = rel['version'].lstrip('v')
        major = ver.split('.')[0]
        if major in tracked:
            if tracked[major] and version_tuple(ver) > version_tuple(tracked[major]):
                if major not in updates or version_tuple(ver) > version_tuple(updates[major]):
                    updates[major] = ver

    if not updates:
        print("  No new versions found")
        return None

    for major, ver in updates.items():
        print(f"  Node {major}: {tracked[major]} -> {ver}")
        if not dry_run:
            content = update_constant(content, f'node{major}Version', ver)
            for flavor in get_os_flavors(content, 'node', major):
                append_to_versions_to_build('nodejs', flavor, ver)

    if not dry_run:
        write_constants(content)
    return ', '.join(f'Node {m} {v}' for m, v in updates.items())


# --- Python ---
def fetch_python_sha256(version):
    tarball_url = f"https://www.python.org/ftp/python/{version}/Python-{version}.tar.xz"
    try:
        releases = fetch_json(f"https://www.python.org/api/v2/downloads/release/?name=Python+{version}")
        if not releases:
            return ''
        release_id = releases[0].get('resource_uri', '').rstrip('/').split('/')[-1]
        files = fetch_json(f"https://www.python.org/api/v2/downloads/release_file/?release={release_id}")
        for f in files:
            if f.get('url', '').endswith('.tar.xz'):
                sha = f.get('sha256_sum', '')
                if sha:
                    return sha.upper()
                tarball_url = f.get('url', tarball_url)
                break
    except Exception as e:
        print(f"  WARNING: API lookup failed for Python {version}: {e}")
    # Fallback: download tarball and compute SHA256 when API field is empty
    try:
        req = urllib.request.Request(tarball_url, headers={'User-Agent': 'Oryx-AutoUpdate/1.0'})
        h = hashlib.sha256()
        with urllib.request.urlopen(req, timeout=120) as resp:
            while True:
                chunk = resp.read(65536)
                if not chunk:
                    break
                h.update(chunk)
        return h.hexdigest().upper()
    except Exception as e:
        print(f"  WARNING: Failed to compute SHA256 for Python {version}: {e}")
    return ''


def check_python(dry_run):
    print("=== Python ===")
    content = read_constants()
    releases = fetch_json("https://www.python.org/api/v2/downloads/release/")

    tracked = {}
    for minor in ['3.9', '3.10', '3.11', '3.12', '3.13', '3.14']:
        key = f'python{minor.replace(".", "")}Version'
        tracked[minor] = get_current_version(content, key)

    updates = {}
    for rel in releases:
        if rel.get('pre_release'):
            continue
        ver = rel['name'].replace('Python ', '').strip()
        parts = ver.split('.')
        if len(parts) < 3:
            continue
        minor = f"{parts[0]}.{parts[1]}"
        if minor in tracked and tracked[minor] and version_tuple(ver) > version_tuple(tracked[minor]):
            if minor not in updates or version_tuple(ver) > version_tuple(updates[minor]):
                updates[minor] = ver

    if not updates:
        print("  No new versions found")
        return None

    for minor, ver in updates.items():
        key = f'python{minor.replace(".", "")}Version'
        print(f"  Python {minor}: {tracked[minor]} -> {ver}")
        # SHA256 is only needed for Python 3.14+ onward
        needs_sha = version_tuple(minor) >= version_tuple('3.14')
        sha = fetch_python_sha256(ver) if needs_sha else ''
        if needs_sha and not sha:
            print(f"  WARNING: No SHA256 found for Python {ver}, skipping versionsToBuild update")
        if not dry_run:
            content = update_constant(content, key, ver)
            gpg = get_current_version(content, f'python{minor.replace(".", "")}_GPG_keys') or ''
            for flavor in get_os_flavors(content, 'python', minor):
                if needs_sha:
                    if sha:
                        append_to_versions_to_build('python', flavor, f'{ver}, {gpg}, {sha},')
                else:
                    append_to_versions_to_build('python', flavor, f'{ver}, {gpg},')

    if not dry_run:
        write_constants(content)
    return ', '.join(f'Python {m} {v}' for m, v in updates.items())


# --- PHP ---
def check_php(dry_run):
    print("=== PHP ===")
    content = read_constants()

    tracked = {}
    for minor in ['8.1', '8.2', '8.3', '8.4', '8.5']:
        key = f'php{minor.replace(".", "")}Version'
        tracked[minor] = get_current_version(content, key)

    updates = {}
    for minor in tracked:
        major = minor.split('.')[0]
        try:
            data = fetch_json(f"https://www.php.net/releases/index.php?json&version={minor}")
            ver = data.get('version')
            if ver and tracked[minor] and version_tuple(ver) > version_tuple(tracked[minor]):
                sha = None
                for src in data.get('source', []):
                    if src['filename'].endswith('.tar.xz'):
                        sha = src.get('sha256')
                        break
                if sha:
                    updates[minor] = {'version': ver, 'sha': sha}
        except Exception as e:
            print(f"  WARNING: Failed to fetch PHP {minor}: {e}")

    if not updates:
        print("  No new versions found")
        return None

    for minor, info in updates.items():
        key = f'php{minor.replace(".", "")}Version'
        sha_key = f'php{minor.replace(".", "")}Version_SHA'
        ver = info['version']
        sha = info['sha']
        print(f"  PHP {minor}: {tracked[minor]} -> {ver}")
        if not dry_run:
            content = update_constant(content, key, ver)
            content = update_constant(content, sha_key, sha)
            gpg = get_current_version(content, f'php{minor.replace(".", "")}_GPG_keys') or ''
            for flavor in get_os_flavors(content, 'php', minor):
                append_to_versions_to_build('php', flavor, f'{ver}, {sha}, {gpg},')

    if not dry_run:
        write_constants(content)
    return ', '.join(f'PHP {m} {i["version"]}' for m, i in updates.items())


# --- .NET ---
def check_dotnet(dry_run):
    print("=== .NET ===")
    content = read_constants()
    index = fetch_json("https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json")

    tracked = {}
    for channel in ['8.0', '9.0', '10.0']:
        key = channel.replace('.', '')
        tracked[channel] = {
            'sdk': get_current_version(content, f'DOTNET_SDK_{key}'),
            'netcore': get_current_version(content, f'NET_CORE_APP_{key}'),
            'aspnet': get_current_version(content, f'ASPNET_CORE_APP_{key}'),
        }

    updates = {}
    for entry in index.get('releases-index', []):
        ch = entry['channel-version']
        if ch not in tracked or entry.get('support-phase') in ('preview', 'eol'):
            continue

        releases_url = entry['releases.json']
        try:
            releases_data = fetch_json(releases_url)
        except Exception as e:
            print(f"  WARNING: Failed to fetch .NET {ch} releases: {e}")
            continue

        latest = releases_data.get('releases', [{}])[0]
        sdk_ver = latest.get('sdk', {}).get('version')
        runtime_ver = latest.get('runtime', {}).get('version')
        aspnet_ver = latest.get('aspnetcore-runtime', {}).get('version')

        if not sdk_ver or not runtime_ver:
            continue

        current = tracked[ch]
        if sdk_ver != current['sdk'] or runtime_ver != current['netcore']:
            sdk_sha = get_linux_sha(latest.get('sdk', {}).get('files', []))
            netcore_sha = get_linux_sha(latest.get('runtime', {}).get('files', []))
            aspnet_sha = get_linux_sha(latest.get('aspnetcore-runtime', {}).get('files', []))
            updates[ch] = {
                'sdk': sdk_ver, 'sdk_sha': sdk_sha,
                'netcore': runtime_ver, 'netcore_sha': netcore_sha,
                'aspnet': aspnet_ver, 'aspnet_sha': aspnet_sha,
            }

    if not updates:
        print("  No new versions found")
        return None

    for ch, info in updates.items():
        key = ch.replace('.', '')
        print(f"  .NET {ch}: SDK {tracked[ch]['sdk']} -> {info['sdk']}, Runtime {tracked[ch]['netcore']} -> {info['netcore']}")
        if not dry_run:
            content = update_constant(content, f'DOTNET_SDK_{key}', info['sdk'])
            content = update_constant(content, f'NET_CORE_APP_{key}', info['netcore'])
            if info['netcore_sha']:
                content = update_constant(content, f'NET_CORE_APP_{key}_SHA', info['netcore_sha'])
            content = update_constant(content, f'ASPNET_CORE_APP_{key}', info['aspnet'])
            if info['aspnet_sha']:
                content = update_constant(content, f'ASPNET_CORE_APP_{key}_SHA', info['aspnet_sha'])
            for flavor in get_os_flavors(content, 'dotnet', ch):
                append_to_versions_to_build('dotnet', flavor, f'{info["sdk"]}, {info["sdk_sha"]},')

    if not dry_run:
        write_constants(content)
    return ', '.join(f'.NET {ch} {i["netcore"]}' for ch, i in updates.items())


def get_linux_sha(files):
    for f in files:
        name = f.get('name', '')
        if 'linux-x64.tar.gz' in name and not name.endswith('.sha512'):
            return f.get('hash', '')
    return ''


STACKS = {
    'node': check_node,
    'python': check_python,
    'php': check_php,
    'dotnet': check_dotnet,
}


def main():
    parser = argparse.ArgumentParser(description='Auto-update Oryx stack versions')
    parser.add_argument('--stack', required=True, choices=list(STACKS.keys()) + ['all'])
    parser.add_argument('--dry-run', action='store_true', help='Show what would change without writing')
    args = parser.parse_args()

    try:
        stacks = STACKS.keys() if args.stack == 'all' else [args.stack]
        all_summaries = []
        for stack in stacks:
            summary = STACKS[stack](args.dry_run)
            if summary:
                all_summaries.append(summary)
    except Exception as e:
        print(f"\nERROR: {e}", file=sys.stderr)
        sys.exit(2)

    gh_output = os.environ.get('GITHUB_OUTPUT')
    if gh_output:
        try:
            with open(gh_output, 'a') as f:
                if all_summaries:
                    f.write(f"summary={', '.join(all_summaries)}\n")
                else:
                    f.write("no_updates=true\n")
        except OSError:
            pass

    if not all_summaries:
        print("\nNo updates found.")
    elif args.dry_run:
        print("\nDry run — no files changed.")
    else:
        print("\nFiles updated. Ready for PR.")

    sys.exit(0)


if __name__ == '__main__':
    main()
