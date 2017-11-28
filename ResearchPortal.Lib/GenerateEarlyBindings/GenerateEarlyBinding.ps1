#
# GenerateEarlyBinding.ps1
#


# The path where the coretools are installed via NuGet
$path = "$PSScriptRoot\..\..\xRM-Portals-Community-Edition\packages\Microsoft.CrmSdk.CoreTools.9.0.0.7\content\bin\coretools"

# check that they are installed
if (! (Test-Path -Path $path)){
    Write-Host "Please restore the ResearchPortal.Web NuGet Packages before you run the Generate Early Bindings."
    exit
}

if (! (Test-Path -Path "$PSScriptRoot\CrmSvcUtil.exe.config")){
    copy "$PSScriptRoot\CrmSvcUtil.exe.config.sample" "$PSScriptRoot\CrmSvcUtil.exe.config"
}

# copy dependent files over to destination where it will be executed
copy "$PSScriptRoot\CrmSvcUtil.exe.config" $path
copy "$PSScriptRoot\*.dll" $path
copy "$PSScriptRoot\OrganizationService.EntityFilter.xml" $path


$entityOutputFile = "$PSScriptRoot\..\ResearchPortal.Entities.cs"
$optionSetEnumsOutputFile = "$PSScriptRoot\..\ResearchPortal.OptionSet.Enums.cs"

#/codewriterfilter:
# Run CrmSvcUtil to generate the code.  Using Interactive login.
#. "$path\crmsvcutil.exe" /l:CS /n:"$namespace" /nologo /out:"$outputFile"  /codewriterfilter:"CRMSvcUtilEntityFilter.CodeWriterFilter,CRMSvcUtilEntityFilter;crm.crmsvcutilextension.CRMiFilteringService,CRM_crmsvcutil_extension" /url:"$baseUrl/XRMServices/2011/Organization.svc" /interactivelogin
. "$path\crmsvcutil.exe" /a /nologo /out:"$entityOutputFile" /codewriterfilter:"CRMSvcUtilEntityFilter.CodeWriterFilter,CRMSvcUtilEntityFilter" /interactivelogin

