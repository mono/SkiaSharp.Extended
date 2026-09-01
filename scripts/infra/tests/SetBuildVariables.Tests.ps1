$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../set-build-variables.ps1'))
$pwsh = (Get-Command pwsh).Source
$identityVariables = @(
    'ARCADE_OFFICIAL_BUILD_ID'
    'BUILD_NUMBER'
    'BUILD_REASON'
    'BUILD_REPOSITORY_PROVIDER'
    'BUILD_REPOSITORY_URI'
    'BUILD_SOURCEBRANCH'
    'BUILD_SOURCEBRANCHNAME'
    'BUILD_SOURCEVERSION'
    'BUILD_SOURCEVERSIONMESSAGE'
    'DOTNET_FINAL_VERSION_KIND'
    'EXTENDED_VERSION'
    'GIT_BRANCH_NAME'
    'GIT_SHA'
    'GIT_URL'
    'PREVIEW_LABEL'
    'PR_NUMBER'
    'SYSTEM_TEAMPROJECT'
    'SYSTEM_PULLREQUEST_PULLREQUESTID'
    'SYSTEM_PULLREQUEST_PULLREQUESTNUMBER'
    'SYSTEM_PULLREQUEST_SOURCEBRANCH'
    'SYSTEM_PULLREQUEST_SOURCECOMMITID'
    'SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI'
)

function Invoke-BuildIdentityCase {
    param(
        [Parameter(Mandatory)][string] $Name,
        [Parameter(Mandatory)][hashtable] $Environment,
        [switch] $ExpectFailure
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $pwsh
    foreach ($argument in @('-NoLogo', '-NoProfile', '-File', $scriptPath, '-UpdateBuildNumber')) {
        $startInfo.ArgumentList.Add($argument)
    }
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    foreach ($variable in $identityVariables) {
        $startInfo.Environment.Remove($variable) | Out-Null
    }

    $defaults = @{
        ARCADE_OFFICIAL_BUILD_ID = '20260818.3'
        BUILD_REASON = 'IndividualCI'
        BUILD_REPOSITORY_PROVIDER = 'GitHub'
        BUILD_REPOSITORY_URI = 'https://github.com/mono/SkiaSharp.Extended.git'
        BUILD_SOURCEBRANCH = 'refs/heads/main'
        BUILD_SOURCEBRANCHNAME = 'main'
        BUILD_SOURCEVERSION = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
        BUILD_SOURCEVERSIONMESSAGE = ''
        EXTENDED_VERSION = '4.0.0'
        PREVIEW_LABEL = 'preview'
        SYSTEM_TEAMPROJECT = 'internal'
    }
    foreach ($pair in $defaults.GetEnumerator()) {
        $startInfo.Environment[$pair.Key] = $pair.Value
    }
    foreach ($pair in $Environment.GetEnumerator()) {
        $startInfo.Environment[$pair.Key] = [string]$pair.Value
    }

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $process.Start() | Out-Null
    $output = $process.StandardOutput.ReadToEnd()
    $errorOutput = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($ExpectFailure) {
        if ($process.ExitCode -eq 0) {
            throw "Case '$Name' unexpectedly succeeded.`n$output"
        }
    } elseif ($process.ExitCode -ne 0) {
        throw "Case '$Name' failed with exit code $($process.ExitCode).`n$output`n$errorOutput"
    }

    return "$output`n$errorOutput"
}

function Get-VariableValue {
    param([Parameter(Mandatory)][string] $Output, [Parameter(Mandatory)][string] $Name)

    $matches = [regex]::Matches($Output, "##vso\[task\.setvariable variable=$([regex]::Escape($Name))\]([^\r\n]*)")
    if ($matches.Count -eq 0) {
        throw "Output did not set variable '$Name'.`n$Output"
    }
    return $matches[$matches.Count - 1].Groups[1].Value
}

function Assert-Equal {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string] $Actual,
        [Parameter(Mandatory)][AllowEmptyString()][string] $Expected,
        [Parameter(Mandatory)][string] $Description
    )

    if ($Actual -cne $Expected) {
        throw "$Description expected '$Expected' but got '$Actual'."
    }
}

