from bs4 import BeautifulSoup
import requests

with open('generated_files/dotnet_version.xml','r') as file:
    content = file.read()

soup = BeautifulSoup(content, 'lxml-xml')

version_table=soup.select('#supported-versions-table .table tr')
print("Available Dotnet Versions on Web")

def scrape_CheckSum(version,category,type):
    runtime_version="".join([category,version])
    # type is sdk or runtime
    url=f"https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/{type}-{runtime_version}-linux-x64-binaries"
    response=requests.get(url)
    html_content=response.text

    # Parse the HTML content with BeautifulSoup
    soup = BeautifulSoup(html_content, 'html.parser')
    
    check_sum=soup.select('#checksum')[0]['value']
    return check_sum

def scrap_sdk_versions(HTML_CONTENT,ASPNET_runtime_version,NET_runtime_version):
    all_version_tags=HTML_CONTENT.select('h3')
    all_description_tags=HTML_CONTENT.select('dl')
    for index,version_tag in enumerate(all_version_tags):
        if "sdk" in version_tag.get('id'):
            sdk_version=(version_tag.text).replace("SDK ","").strip()
            check_sum=scrape_CheckSum(sdk_version,"","sdk")

            # This is to check for full_version
            if(len(all_description_tags)>index):
                description_title_tags=all_description_tags[index].select('dt')

                for title_tag in description_title_tags:
                    if "Full version" in title_tag.text:
                        full_version=title_tag.find_next_sibling().text
                        sdk_version=full_version
            
            print(f"sdk_version is {sdk_version}, SHA is {check_sum}")
            with open('generated_files/dotnet_sdk_latest_versions.txt', 'a') as version_file:
                version_file.write(f"{ASPNET_runtime_version}:{sdk_version}, {check_sum},\n") 
                version_file.write(f"{NET_runtime_version}:{sdk_version}, {check_sum},\n")

    return

def scrap_runtime_versions(HTML_CONTENT,major_version):
    all_version_tags=HTML_CONTENT.select('h3')
    all_description_tags=HTML_CONTENT.select('dl')
    x=major_version.replace('.','')
    ASPNET_runtime_version=None
    NET_runtime_version=None
    for index,version_tag in enumerate(all_version_tags):
        if ("runtime" in version_tag.get('id')) and not ("desktop" in version_tag.get('id')):
            if ("ASP.NET Core Runtime" in version_tag.text):
                runtime_version=(version_tag.text).replace("ASP.NET Core Runtime ","").strip()
                check_sum=scrape_CheckSum(runtime_version,"aspnetcore-","runtime")

                if(len(all_description_tags)>index):
                    description_title_tags=all_description_tags[index].select('dt')

                    for title_tag in description_title_tags:
                        if "Full version" in title_tag.text:
                            full_version=title_tag.find_next_sibling().text
                            runtime_version=full_version
                
                ASPNET_runtime_version=runtime_version
                print(f"ASP_runtime_version is {runtime_version}, SHA is {check_sum}")
                with open('generated_files/dotnet_latest_versions.txt', 'a') as version_file:
                    version_file.write(f"ASPNET_CORE_APP_{x}={runtime_version},")
                    version_file.write(f"ASPNET_CORE_APP_{x}_SHA={check_sum}\n")

            if (".NET Runtime" in version_tag.text):
                runtime_version=(version_tag.text).replace(".NET Runtime ","").strip()
                check_sum=scrape_CheckSum(runtime_version,"","runtime")

                if(len(all_description_tags)>index):
                    description_title_tags=all_description_tags[index].select('dt')

                    for title_tag in description_title_tags:
                        if "Full version" in title_tag.text:
                            full_version=title_tag.find_next_sibling().text
                            runtime_version=full_version

                NET_runtime_version=runtime_version
                print(f"NET_runtime_version is {runtime_version}, SHA is {check_sum}")
                with open('generated_files/dotnet_latest_versions.txt', 'a') as version_file:
                    version_file.write(f"NET_CORE_APP_{x}={runtime_version},")
                    version_file.write(f"NET_CORE_APP_{x}_SHA={check_sum}\n")    
        
    return [ASPNET_runtime_version,NET_runtime_version]

def scrap_particular_version(major_version,version):
    url=f"https://dotnet.microsoft.com/en-us/download/dotnet/{major_version}"
    response=requests.get(url)
    html_content=response.text

    # Parse the HTML content with BeautifulSoup
    soup = BeautifulSoup(html_content, 'html.parser')

    version_details=soup.select('.download-wrap .row .col-md-6')

    runtime_fullversions=scrap_runtime_versions(version_details[1],major_version)
    scrap_sdk_versions(version_details[0],runtime_fullversions[0],runtime_fullversions[1])
    
    return

for index,each_version in enumerate(version_table):
    if index !=0:
        version=each_version.find_all('td')[3].text
        split_version=version.split('.')

        major_version='.'.join(split_version[:2])
        
        print(f"version is {version}")

        scrap_particular_version(major_version,version)
    
