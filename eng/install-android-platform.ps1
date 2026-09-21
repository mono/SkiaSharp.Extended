param(
    [Parameter(Mandatory)]
    [string[]] $API
)

$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'install-android-sdk.ps1')

$sdk = $env:ANDROID_SDK_ROOT
$sdkManager = $env:ANDROID_SDK_MANAGER_PATH
$apis = $API -split ','
Write-Host "Installing $($apis.Count) Android API levels $API..."

foreach ($apiLevel in $apis) {
    Write-Host "Installing Android API level $apiLevel..."

    $apiPath = Join-Path $sdk 'platforms' "android-$apiLevel" 'android.jar'
    if (Test-Path $apiPath) {
        Write-Host "Android API level $apiLevel was already installed."
        continue
    }

    Set-Content -Value 'y' -Path yes.txt
    try {
        if ($IsMacOS -or $IsLinux) {
            sh -c "'$sdkManager' 'platforms;android-$apiLevel' < yes.txt"
        } else {
            cmd /c "`"$sdkManager`" `"platforms;android-$apiLevel`" < yes.txt"
        }
        if (-not $?) {
            Write-Host "Failed to install Android API level $apiLevel."
            exit 1
        }
        Write-Host "Installation of Android API level $apiLevel complete."
    } finally {
        Remove-Item yes.txt
    }
}

exit $LASTEXITCODE
