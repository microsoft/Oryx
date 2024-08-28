from bs4 import BeautifulSoup
import requests

with open('generated_files/dotnet_version.xml','r') as file:
    content = file.read()

soup = BeautifulSoup(content, 'lxml-xml')

version_table=soup.select('#supported-versions-table .table tr')

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

def find_sdks(version):
    print(f"sdk version for {version}")
    url=f"https://dotnet.microsoft.com/en-us/download/dotnet/{version}"
    response=requests.get(url)
    html_content=response.text

    # Parse the HTML content with BeautifulSoup
    soup = BeautifulSoup(html_content, 'html.parser')

    if 'preview' in version:
        downloads_table=soup.select('.download-wrap .row .col-md-6 dl')
        print(downloads_table)

        for each_version_table in downloads_table:
            each_version=each_version_table.select()
            for element in each_version:
                if element.text == "Full version":
                    sdk_version=element.next_sibling().text
                    break
            
            check_sum=scrape_CheckSum(sdk_version,"","sdk")
            with open('generated_files/dotnet_sdk_latest_versions.txt', 'a') as version_file:
                version_file.write(f"{sdk_version},{check_sum}")

    else:
        downloads_table=soup.select('.download-wrap .row .col-md-6 h3')
        print(downloads_table)

        for element in downloads_table:
            if "SDK" in element.id:
                sdk_version=(element.text).replace("SDK ","").strip()
                check_sum=scrape_CheckSum(sdk_version,"","sdk")
                with open('generated_files/dotnet_sdk_latest_versions.txt', 'a') as version_file:
                    version_file.write(f"{sdk_version},{check_sum}")        

    return

def scrap_sdk_versions(HTML_CONTENT):
    all_version_tags=HTML_CONTENT.select('h3')
    all_description_tags=HTML_CONTENT.select('dl')
    print(all_version_tags)
    for index,version_tag in enumerate(all_version_tags):
        print(version_tag)
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
                version_file.write(f"{sdk_version}, {check_sum},\n")      
    return

def scrap_runtime_versions(HTML_CONTENT,major_version):
    all_version_tags=HTML_CONTENT.select('h3')
    all_description_tags=HTML_CONTENT.select('dl')
    print(f"all_description_tags length is {len(all_description_tags)}")
    x=major_version.replace('.','')
    for index,version_tag in enumerate(all_version_tags):
        if ("runtime" in version_tag.get('id')) and not ("desktop" in version_tag.get('id')):
            if ("ASP.NET Core Runtime" in version_tag.text):
                runtime_version=(version_tag.text).replace("ASP.NET Core Runtime ","").strip()
                check_sum=scrape_CheckSum(runtime_version,"aspnetcore-","runtime")
                print(f"{index} in asp")

                if(len(all_description_tags)>index):
                    print(f"{all_description_tags[index]}")
                    description_title_tags=all_description_tags[index].select('dt')

                    for title_tag in description_title_tags:
                        if "Full version" in title_tag.text:
                            full_version=title_tag.find_next_sibling().text
                            runtime_version=full_version
                
                print(f"ASP_runtime_version is {runtime_version}, SHA is {check_sum}")
                with open('generated_files/dotnet_latest_versions.txt', 'a') as version_file:
                    version_file.write(f"ASPNET_CORE_APP_{x}={runtime_version},")
                    version_file.write(f"ASPNET_CORE_APP_{x}_SHA={check_sum}\n")

            if (".NET Runtime" in version_tag.text):
                runtime_version=(version_tag.text).replace(".NET Runtime ","").strip()
                check_sum=scrape_CheckSum(runtime_version,"","runtime")
                print(f"{index} in net")

                if(len(all_description_tags)>index):
                    print(f"{all_description_tags[index]}")
                    description_title_tags=all_description_tags[index].select('dt')

                    for title_tag in description_title_tags:
                        if "Full version" in title_tag.text:
                            full_version=title_tag.find_next_sibling().text
                            runtime_version=full_version

                print(f"NET_runtime_version is {runtime_version}, SHA is {check_sum}")
                with open('generated_files/dotnet_latest_versions.txt', 'a') as version_file:
                    version_file.write(f"NET_CORE_APP_{x}={runtime_version},")
                    version_file.write(f"NET_CORE_APP_{x}_SHA={check_sum}\n")    
        
    return

def scrap_particular_version(major_version):
    url=f"https://dotnet.microsoft.com/en-us/download/dotnet/{major_version}"
    response=requests.get(url)
    html_content=response.text

    # Parse the HTML content with BeautifulSoup
    soup = BeautifulSoup(html_content, 'html.parser')

    version_details=soup.select('.download-wrap .row .col-md-6')

    scrap_sdk_versions(version_details[0])
    scrap_runtime_versions(version_details[1],major_version)

    return

for index,each_version in enumerate(version_table):
    if index !=0:
        version=each_version.find_all('td')[3].text
        split_version=version.split('.')

        major_version='.'.join(split_version[:2])
        
        print(f"version is {version}")
        print(f"major version is {major_version}")

        scrap_particular_version(major_version)
        # print(version)
        # find_sdks(version)
        # check_sum_aspnetcore=scrape_CheckSum(version,"aspnetcore-","runtime")
        # check_sum_netcore=scrape_CheckSum(version,"","runtime")

        # index_of_colon1=version.find('.')
        # first_part=version[0:index_of_colon1]
        # index_of_colon2=version.find('.',index_of_colon1+1)
        # second_part=version[index_of_colon1+1:index_of_colon2]

        # x="".join([first_part,second_part])


        # with open('generated_files/dotnet_latest_versions.txt', 'a') as version_file:
        #     version_file.write(f"NET_CORE_APP_{x}={version},")
        #     version_file.write(f"NET_CORE_APP_{x}_SHA={check_sum_netcore}\n")
        #     version_file.write(f"ASPNET_CORE_APP_{x}={version},")
        #     version_file.write(f"ASPNET_CORE_APP_{x}_SHA={check_sum_aspnetcore}\n")




        
    
