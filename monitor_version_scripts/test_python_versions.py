#!/usr/bin/env python3
"""
Test script for Python version updates from endoflife.date API

This script can be used to:
1. Test the Python version fetching logic
2. Simulate API responses when the API is unavailable
3. Verify the update workflow
"""
import requests
import json
from datetime import date
import sys
import os

def fetch_python_versions(use_mock=False):
    """
    Fetch Python versions from endoflife.date API or use mock data
    
    Args:
        use_mock (bool): If True, use mock data instead of API
        
    Returns:
        list: List of Python version information
    """
    if use_mock:
        print("Using mock data (API unavailable)", file=sys.stderr)
        # Mock data representing a typical API response
        json_data = [
            {"cycle": "3.14", "latest": "3.14.0rc2", "eol": False},
            {"cycle": "3.13", "latest": "3.13.5", "eol": "2029-10-01"},
            {"cycle": "3.12", "latest": "3.12.11", "eol": "2028-10-02"},
            {"cycle": "3.11", "latest": "3.11.13", "eol": "2027-10-24"},
            {"cycle": "3.10", "latest": "3.10.18", "eol": "2026-10-04"},
            {"cycle": "3.9", "latest": "3.9.23", "eol": "2025-10-05"},
            {"cycle": "3.8", "latest": "3.8.20", "eol": "2024-10-07"}
        ]
    else:
        try:
            print("Fetching from API...", file=sys.stderr)
            response = requests.get('https://endoflife.date/api/python.json', timeout=10)
            response.raise_for_status()
            json_data = response.json()
            print(f"Successfully fetched {len(json_data)} Python versions", file=sys.stderr)
        except Exception as e:
            print(f"Error fetching from API: {e}", file=sys.stderr)
            print("Falling back to mock data", file=sys.stderr)
            return fetch_python_versions(use_mock=True)
    
    return json_data

def filter_active_versions(json_data):
    """
    Filter Python versions that are still active (not EOL)
    
    Args:
        json_data (list): List of Python version information from API
        
    Returns:
        list: List of active Python versions
    """
    todays_date = date.today().strftime("%Y-%m-%d")
    active_versions = []
    
    for element in json_data:
        # Include versions where eol is False (no EOL date) or eol is after today
        if element["eol"] == False or element["eol"] > todays_date:
            version = element["latest"]
            major_version = element["cycle"].replace('.', '')
            active_versions.append({
                'cycle': element["cycle"],
                'major_version': major_version,
                'version': version,
                'eol': element["eol"]
            })
    
    return active_versions

def write_versions_file(active_versions, output_file='generated_files/python_latest_versions.txt'):
    """
    Write Python versions to output file in the format expected by the update workflow
    
    Args:
        active_versions (list): List of active Python versions
        output_file (str): Path to output file
    """
    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    
    with open(output_file, 'w') as version_file:
        for v in active_versions:
            version_file.write(f"python{v['major_version']}Version={v['version']}\n")
    
    print(f"Wrote {len(active_versions)} versions to {output_file}", file=sys.stderr)

def main():
    """Main function to fetch and display Python versions"""
    use_mock = '--mock' in sys.argv or '--test' in sys.argv
    write_file = '--write' in sys.argv
    
    # Fetch versions
    json_data = fetch_python_versions(use_mock=use_mock)
    
    # Filter active versions
    active_versions = filter_active_versions(json_data)
    
    # Display results
    print("\nActive Python versions (not EOL):")
    print("=" * 70)
    for v in active_versions:
        eol_str = v['eol'] if v['eol'] != False else 'No EOL date'
        print(f"Python {v['cycle']:5} : {v['version']:12} (EOL: {eol_str})")
    
    print("\n" + "=" * 70)
    print("Constants format (for constants.yml):")
    print("=" * 70)
    for v in active_versions:
        print(f"python{v['major_version']}Version: {v['version']}")
    
    # Write to file if requested
    if write_file:
        write_versions_file(active_versions)
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
