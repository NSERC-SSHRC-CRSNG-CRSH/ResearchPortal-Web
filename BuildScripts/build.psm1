#
# Contains helpful functions for manipulating the file system.
#
# Get the currend directory of the script path
function Get-ScriptDirectory
{
	if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd }
}
$scriptPath = Get-ScriptDirectory

# Import the WebAdministration module to manage IIS
Import-Module "WebAdministration"


Add-Type -AssemblyName System.IO.Compression.FileSystem

$rootSourcePath = [System.IO.Path]::GetFullPath((Join-Path $scriptPath '..\'))

$buildSettings = @{
    "NuGetUrl"="https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    "NuGetFilePath"=Join-Path $rootSourcePath "nuget.exe"    
}

$buildSettings.Projects = (Get-Content (Join-Path $scriptPath "targets.json")) -join "`n" | ConvertFrom-Json


$buildSettings.Projects  | Get-Member -MemberType NoteProperty | ForEach-Object {
    $project = $_.Name
    $buildSettings.Projects."$project".BuildProfiles | Get-Member -MemberType NoteProperty | ForEach-Object {
        $profile = $_.Name
        if ($buildSettings.Projects."$project".BuildProfiles."$profile".PublishProfile.publishUrl -and
            !$buildSettings.Projects."$project".BuildProfiles."$profile".PublishProfile.publishUrl.Contains(":")){
            # specify the root path of the publishUrl
            $buildSettings.Projects."$project".BuildProfiles."$profile".PublishProfile.publishUrl = Join-Path $rootSourcePath $buildSettings.Projects."$project".BuildProfiles."$profile".PublishProfile.publishUrl
            Write-Debug "Updated publishUrl for $project, profile: $profile to $($buildSettings.Projects."$project".BuildProfiles."$profile".PublishProfile.publishUrl)"
        }
    }
}

$VSSetupExists = Get-Command Get-VSSetupInstance -ErrorAction SilentlyContinue
$VSSetupPath = "$([Environment]::GetFolderPath("MyDocuments"))\WindowsPowerShell\Modules\VSSetup"
function GetBuildSettings(){
    $vsSetupZipUrl = "https://github.com/Microsoft/vssetup.powershell/releases/download/2.0.1/VSSetup.zip"
    
    if(!(Test-Path $VSSetupPath)){
        Write-Host "VS Setup Module is not installed, installing it."
        $wc = New-Object System.Net.WebClient
        $outFile = Join-Path $env:TEMP "vssetup.zip"
        Write-Host "Downloading file: $vsSetupZipUrl to $outFile"
        $wc.DownloadFile($vsSetupZipUrl, $outFile)
        Write-Host "Extracting $outFile to $VSSetupPath"    
        [System.IO.Compression.ZipFile]::ExtractToDirectory($outFile, $VSSetupPath)
    }
    # check if it is not loaded
    $VSSetupExists = Get-Command Get-VSSetupInstance -ErrorAction SilentlyContinue
    if(-not $VSSetupExists){
        # load the module
        Import-Module $VSSetupPath
    }

    #if it does not still exist exit
    $VSSetupExists = Get-Command Get-VSSetupInstance -ErrorAction SilentlyContinue
    if(-not $VSSetupExists){
        Write-Host "Unable to load VSSetup Module" -ForegroundColor Red
        exit 0
    }
    $msbuildPath = ""
    if (Test-Path ".msbuild"){
        $msbuildPath = Get-Content ".msbuild"
    }
    if (!($msbuildPath) -or !(Test-Path $msbuildPath)){
        Write-Host "Looking for path to msbuild.exe"
        $vsPath = (Get-VSSetupInstance | Select-VSSetupInstance -Latest -Require Microsoft.Component.MSBuild).InstallationPath
        $msbuildPath = (Get-ChildItem $vsPath -Recurse -Filter "msbuild.exe" | where {$_.Directory.FullName -like "*64*"}).FullName
        Write-Host "Found path to msbuild.exe: $msbuildPath"
        $msbuildPath > ".msbuild"
    }
    $buildSettings.MSbuildExe=$msbuildPath

    Write-Output $buildSettings
}

$physicalPath = Join-Path $rootSourcePath $($publishProfile.PublishProfile.publishUrl)

function DownloadNuGet(){
    if (!(Test-Path -Path $buildSettings.NuGetFilePath)){
	    $wc = New-Object System.Net.WebClient
	    Write-Host "Url to download: $($buildSettings.NuGetUrl)"
	    Write-Host "Output Path: $($buildSettings.NuGetFilePath)"
	    $wc.DownloadFile($buildSettings.NuGetUrl, $buildSettings.NuGetFilePath)
	    Write-Host "$($buildSettings.NuGetFilePath) - Downloaded." -foreground green
    }
}


function GetDeploySettings(){
    Write-Output $deploySettings
}

# Given the web deploy configuration, extract the properties string to pass on to msbuild
function GetMSBuildWebDeployProperties($publishProfile){
    $profileKeys = $publishProfile  | Get-Member -MemberType NoteProperty 
    if ($profileKeys.Count -eq 0){
        return
    }

    Write-Output "/p:DeployOnBuild=true"        
    $profileKeys | ForEach-Object {
        $key = $_.Name
        Write-Output "/p:`"$key=$($publishProfile."$key")`""
    }
}


# Given the web deploy configuration, extract the properties string to pass on to msbuild
function GetMSBuildPropertyArgs($buildProfile){
    $buildProperties = $buildProfile.Properties
     $buildProperties  | Get-Member -MemberType NoteProperty | ForEach-Object {
        $key = $_.Name
        Write-Output "/p:`"$key=$($buildProperties."$key")`""
    }
}

# Given a solution path, restore the nuget Packages
function RestoreNuGetPackages($pathToSolution){
    # Download NuGet Executable if it does not exist
    DownloadNuGet
    Write-Host "Restoring NuGet packages for: " -foregroundcolor green -NoNewline
    Write-Host "$pathToSolution" -foregroundcolor Yellow
    & "$($buildSettings.NuGetFilePath)" restore "$pathToSolution"
}


function WriteObject ($object, $indent){
    WriteObjectRecurse -object $object -indent 0
    Write-Host "" #finish with new line
}
function WriteObjectRecurse ($object, $indent){
    if ($indent -gt 30){
        Write-Host "{...}" -NoNewline -ForegroundColor Yellow
        return
    }
    $members = ($object | Get-Member -MemberType NoteProperty )
    if ($members.Count -eq 0) {
        Write-Host "$object" -NoNewline -ForegroundColor Yellow
    }
    $members | ForEach-Object {
        $name = $_.Name
        Write-Host ""
        Write-Host ("  " * $indent) -NoNewline
        Write-Host "$name : " -NoNewline -ForegroundColor Green
        WriteObjectRecurse -object $object."$name" -indent ($indent + 1)
    }
}


function PrintHelp(){
    Write-Host ""
    Write-Host "Help"
    Write-Host "-command, speficy a command to run :" -ForegroundColor White
    Write-Host "-project, speficy one of the following solution projects :" -ForegroundColor White
    Write-Host "-profile, speficy a build profile for the specified project:" -ForegroundColor White
    Write-Host ""
    Write-Host "List of Projects and their Build Profiles:" -ForegroundColor White
    $buildSettings.Projects | Get-Member -MemberType NoteProperty | ForEach-Object {
        $key = $_.Name
        Write-Host "  $key - " -ForegroundColor Green -NoNewline
        Write-Host $($buildSettings.Projects."$key".PathToSolution) -ForegroundColor DarkGreen

        $buildSettings.Projects."$key".BuildProfiles| Get-Member -MemberType NoteProperty | ForEach-Object {
            $profileName = $_.Name
            Write-Host "    $profileName" -NoNewline -ForegroundColor DarkGreen
            Write-Host " - " -NoNewline -ForegroundColor White
            Write-Host "$($buildSettings.Projects."$key".BuildProfiles."$profileName".Description)"  -ForegroundColor Yellow
        }       
    }
    Write-Host ""
    Write-Host "List of Commands:" -ForegroundColor White

    Write-Host "  MSBuild - " -ForegroundColor Green -NoNewline
    Write-Host "Build the project using the specified profile" -ForegroundColor DarkGreen 
    Write-Host "  MSBuildPublish - " -ForegroundColor Green -NoNewline
    Write-Host "Build the project, and publish it using the profile" -ForegroundColor DarkGreen 

    Write-Host "  InitializeIIS - " -ForegroundColor Green -NoNewline
    Write-Host "Initialize the development envionment, configuring IIS and building and publish xRM Portals and customization to the IIS publish directory" -ForegroundColor DarkGreen 

    Write-Host "  InitializeRP2 - " -ForegroundColor Green -NoNewline
    Write-Host "Initialize the development envionment without configuring IIS.  It builds and publishes xRM Portals and cutomizations to the publish directory" -ForegroundColor DarkGreen 
}

# validates the profile for the project
# exits if no profile exists
# returns it if it does
function ValidateBuildProfileName($project, $profileName){
    if (!($project)){
        Write-Host "The project is not defined." -ForegroundColor Red
        Write-Host ""
        PrintHelp
        exit 1
    }
    if (!($project.BuildProfiles)){
        Write-Host "The project sepcified does not define any Build Profiles." -ForegroundColor Red
        WriteObject -object $project
        Write-Host ""
        PrintHelp
        exit 1
    }
    if (!($project.BuildProfiles."$profileName")){
        Write-Host "The project sepcified does not define the profile: '$profileName'." -ForegroundColor Red
        Write-Host ""
        PrintHelp
        exit 1
    }
    Write-Output $project.BuildProfiles."$profileName"
}

$MSDeployPath = "C:\Program Files\IIS\Microsoft Web Deploy V3"
if (!$env:MSDeployPath){
    $MSDeployPath=$env:MSDeployPath
}


function CreateMsDeployPackage($sourcePath, $destinationPackage){
    & $MSDeployPath -verb:sync -source:contentPath="$sourcePath" -dest:package="$destinationPackage"
}



###############################
#
#  Build Command Functions
#
###############################



function BuildPublish($project, $profileName){
   
    $buildProfile = ValidateBuildProfileName -project $project -profileName $profileName

    $PublishPropertiesBuildString=GetMSBuildWebDeployProperties -publishProfile $buildProfile.PublishProfile

    Write-Host "MSBuild Publish Arguements: $PublishPropertiesBuildString" -foregroundcolor green
    Build -project $project -profileName $profileName -additionalMSBuildArguements $PublishPropertiesBuildString
}

#######
#  Given a project object, p
#######
function Build($project, $profileName, $additionalMSBuildArguements){
    $buildProfile = ValidateBuildProfileName -project $project -profileName $profileName
    
    $pathToSolution = Join-Path $rootSourcePath  $project.PathToSolution

    RestoreNuGetPackages $pathToSolution

    if ($project.PublishProject){
        $pathToSolution = Join-Path $rootSourcePath  $project.PublishProject
    }

    $msBuildArgs = GetMSBuildPropertyArgs -buildProfile $buildProfile

    if ($project.TargetProject){
       $msBuildArgs =@("/T:$($project.TargetProject)") + $msBuildArgs
    }
    Write-Host "Building Solution: " -foregroundcolor green -NoNewline
    Write-Host "$pathToSolution" -foregroundcolor Yellow
    Write-Host "MSBuild Property Args: " -foregroundcolor green -NoNewline
    Write-Host ($msBuildArgs -join ' ') -foregroundcolor Yellow
    if ($additionalMSBuildArguements){
        Write-Host "MSBuild Additional Args: " -foregroundcolor green -NoNewline
        Write-Host ($additionalMSBuildArguements -join ' ') -foregroundcolor Yellow
    }
    
    $msBuildArgs =@($pathToSolution) + $msBuildArgs
    
    if ($additionalMSBuildArguements){
       $msBuildArgs = $msBuildArgs + $additionalMSBuildArguements
    }
    Write-Host "$($buildSettings.MSbuildExe) $($msBuildArgs -join ' ') " -foregroundcolor green
    & "$($buildSettings.MSbuildExe)" $msBuildArgs
    if ($LASTEXITCODE -gt 0){
        Write-Host "MSBuild exited with code $LASTEXITCODE "
        exit $LASTEXITCODE
    }
}

function xRMPortals(){
    Write-Host "Initializing xRMPortalsCommunityEdition"
    $proj =  $buildSettings.Projects.xRMPortalsCommunityEdition    
    Build -project $proj -profileName "Dev"
    
    $proj =  $buildSettings.Projects.ResearchPortal  
    $pathToSolution = Join-Path $rootSourcePath  $proj.PathToSolution

    RestoreNuGetPackages $pathToSolution
}


function xRMPortalsNuGetRestore(){
    Write-Host "Initializing xRMPortalsCommunityEdition"
    $proj =  $buildSettings.Projects.xRMPortalsCommunityEdition    
    $pathToSolution = Join-Path $rootSourcePath  $proj.PathToSolution

    RestoreNuGetPackages $pathToSolution
    
    $proj =  $buildSettings.Projects.ResearchPortal  
    $pathToSolution = Join-Path $rootSourcePath  $proj.PathToSolution

    RestoreNuGetPackages $pathToSolution
}