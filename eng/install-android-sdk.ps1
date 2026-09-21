param(
    [string] $InstallDestination = $null
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$homeDirectory = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }

$manualLocation = Join-Path $homeDirectory 'android-sdk'
if ($InstallDestination) {
    $manualLocation = $InstallDestination
}

function Get-SdkManager {
    param(
        [string] $SdkPath,
        [string] $Indent = '    '
    )

    Write-Host "${Indent}Looking for the SDK Manager in $SdkPath..."

    if (-not $SdkPath -or -not (Test-Path $SdkPath)) {
        Write-Host "${Indent}No SDK Manager found."
        return ''
    }

    $commandLineTools = Join-Path $SdkPath 'cmdline-tools'
    if (Test-Path $commandLineTools) {
        Write-Host "${Indent}  Found command line tools:"
        $commandLineTools | Get-ChildItem | ForEach-Object {
            Write-Host "${Indent}    $_"
        }
    } else {
        Write-Host "${Indent}  No command line tools found."
        return ''
    }

    Write-Host "${Indent}  Selecting the latest command line tools..."

    $sdkExtension = if (-not $IsMacOS -and -not $IsLinux) { '.bat' } else { '' }
    $versions = Get-ChildItem (Join-Path $SdkPath 'cmdline-tools')
    $latest = ($versions | Select-Object -Last 1)[0]
    Write-Host "${Indent}  Latest command line tools found at $latest."
    $sdkManager = Join-Path $latest 'bin' "sdkmanager$sdkExtension"

    if (-not (Test-Path $sdkManager)) {
        Write-Host "${Indent}No SDK Manager found."
        return ''
    }

    Write-Host "${Indent}Found the SDK Manager at $sdkManager."

    return $sdkManager
}

function Get-Adb {
    param(
        [string] $SdkPath,
        [string] $Indent = '    '
    )

    Write-Host "${Indent}Looking for ADB in $SdkPath..."

    $platformTools = Join-Path $SdkPath 'platform-tools'
    if (Test-Path $platformTools) {
        Write-Host "${Indent}  Found platform tools: $platformTools"
    } else {
        Write-Host "${Indent}  No platform tools found."
        return ''
    }

    $adbExtension = if (-not $IsMacOS -and -not $IsLinux) { '.exe' } else { '' }
    $adb = Get-ChildItem $platformTools | Where-Object { $_.Name -eq "adb$adbExtension" }

    if (-not $adb) {
        Write-Host "${Indent}No ADB found."
        return ''
    }

    Write-Host "${Indent}Found ADB at $adb."

    return $adb
}

function Get-AndroidSdk {
    param(
        [string] $SdkPath,
        [string] $PathType,
        [string] $Indent = '  '
    )

    Write-Host "${Indent}Looking for the Android SDK in $SdkPath ($PathType)..."

    if (-not $SdkPath -or -not (Test-Path $SdkPath)) {
        Write-Host "${Indent}No Android SDK found."
        return ''
    }

    $sdkManager = Get-SdkManager -SdkPath $SdkPath
    if (-not (Test-Path $sdkManager)) {
        Write-Host "${Indent}No SDK Manager found, not going to use this one."
        return ''
    }

    $adb = Get-Adb -SdkPath $SdkPath
    if (-not (Test-Path $adb)) {
        Write-Host "${Indent}No ADB found, not going to use this one."
        return ''
    }

    Write-Host "${Indent}Using the Android SDK at $SdkPath."

    return $SdkPath
}

Write-Host 'Looking for an existing Android SDK...'

$sdk = Get-AndroidSdk -SdkPath $env:ANDROID_SDK_ROOT -PathType 'ANDROID_SDK_ROOT'
if (-not $sdk -and $env:ANDROID_HOME) {
    $sdk = Get-AndroidSdk -SdkPath $env:ANDROID_HOME -PathType 'ANDROID_HOME'
}
if (-not $sdk -and -not $IsMacOS -and -not $IsLinux) {
    $programFilesSdk = Join-Path ${env:ProgramFiles(x86)} 'Android' 'android-sdk'
    $sdk = Get-AndroidSdk -SdkPath $programFilesSdk -PathType 'Program Files'
}
if (-not $sdk) {
    $sdk = Get-AndroidSdk -SdkPath $manualLocation -PathType 'Previous install location'
}

