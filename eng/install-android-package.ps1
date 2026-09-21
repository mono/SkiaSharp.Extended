param(
    [Parameter(Mandatory)]
    [string] $Package
)

$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'install-android-sdk.ps1')

$sdkManager = $env:ANDROID_SDK_MANAGER_PATH

Set-Content -Value 'y' -Path yes.txt
try {
    if ($IsMacOS -or $IsLinux) {
        sh -c "'$sdkManager' '$Package' < yes.txt"
    } else {
        cmd /c "`"$sdkManager`" `"$Package`" < yes.txt"
    }
} finally {
    Remove-Item yes.txt
}

exit $LASTEXITCODE
