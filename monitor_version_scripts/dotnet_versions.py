from bs4 import BeautifulSoup
import requests

with open('dotnet_version.xml','r') as file:
    content = file.read()

soup = BeautifulSoup(content, 'lxml-xml')

version_table=soup.select('#supported-versions-table .table tr')

def scrape_CheckSum(version,runtime):
    runtime_version="".join([runtime,version])
    url=f"https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-{runtime_version}-linux-x64-binaries"
    response=requests.get(url)
    html_content=response.text

    # Parse the HTML content with BeautifulSoup
    soup = BeautifulSoup(html_content, 'html.parser')
    
    check_sum=soup.select('#checksum')[0]['value']
    return check_sum

for index,each_version in enumerate(version_table):
    if index !=0:
        version=each_version.find_all('td')[3].text
        print(version)
        check_sum_aspnetcore=scrape_CheckSum(version,"aspnetcore-")
        check_sum_netcore=scrape_CheckSum(version,"")

        index_of_colon1=version.find('.')
        first_part=version[0:index_of_colon1]
        index_of_colon2=version.find('.',index_of_colon1+1)
        second_part=version[index_of_colon1+1:index_of_colon2]

        x="".join([first_part,second_part])


        with open('dotnet_latest_versions.txt', 'a') as version_file:
            version_file.write(f"NET_CORE_APP_{x}={version},")
            version_file.write(f"NET_CORE_APP_{x}_SHA={check_sum_netcore}\n")
            version_file.write(f"ASPNET_CORE_APP_{x}={version},")
            version_file.write(f"ASPNET_CORE_APP_{x}_SHA={check_sum_aspnetcore}\n")




        
    
