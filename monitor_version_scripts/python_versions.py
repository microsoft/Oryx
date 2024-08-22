from bs4 import BeautifulSoup
import requests

url = "https://www.python.org/downloads/"
response = requests.get(url)

with open("python_version.xml", "w", encoding="utf-8") as file:
    file.write(BeautifulSoup(response.content, 'html.parser').prettify())

with open('python_version.xml','r') as file:
    content = file.read()

soup = BeautifulSoup(content, 'lxml-xml')

version_table = soup.select('.row.download-list-widget .list-row-container.menu li')
for version_tab in version_table:
    # version=version_tab.find(class__='release-number').find('a').text
    # print(f"{version}\n")
    version_full=version_tab.select('.release-number a')[0].text

    # Initialize variables to store the indices
    first_numeric_index = None
    last_numeric_index = None

    # Iterate through the string to find the indices
    for i, char in enumerate(version_full):
        if char.isdigit():
            if first_numeric_index is None:
                first_numeric_index = i
            last_numeric_index = i

    version=version_full[first_numeric_index:last_numeric_index+1]
    index_of_colon1=version_full.find('.')
    first_part=version_full[first_numeric_index:index_of_colon1]
    index_of_colon2=version_full.find('.',index_of_colon1+1)
    second_part=version_full[index_of_colon1+1:index_of_colon2]
    key="".join(["python",first_part,second_part,"Version"])

    with open('python_latest_versions.txt', 'a') as version_file:
        version_file.write(f"{key}={version}\n")
