[CmdletBinding()]
param(
    [string] $XcodeVersion = '26.3'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $IsMacOS) {
    exit 0
}

$dotnet = './eng/common/dotnet.sh'
$xcodeInstallations = (& $dotnet apple xcode list --format json) | ConvertFrom-Json
$xcode = @($xcodeInstallations | Where-Object { $_.Version -eq $XcodeVersion }) | Select-Object -First 1
if ($null -eq $xcode -or -not (Test-Path $xcode.Path)) {
    $available = @($xcodeInstallations.Version) -join ', '
    throw "AppleDev.Tools could not locate Xcode $XcodeVersion. Available: $available."
}

$developerDirectory = Join-Path $xcode.Path 'Contents/Developer'
Write-Host "Xcode: $($xcode.Version) at $($xcode.Path)"
Write-Host "##vso[task.setvariable variable=DEVELOPER_DIR]$developerDirectory"
$env:DEVELOPER_DIR = $developerDirectory
