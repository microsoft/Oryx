import requests
from datetime import date
from bs4 import BeautifulSoup
 
with open('generated_files/php_version.xml', 'r') as file:
    content = file.read()
 
soup = BeautifulSoup(content, 'lxml-xml')
 
def getSHA(php_version):
    elements = soup.find("a", href=f"/distributions/php-{php_version}.tar.gz")
    if elements:
        element_SHA = elements.find_parent().find(class_="sha256").text
        return element_SHA
    else:
        return None
 
response = requests.get('https://endoflife.date/api/php.json')
json_data = response.json()
 
todays_date = date.today().strftime("%Y-%m-%d")
 
for element in json_data:
    if element["eol"] != True and element["eol"] > todays_date:
        version = element["latest"]
        version_SHA = getSHA(element["latest"])
        if version_SHA:
            x = element["cycle"].replace('.', '')
            with open('generated_files/php_latest_versions.txt', 'a') as version_file:
                version_file.write(f"php{x}Version={version},")
                version_file.write(f"php{x}Version_SHA={version_SHA}\n")