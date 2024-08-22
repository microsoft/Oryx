from bs4 import BeautifulSoup

with open('node_version.xml','r') as file:
    content = file.read()

soup = BeautifulSoup(content, 'lxml-xml')

versions_table=soup.find(id='tbVersions').find('tbody').find_all('tr')

for version_elements in versions_table:
    version_element = version_elements.find('td', {'data-label': 'Version'})
    index_of_colon = version_element.text.find('.')
    major_version=version_element.text[1:index_of_colon]

    # globals()[f'node{major_version}Version']=version_element.text[1:]

    with open('node_latest_versions.txt', 'a') as version_file:
        version_file.write(f"node{major_version}Version={version_element.text[1:]}\n")
