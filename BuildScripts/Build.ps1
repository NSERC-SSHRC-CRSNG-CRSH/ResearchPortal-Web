Param(
  [string]$command,
  [string]$project,
  [string]$profile,
  [switch]$help
)

# Get the currend directory of the script path
function Get-ScriptDirectory
{
	if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd }
}
$scriptPath = Get-ScriptDirectory
#Importing our lib of functions
Import-Module –Name "$scriptPath/build.psm1" -Force 

if ($help -or !($command)){
    PrintHelp
    exit 0
}

# get the build settings
$buildSettings = GetBuildSettings

# get the project from the settings
$proj =  $buildSettings.Projects."$project"

# run the command with the arguements
&$command -project $proj -profileName $profile