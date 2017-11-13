

#$sdkpath= Get-Item -Path "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0" -name "FrameworkSDKRoot"
#$sdkpath=$sdkpath.FrameworkSDKRoot


$pathToXsd = "c:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\xsd.exe";


$listOfFiles = " /classes /language:CS /namespace:Orcid.Models .\activities-2.0.xsd .\address-2.0.xsd .\bulk-2.0.xsd .\client-2.0.xsd .\common-2.0.xsd .\education-2.0.xsd .\email-2.0.xsd .\employment-2.0.xsd .\error-2.0.xsd .\funding-2.0.xsd .\group-id-2.0.xsd .\history-2.0.xsd .\keyword-2.0.xsd .\notification-custom-2.0.xsd .\notification-permission-2.0.xsd .\orcid-error-1.0.xsd .\other-name-2.0.xsd .\peer-review-2.0.xsd .\person-2.0.xsd .\person-external-identifier-2.0.xsd .\personal-details-2.0.xsd .\preferences-2.0.xsd  .\researcher-url-2.0.xsd .\search-2.0.xsd .\work-2.0.xsd .\record-2.0.xsd" 

#Write-Host "Generating file for $listOfFiles"
Write-Host "$pathToXsd"  /classes /language:CS /namespace:Orcid.Models $listOfFiles  /out:$PSScriptRoot
. "$pathToXsd"  /classes /language:CS /namespace:Orcid.Models $listOfFiles.Split(" ") /out:$PSScriptRoot

