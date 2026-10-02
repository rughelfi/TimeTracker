<#
.SYNOPSIS
    Convenience wrapper to build the installer from the root directory.
#>
[CmdletBinding()]
param(
    [string]$Version = "",
    [string]$Configuration = "Release",
    [bool]$SelfContained = $true,
    [bool]$ReadyToRun = $true
)

& "$PSScriptRoot\Installer\build-installer.ps1" -Version $Version -Configuration $Configuration -SelfContained $SelfContained -ReadyToRun $ReadyToRun
