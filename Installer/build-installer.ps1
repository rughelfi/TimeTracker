<#
.SYNOPSIS
    Builds the TimeTracker installer using dotnet publish and Inno Setup.
.PARAMETER Version
    The version string for the installer (e.g., 1.0.0). If omitted, read from TimeTracker.csproj.
.PARAMETER Configuration
    Build configuration (Release or Debug). Default is Release.
.PARAMETER SelfContained
    Whether to include the .NET runtime (recommended for end users). Default is $true.
.PARAMETER ReadyToRun
    Whether to compile with ReadyToRun for faster startup. Default is $true.
#>
[CmdletBinding()]
param(
    [string]$Version = "",
    [string]$Configuration = "Release",
    [bool]$SelfContained = $true,
    [bool]$ReadyToRun = $true,
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectDir = Split-Path -Parent $ScriptDir
$CsprojPath = Join-Path $ProjectDir "TimeTracker\TimeTracker.csproj"
$IssPath = Join-Path $ScriptDir "TimeTracker.iss"
$OutputDir = Join-Path $ScriptDir "Output"
$TempPublishDir = Join-Path $ScriptDir "temp_publish"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "   TimeTracker Installer Builder" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# 1. Determine Version
if ([string]::IsNullOrWhiteSpace($Version)) {
    if (Test-Path $CsprojPath) {
        $csprojContent = Get-Content $CsprojPath -Raw
        if ($csprojContent -match '<Version>([^<]+)</Version>') {
            $Version = $Matches[1].Trim()
        }
    }
}
if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = "1.0.0"
}
Write-Host "[1/4] Target Version: $Version ($Configuration, SelfContained: $SelfContained)" -ForegroundColor Green

# 2. Check for Inno Setup Compiler (ISCC.exe)
function Find-ISCC {
    $cmd = Get-Command "iscc.exe" -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $candidatePaths = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "$env:LOCALAPPDATA\Programs\Inno Setup 7\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 7\ISCC.exe"
    )

    foreach ($path in $candidatePaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    return $null
}

$isccPath = Find-ISCC

if (-not $isccPath) {
    Write-Host "[!] Inno Setup not found. Attempting automatic installation..." -ForegroundColor Yellow
    $innoUrl = "https://github.com/jrsoftware/issrc/releases/download/is-6_7_3/innosetup-6.7.3.exe"
    $installerTemp = Join-Path $env:TEMP "innosetup-installer.exe"
    Write-Host "    Downloading Inno Setup 6..." -ForegroundColor Gray
    Invoke-WebRequest -Uri $innoUrl -OutFile $installerTemp
    $installDir = "$env:LOCALAPPDATA\Programs\Inno Setup 6"
    Write-Host "    Installing to $installDir..." -ForegroundColor Gray
    Start-Process -FilePath $installerTemp -ArgumentList "/CURRENTUSER /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /DIR=`"$installDir`"" -Wait
    Remove-Item $installerTemp -Force -ErrorAction SilentlyContinue

    $isccPath = Find-ISCC
    if (-not $isccPath) {
        throw "Could not find or install Inno Setup (ISCC.exe). Please install Inno Setup manually from https://jrsoftware.org/isdl.php"
    }
}

Write-Host "[2/4] Found Inno Setup Compiler: $isccPath" -ForegroundColor Green

# 3. Publish the Application
Write-Host "[3/4] Publishing TimeTracker with .NET SDK..." -ForegroundColor Green
if (Test-Path $TempPublishDir) {
    Remove-Item -Recurse -Force $TempPublishDir
}

$publishArgs = @(
    "publish",
    "`"$CsprojPath`"",
    "-c", $Configuration,
    "-r", $Runtime,
    "-o", "`"$TempPublishDir`""
)

if ($SelfContained) {
    $publishArgs += "--self-contained", "true"
} else {
    $publishArgs += "--self-contained", "false"
}

if ($ReadyToRun -and $SelfContained) {
    $publishArgs += "-p:PublishReadyToRun=true"
}

Write-Host "    Executing: dotnet $($publishArgs -join ' ')" -ForegroundColor Gray
$publishProcess = Start-Process -FilePath "dotnet" -ArgumentList $publishArgs -NoNewWindow -Wait -PassThru
if ($publishProcess.ExitCode -ne 0) {
    throw "dotnet publish failed with exit code $($publishProcess.ExitCode)"
}

# 4. Compile Installer
Write-Host "[4/4] Compiling installer with Inno Setup..." -ForegroundColor Green
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

$isccArgs = @(
    "/Qp",
    "/DMyAppVersion=$Version",
    "/DSourceDir=$TempPublishDir",
    "/DOutputDir=$OutputDir",
    "`"$IssPath`""
)

$isccProcess = Start-Process -FilePath $isccPath -ArgumentList $isccArgs -NoNewWindow -Wait -PassThru
if ($isccProcess.ExitCode -ne 0) {
    throw "ISCC compilation failed with exit code $($isccProcess.ExitCode)"
}

# Cleanup temporary publish folder
if (Test-Path $TempPublishDir) {
    Remove-Item -Recurse -Force $TempPublishDir -ErrorAction SilentlyContinue
}

$expectedInstaller = Join-Path $OutputDir "TimeTracker-Setup-v$Version.exe"
if (Test-Path $expectedInstaller) {
    $fileInfo = Get-Item $expectedInstaller
    $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host " SUCCESS! Installer generated:" -ForegroundColor Green
    Write-Host " File: $($fileInfo.FullName)" -ForegroundColor Yellow
    Write-Host " Size: $sizeMB MB" -ForegroundColor Yellow
    Write-Host "=========================================" -ForegroundColor Green
} else {
    Write-Warning "Setup completed, check $OutputDir for generated files."
}
