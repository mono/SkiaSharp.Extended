[CmdletBinding()]
param(
    [string] $XcodeVersion = '26.3',
    [string] $IosSimulatorRuntime = 'iOS 26.2'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Set-CIEnvironmentVariable {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [Parameter(Mandatory)]
        [string] $Value
    )

    Set-Item -Path "Env:$Name" -Value $Value
    if ($env:TF_BUILD -eq 'True') {
        Write-Host "##vso[task.setvariable variable=$Name]$Value"
    }
    if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_ENV)) {
        Add-Content -Path $env:GITHUB_ENV -Value "$Name=$Value"
    }
}

$dotnet = if ($IsWindows) { './eng/common/dotnet.ps1' } else { './eng/common/dotnet.sh' }

$javaHome = (& $dotnet android jdk find --version '[17.0,18.0)').Trim()
if ([string]::IsNullOrWhiteSpace($javaHome) -or -not (Test-Path $javaHome)) {
    throw 'AndroidSdk.Tool could not locate JDK 17.'
}
& $dotnet android jdk dotnet-prefer --home $javaHome
if ($LASTEXITCODE) {
    throw "Setting the preferred JDK failed with exit code $LASTEXITCODE."
}

$androidSdkRoot = (& $dotnet android sdk find).Trim()
if ([string]::IsNullOrWhiteSpace($androidSdkRoot) -or -not (Test-Path $androidSdkRoot)) {
    throw 'AndroidSdk.Tool could not locate the Android SDK.'
}
& $dotnet android sdk accept-licenses --home $androidSdkRoot --force
if ($LASTEXITCODE) {
    throw "Android SDK license acceptance failed with exit code $LASTEXITCODE."
}

$requiredPackages = @(
    'platform-tools'
    'platforms;android-36'
    'build-tools;36.0.0'
    'cmdline-tools;19.0'
)
$arguments = @('android', 'sdk', 'install', '--home', $androidSdkRoot)
foreach ($package in $requiredPackages) {
    $arguments += @('--package', $package)
}
& $dotnet @arguments
if ($LASTEXITCODE) {
    throw "Android SDK provisioning failed with exit code $LASTEXITCODE."
}
& $dotnet android sdk dotnet-prefer --home $androidSdkRoot
if ($LASTEXITCODE) {
    throw "Setting the preferred Android SDK failed with exit code $LASTEXITCODE."
}

$sdkInfo = (& $dotnet android sdk info --home $androidSdkRoot --format json) | ConvertFrom-Json
$installedPackages = @($sdkInfo.InstalledComponents.Path)
$missingPackages = @($requiredPackages | Where-Object { $_ -notin $installedPackages })
if ($missingPackages.Count -ne 0) {
    throw "AndroidSdk.Tool did not install: $($missingPackages -join ', ')."
}

Write-Host "JDK: $javaHome"
Write-Host "Android SDK: $androidSdkRoot"
Set-CIEnvironmentVariable -Name JAVA_HOME -Value $javaHome
Set-CIEnvironmentVariable -Name JavaSdkDirectory -Value $javaHome
Set-CIEnvironmentVariable -Name ANDROID_SDK_ROOT -Value $androidSdkRoot
Set-CIEnvironmentVariable -Name AndroidSdkDirectory -Value $androidSdkRoot

if ($IsMacOS) {
    $xcodeInstallations = (& $dotnet apple xcode list --format json) | ConvertFrom-Json
    $xcode = @($xcodeInstallations | Where-Object { $_.Version -eq $XcodeVersion }) | Select-Object -First 1
    if ($null -eq $xcode -or -not (Test-Path $xcode.Path)) {
        $available = @($xcodeInstallations.Version) -join ', '
        throw "AppleDev.Tools could not locate Xcode $XcodeVersion. Available: $available."
    }

    $developerDirectory = Join-Path $xcode.Path 'Contents/Developer'
    Write-Host "Xcode: $($xcode.Version) at $($xcode.Path)"
    Set-CIEnvironmentVariable -Name DEVELOPER_DIR -Value $developerDirectory

    $simulators = @((& $dotnet apple simulator list --available --runtime $IosSimulatorRuntime --format json) | ConvertFrom-Json)
    if ($simulators.Count -eq 0) {
        throw "AppleDev.Tools found no available $IosSimulatorRuntime simulator."
    }
    Write-Host "Available $IosSimulatorRuntime simulators: $($simulators.Count)"
}
