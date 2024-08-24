from bs4 import BeautifulSoup

with open('generated_files/php_version.xml','r') as file:
    content = file.read()

soup = BeautifulSoup(content, 'lxml-xml')

elements = soup.select('h3[id^=v8]')

file_type="tar.bz2"

for element in elements:
    version=(element.get('id')[1:])
    content_div=element.find_next_sibling()
    ul_tag = content_div.find('ul')
    li_elements=ul_tag.find_all('li')

    search_string=f"{version}.{file_type}"

    for li_element in li_elements:
        if(search_string in li_element.text):
            sha256_spans = li_element.find_all(class_='sha256')
            if sha256_spans:
                x=version[2]

                with open('generated_files/php_latest_versions.txt', 'a') as version_file:
                    version_file.write(f"php8{x}Version={version},")
                    version_file.write(f"php8{x}Version_SHA={sha256_spans[0].text}\n")
