param(
    [string] $Version = '17.0.8.1',
    [string] $FolderVersion = '17.0.8.1+1',
    [string] $InstallDestination
)

$ErrorActionPreference = 'Stop'

if ("$env:JAVA_HOME_17_X64" -and (Test-Path (Join-Path $env:JAVA_HOME_17_X64 'bin'))) {
    Write-Host "Java is already installed to '$env:JAVA_HOME_17_X64'..."
    $javaHome = $env:JAVA_HOME_17_X64
} else {
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $homeDirectory = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }
    if ($IsMacOS) {
        $extension = 'tar.gz'
        $url = "https://aka.ms/download-jdk/microsoft-jdk-$Version-macOS-x64.tar.gz"
    } elseif ($IsLinux) {
        $extension = 'tar.gz'
        $url = "https://aka.ms/download-jdk/microsoft-jdk-$Version-linux-x64.tar.gz"
    } else {
        $extension = 'zip'
        $url = "https://aka.ms/download-jdk/microsoft-jdk-$Version-windows-x64.zip"
    }

    $jdkDirectory = if ($InstallDestination) {
        $InstallDestination
    } else {
        Join-Path $homeDirectory 'openjdk'
    }
    $tempDirectory = Join-Path $homeDirectory 'openjdk-temp'
    $archive = Join-Path $tempDirectory "openjdk.$extension"

    Write-Host "Downloading OpenJDK to '$archive'..."
    New-Item -ItemType Directory -Force -Path $tempDirectory | Out-Null
    (New-Object System.Net.WebClient).DownloadFile($url, $archive)

    Write-Host "Extracting OpenJDK to '$jdkDirectory'..."
    New-Item -ItemType Directory -Force -Path $jdkDirectory | Out-Null
    if ($IsMacOS -or $IsLinux) {
        tar -vxzf $archive -C $jdkDirectory
    } else {
        [System.IO.Compression.ZipFile]::ExtractToDirectory($archive, $jdkDirectory)
    }

    $javaHome = if ($IsMacOS) {
        Join-Path $jdkDirectory "jdk-$FolderVersion/Contents/Home"
    } else {
        Join-Path $jdkDirectory "jdk-$FolderVersion"
    }
}

Write-Host "##vso[task.setvariable variable=JAVA_HOME;]$javaHome"
$env:JAVA_HOME = $javaHome

$javaBin = Join-Path $javaHome 'bin'
if (-not $env:PATH.Contains($javaBin)) {
    $env:PATH = "$javaBin$([IO.Path]::PathSeparator)$env:PATH"
    Write-Host "##vso[task.setvariable variable=PATH;]$env:PATH"
}

java -version
exit $LASTEXITCODE
