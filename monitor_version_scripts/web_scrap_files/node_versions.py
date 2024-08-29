from bs4 import BeautifulSoup

with open('generated_files/node_version.xml','r') as file:
    content = file.read()

soup = BeautifulSoup(content, 'lxml-xml')

versions_table=soup.find(id='tbVersions').find('tbody').find_all('tr')

print("Available Node Versions on Web")
for version_elements in versions_table:
    version_element = version_elements.find('td', {'data-label': 'Version'})
    index_of_colon = version_element.text.find('.')
    major_version=version_element.text[1:index_of_colon]

    with open('generated_files/node_latest_versions.txt', 'a') as version_file:
        print(f"{version_element.text[1:]}")
        version_file.write(f"node{major_version}Version={version_element.text[1:]}\n")
