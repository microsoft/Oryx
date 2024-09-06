import requests
from datetime import date
 
response = requests.get('https://endoflife.date/api/nodejs.json')
json_data = response.json()
 
todays_date = date.today().strftime("%Y-%m-%d")
 
for element in json_data:
    if element["eol"] != True and element["eol"] > todays_date:
        version = element["latest"]
        major_version = element["cycle"]
        with open('generated_files/node_latest_versions.txt', 'a') as version_file:
            version_file.write(f"node{major_version}Version={version}\n")