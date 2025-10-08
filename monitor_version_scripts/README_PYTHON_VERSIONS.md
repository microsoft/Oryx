# Python Version Update Testing

This directory contains scripts for fetching and updating Python versions from the endoflife.date API.

## Scripts

### `python_versions.py`
The main script used by the automated workflow to fetch Python versions. It:
- Fetches data from https://endoflife.date/api/python.json
- Filters versions that are still active (not EOL)
- Writes results to `generated_files/python_latest_versions.txt`

**Recent Fix**: Updated to handle versions with `eol: False` (versions without an end-of-life date, such as release candidates).

### `test_python_versions.py`
A test/verification script that:
- Can fetch from the API or use mock data
- Displays active Python versions in a readable format
- Helps verify the update workflow

## Usage

### Running the Main Script
```bash
cd monitor_version_scripts
python3 web_scrap_files/python_versions.py
```

This will create `generated_files/python_latest_versions.txt` with content like:
```
python314Version=3.14.0rc2
python313Version=3.13.5
python312Version=3.12.11
...
```

### Running the Test Script

To fetch from the API:
```bash
python3 test_python_versions.py
```

To use mock data (when API is unavailable):
```bash
python3 test_python_versions.py --mock
```

To write output to file:
```bash
python3 test_python_versions.py --mock --write
```

## Full Update Workflow

The complete update workflow is orchestrated by `monitor_variables.sh`:

1. **Fetch versions**: `python_versions.py` fetches from endoflife.date API
2. **Update constants**: `update_latest_stack_versions.sh` updates the YAML files
3. **Update build files**: `update_versions_to_build.sh` updates versionsToBuild.txt files
4. **Update constants.yml**: Final constants are written to `images/constants.yml`

## Python Version EOL Status

As of October 2025:
- Python 3.14: No EOL date (release candidate/new release)
- Python 3.13: EOL 2029-10-01
- Python 3.12: EOL 2028-10-02  
- Python 3.11: EOL 2027-10-24
- Python 3.10: EOL 2026-10-04
- Python 3.9: EOL 2025-10-05 (recently reached EOL)
- Python 3.8: EOL 2024-10-07 (past EOL)

## Bug Fix Detail

**Issue**: The original `python_versions.py` script used:
```python
if element["eol"] > todays_date:
```

This failed to include versions where `eol` is `False` (like Python 3.14 release candidates).

**Fix**: Updated to:
```python
if element["eol"] == False or element["eol"] > todays_date:
```

This properly handles both:
- Versions with no EOL date (`eol: False`)
- Versions with a future EOL date
