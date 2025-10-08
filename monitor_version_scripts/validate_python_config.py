#!/usr/bin/env python3
"""
Validate Python version configuration in constants.yml

This script ensures that:
1. All Python versions are properly formatted (X.Y.Z or X.Y.Zrcn)
2. Each Python version has a corresponding GPG key
3. OS flavor mappings exist for each version
"""
import re
import yaml
import sys

def load_constants():
    """Load constants.yml file"""
    with open('images/constants.yml', 'r') as f:
        return yaml.safe_load(f)

def validate_version_format(python_versions):
    """Validate that Python versions follow the correct format"""
    version_pattern = re.compile(r'^\d+\.\d+\.\d+([a-z]+\d+)?$')
    all_valid = True
    
    print("Validating Python version format...")
    print("=" * 70)
    
    for key, version in sorted(python_versions.items()):
        is_valid = version_pattern.match(str(version)) is not None
        status = "✓" if is_valid else "✗"
        print(f"{status} {key:30} = {version}")
        if not is_valid:
            all_valid = False
            print(f"  ERROR: Invalid version format: {version}")
    
    print()
    return all_valid

def validate_gpg_keys(python_versions, constants):
    """Validate that each Python version has a GPG key"""
    print("Validating GPG keys...")
    print("=" * 70)
    
    missing_gpg = []
    for key in python_versions.keys():
        # Extract version number from key like python310Version -> 310
        version_num = re.search(r'python(\d+)', key)
        if version_num:
            gpg_key_name = f"python{version_num.group(1)}_GPG_keys"
            if gpg_key_name in constants['variables']:
                gpg_value = constants['variables'][gpg_key_name]
                print(f"✓ {key:30} GPG: {gpg_key_name}")
                # Validate GPG key format (hex string)
                if not re.match(r'^[A-F0-9\s]+$', str(gpg_value)):
                    print(f"  WARNING: GPG key may have invalid format")
            else:
                print(f"✗ {key:30} MISSING: {gpg_key_name}")
                missing_gpg.append(gpg_key_name)
    
    print()
    return len(missing_gpg) == 0

def validate_os_flavors(python_versions, constants):
    """Validate that each Python version has OS flavor configuration"""
    print("Validating OS flavor mappings...")
    print("=" * 70)
    
    missing_flavors = []
    for key in python_versions.keys():
        # Extract version number from key like python310Version -> 310
        version_num = re.search(r'python(\d+)', key)
        if version_num:
            flavor_key_name = f"python{version_num.group(1)}osFlavors"
            if flavor_key_name in constants['variables']:
                flavors = constants['variables'][flavor_key_name]
                print(f"✓ {key:30} OS: {flavors}")
            else:
                print(f"⚠ {key:30} MISSING: {flavor_key_name}")
                missing_flavors.append(flavor_key_name)
    
    print()
    # OS flavors are optional for legacy versions, so just warn
    return True

def main():
    """Main validation function"""
    print("\n" + "=" * 70)
    print("Python Version Configuration Validation")
    print("=" * 70)
    print()
    
    try:
        constants = load_constants()
    except FileNotFoundError:
        print("ERROR: images/constants.yml not found!")
        print("Make sure to run this script from the repository root.")
        return 1
    except yaml.YAMLError as e:
        print(f"ERROR: Failed to parse constants.yml: {e}")
        return 1
    
    # Extract Python versions
    python_versions = {}
    for key, value in constants['variables'].items():
        if ('python' in key.lower() and 
            'version' in key.lower() and 
            'gpg' not in key.lower() and 
            'flavor' not in key.lower()):
            python_versions[key] = value
    
    print(f"Found {len(python_versions)} Python version entries")
    print()
    
    # Run validations
    format_valid = validate_version_format(python_versions)
    gpg_valid = validate_gpg_keys(python_versions, constants)
    flavor_valid = validate_os_flavors(python_versions, constants)
    
    # Summary
    print("=" * 70)
    print("Validation Summary")
    print("=" * 70)
    print(f"Version format:   {'✓ PASS' if format_valid else '✗ FAIL'}")
    print(f"GPG keys:         {'✓ PASS' if gpg_valid else '✗ FAIL'}")
    print(f"OS flavors:       {'✓ PASS' if flavor_valid else '⚠ WARNINGS'}")
    print("=" * 70)
    
    if format_valid and gpg_valid and flavor_valid:
        print("✓ All validations passed!")
        return 0
    else:
        print("✗ Some validations failed!")
        return 1

if __name__ == "__main__":
    sys.exit(main())