if (-not $sdk) {
    Write-Host 'No Android SDK found, will download and install the latest...'

    $sdk = $manualLocation
    Write-Host "  Install destination is $sdk."

    Write-Host '  Getting the latest command line tool info...'
    [xml] $repository = Invoke-WebRequest -Uri 'https://dl.google.com/android/repository/repository2-1.xml'
    $stable = $repository.DocumentElement.remotePackage |
        Where-Object { $_.path -eq 'cmdline-tools;latest' -and $_.channelRef.ref -eq 'channel-0' }
    $platform = if ($IsMacOS) { 'macosx' } elseif ($IsLinux) { 'linux' } else { 'windows' }
    $archive = $stable.archives.archive | Where-Object { $_.'host-os' -eq $platform }
    $filename = $archive.complete.url
    Write-Host "  Latest command line tools are $filename..."
    $url = "https://dl.google.com/android/repository/$filename"

    $sdkTemp = Join-Path $homeDirectory 'android-sdk-temp'
    $zip = Join-Path $sdkTemp $filename

    if (-not (Test-Path $zip)) {
        Write-Host "  Downloading SDK ($url) to '$zip'..."
        New-Item -ItemType Directory -Force -Path $sdkTemp | Out-Null
        (New-Object System.Net.WebClient).DownloadFile($url, $zip)
    }

    if (Test-Path (Join-Path $sdkTemp 'extracted')) {
        Write-Host "  Removing old extracted SDK from $sdkTemp/extracted..."
        Remove-Item -Recurse -Force (Join-Path $sdkTemp 'extracted')
    }
    if (Test-Path $sdk) {
        Write-Host "  Removing old SDK from $sdk..."
        Remove-Item -Recurse -Force $sdk
    }
    Write-Host "  Extracting SDK to $sdkTemp/extracted..."
    try {
        if ($IsLinux -or $IsMacOS) {
            unzip -q $zip -d (Join-Path $sdkTemp 'extracted')
            if (-not $?) {
                throw '  Failed to extract the SDK.'
            }
        } else {
            [System.IO.Compression.ZipFile]::ExtractToDirectory($zip, (Join-Path $sdkTemp 'extracted'))
        }
    } catch {
        Write-Host '  Failed to extract the SDK, deleting the file as it may be corrupt...'
        Remove-Item -Force $zip
        exit 1
    }
    Write-Host "  Moving command line tools to $sdk/cmdline-tools/latest..."
    New-Item -ItemType Directory -Force -Path (Join-Path $sdk 'cmdline-tools' 'latest') | Out-Null
    Move-Item (Join-Path $sdkTemp 'extracted' 'cmdline-tools' '*') (Join-Path $sdk 'cmdline-tools' 'latest')

    Write-Host 'Installation complete.'
}

$sdkManager = Get-SdkManager -SdkPath $sdk -Indent ''
$adb = Get-Adb -SdkPath $sdk -Indent ''

Write-Host "Using Android SDK at $sdk."
Write-Host "##vso[task.setvariable variable=ANDROID_SDK_ROOT;]$sdk"
Write-Host "##vso[task.setvariable variable=ANDROID_HOME;]$sdk"
Write-Host "##vso[task.setvariable variable=AndroidSdkDirectory;]$sdk"
Write-Host "##vso[task.setvariable variable=ANDROID_SDK_MANAGER_PATH;]$sdkManager"
Write-Host "##vso[task.setvariable variable=ANDROID_ADB_PATH;]$adb"
$env:ANDROID_SDK_ROOT = $sdk
$env:ANDROID_HOME = $sdk
$env:AndroidSdkDirectory = $sdk
$env:ANDROID_SDK_MANAGER_PATH = $sdkManager
$env:ANDROID_ADB_PATH = $adb

exit $LASTEXITCODE
