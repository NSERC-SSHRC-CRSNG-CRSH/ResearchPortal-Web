# Get the currend directory of the script path
function Get-ScriptDirectory
{
	if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd }
}
$scriptPath = Get-ScriptDirectory
& "$scriptPath\Build.ps1" -command xRMPortalsNuGetRestore