function Assert-BuildLabel {
    param([Parameter(Mandatory)][string] $Output, [Parameter(Mandatory)][string] $Expected)

    $match = [regex]::Match($Output, '(?m)^Build label: (.+)$')
    if (-not $match.Success) {
        throw "Output did not contain a build label.`n$Output"
    }
    Assert-Equal $match.Groups[1].Value.Trim() $Expected 'Build label'
}

$githubPr = Invoke-BuildIdentityCase 'GitHub PR' @{
    BUILD_REASON = 'PullRequest'
    BUILD_SOURCEBRANCH = 'refs/pull/42/merge'
    BUILD_SOURCEBRANCHNAME = 'merge'
    SYSTEM_PULLREQUEST_PULLREQUESTNUMBER = '42'
    SYSTEM_PULLREQUEST_SOURCEBRANCH = 'refs/heads/feature/foo'
    SYSTEM_PULLREQUEST_SOURCECOMMITID = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
    SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI = 'https://github.com/mono/SkiaSharp.Extended.git'
}
Assert-Equal (Get-VariableValue $githubPr 'PREVIEW_LABEL') 'pr.42' 'GitHub PR label'
Assert-Equal (Get-VariableValue $githubPr 'GIT_SHA') 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb' 'GitHub PR commit'
Assert-BuildLabel $githubPr '4.0.0-pr.42.26418.3'

$azurePr = Invoke-BuildIdentityCase 'Azure Repos PR' @{
    BUILD_REASON = 'PullRequest'
    BUILD_REPOSITORY_PROVIDER = 'TfsGit'
    BUILD_REPOSITORY_URI = 'https://dev.azure.com/dnceng/internal/_git/dotnet-SkiaSharp.Extended'
    BUILD_SOURCEBRANCH = 'refs/pull/123/merge'
    BUILD_SOURCEBRANCHNAME = 'merge'
    SYSTEM_PULLREQUEST_PULLREQUESTID = '123'
    SYSTEM_PULLREQUEST_SOURCEBRANCH = 'refs/heads/feature/bar'
    SYSTEM_PULLREQUEST_SOURCECOMMITID = 'cccccccccccccccccccccccccccccccccccccccc'
}
Assert-Equal (Get-VariableValue $azurePr 'PREVIEW_LABEL') 'pr.123' 'Azure Repos PR label'
Assert-BuildLabel $azurePr '4.0.0-pr.123.26418.3'

$main = Invoke-BuildIdentityCase 'Main CI' @{}
Assert-Equal (Get-VariableValue $main 'PREVIEW_LABEL') 'preview' 'Main preview label'
Assert-BuildLabel $main '4.0.0-preview.26418.3+main'

$nightly = Invoke-BuildIdentityCase 'Scheduled build' @{ BUILD_REASON = 'Schedule' }
Assert-Equal (Get-VariableValue $nightly 'PREVIEW_LABEL') 'nightly' 'Nightly label'

$release = Invoke-BuildIdentityCase 'Exact release' @{
    BUILD_SOURCEBRANCH = 'refs/heads/release/4.0.0'
    BUILD_SOURCEBRANCHNAME = '4.0.0'
    PREVIEW_LABEL = 'Stable'
}
Assert-Equal (Get-VariableValue $release 'DOTNET_FINAL_VERSION_KIND') 'release' 'Release final version kind'
Assert-BuildLabel $release '4.0.0+20260818.3'

$invalidRelease = Invoke-BuildIdentityCase 'Untrusted exact release' @{
    PREVIEW_LABEL = 'stable'
} -ExpectFailure
if ($invalidRelease -notmatch 'Exact release packages require') {
    throw "Untrusted release failed for the wrong reason.`n$invalidRelease"
}

$malformed = Invoke-BuildIdentityCase 'Malformed official build ID' @{
    ARCADE_OFFICIAL_BUILD_ID = '2026.1'
} -ExpectFailure
if ($malformed -notmatch 'must use yyyyMMdd.revision') {
    throw "Malformed build ID failed for the wrong reason.`n$malformed"
}

Write-Host 'Build identity tests passed.'
