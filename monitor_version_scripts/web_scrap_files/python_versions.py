import requests
from datetime import date
 
response = requests.get('https://endoflife.date/api/python.json')
json_data = response.json()
 
todays_date = date.today().strftime("%Y-%m-%d")
 
for element in json_data:
    # Include versions where eol is False (no EOL date) or eol is after today
    if element["eol"] == False or element["eol"] > todays_date:
        version = element["latest"]
        major_version = element["cycle"].replace('.', '')
 
        with open('generated_files/python_latest_versions.txt', 'a') as version_file:
            version_file.write(f"python{major_version}Version={version}\n")