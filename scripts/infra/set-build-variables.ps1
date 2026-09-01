Param(
    [switch] $UpdateBuildNumber
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-FirstNonEmpty {
    param([string[]] $Values)

    foreach ($value in $Values) {
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }
    }

    return ''
}

function Set-BuildVariable {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [AllowEmptyString()]
        [string] $Value
    )

    Set-Item -Path "Env:$Name" -Value $Value
    Write-Host "##vso[task.setvariable variable=$Name]$Value"
}

function ConvertTo-ProductBuildNumber {
    param([Parameter(Mandatory)][string] $OfficialBuildId)

    $match = [regex]::Match($OfficialBuildId, '^(?<date>\d{8})\.(?<revision>\d+)$')
    if (-not $match.Success) {
        throw "ARCADE_OFFICIAL_BUILD_ID '$OfficialBuildId' must use yyyyMMdd.revision."
    }

    $date = [DateTime]::ParseExact(
        $match.Groups['date'].Value,
        'yyyyMMdd',
        [Globalization.CultureInfo]::InvariantCulture)
    $shortDate = (($date.Year % 100) * 1000) + (50 * $date.Month) + $date.Day
    return "$shortDate.$($match.Groups['revision'].Value)"
}

$officialBuildId = "$env:ARCADE_OFFICIAL_BUILD_ID"
if ([string]::IsNullOrWhiteSpace($officialBuildId)) {
    throw 'ARCADE_OFFICIAL_BUILD_ID is empty.'
}

$productBuildNumber = ConvertTo-ProductBuildNumber $officialBuildId
Set-BuildVariable BUILD_NUMBER $productBuildNumber

$rawBranch = "$env:BUILD_SOURCEBRANCH"
$pullRequestRef = [regex]::Match($rawBranch, '^refs/pull/(\d+)/merge$')
$isPullRequest = $env:BUILD_REASON -eq 'PullRequest' -or $pullRequestRef.Success
$prNumber = Get-FirstNonEmpty @(
    "$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER",
    "$env:SYSTEM_PULLREQUEST_PULLREQUESTID",
    $(if ($pullRequestRef.Success) { $pullRequestRef.Groups[1].Value } else { '' })
)

if ($isPullRequest -and [string]::IsNullOrWhiteSpace($prNumber)) {
    throw "Unable to determine the pull request number for '$rawBranch' from provider '$env:BUILD_REPOSITORY_PROVIDER'."
}

if ($isPullRequest) {
    Set-BuildVariable PR_NUMBER $prNumber
    if ([string]::IsNullOrWhiteSpace($env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER)) {
        Set-BuildVariable SYSTEM_PULLREQUEST_PULLREQUESTNUMBER $prNumber
    }
}

$sourceCommit = if ($isPullRequest) {
    Get-FirstNonEmpty @("$env:SYSTEM_PULLREQUEST_SOURCECOMMITID")
} else {
    ''
}
if ([string]::IsNullOrWhiteSpace($sourceCommit) -and $isPullRequest) {
    $mergeMessage = [regex]::Match("$env:BUILD_SOURCEVERSIONMESSAGE", '^Merge\s+([0-9a-fA-F]{7,40})\s+into\s+')
    if ($mergeMessage.Success) {
        $sourceCommit = $mergeMessage.Groups[1].Value
    }
}
if ([string]::IsNullOrWhiteSpace($sourceCommit)) {
    $sourceCommit = "$env:BUILD_SOURCEVERSION"
}

$sourceBranch = if ($isPullRequest) {
    Get-FirstNonEmpty @("$env:SYSTEM_PULLREQUEST_SOURCEBRANCH", $rawBranch)
} else {
    $rawBranch
}
$sourceRepository = Get-FirstNonEmpty @(
    "$env:SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI",
    "$env:BUILD_REPOSITORY_URI"
)

if ([string]::IsNullOrWhiteSpace($sourceCommit) -or
    [string]::IsNullOrWhiteSpace($sourceBranch) -or
    [string]::IsNullOrWhiteSpace($sourceRepository)) {
    throw "Incomplete source identity: commit='$sourceCommit', branch='$sourceBranch', repository='$sourceRepository'."
}

Set-BuildVariable GIT_SHA $sourceCommit
Set-BuildVariable GIT_BRANCH_NAME $sourceBranch
Set-BuildVariable GIT_URL $sourceRepository

$previewLabel = "$env:PREVIEW_LABEL".Trim().ToLowerInvariant()
if ($isPullRequest) {
    $previewLabel = "pr.$prNumber"
} elseif ($env:BUILD_REASON -eq 'Schedule') {
    $previewLabel = 'nightly'
}

if ([string]::IsNullOrWhiteSpace($previewLabel)) {
    throw "Preview label is empty for build reason '$env:BUILD_REASON'."
}

Set-BuildVariable PREVIEW_LABEL $previewLabel

$isReleaseBuild = $previewLabel -ceq 'stable'
$finalVersionKind = if ($isReleaseBuild) { 'release' } else { '' }
Set-BuildVariable DOTNET_FINAL_VERSION_KIND $finalVersionKind

if ($isReleaseBuild -and
    ($env:SYSTEM_TEAMPROJECT -ne 'internal' -or
     -not "$env:BUILD_SOURCEBRANCH".StartsWith('refs/heads/release/', [StringComparison]::OrdinalIgnoreCase))) {
    throw 'Exact release packages require an internal release/* branch.'
}

Write-Host '# Build identity'
Write-Host "Official build ID: $officialBuildId"
Write-Host "Product build number: $productBuildNumber"
Write-Host "Provider: $env:BUILD_REPOSITORY_PROVIDER"
Write-Host "Reason: $env:BUILD_REASON"
Write-Host "Source branch: $sourceBranch"
Write-Host "Source commit: $sourceCommit"
Write-Host "Source repository: $sourceRepository"
Write-Host "Preview label: $previewLabel"

if ($UpdateBuildNumber) {
    if ($isReleaseBuild) {
        $label = "$env:EXTENDED_VERSION+$officialBuildId"
    } else {
        $branchMetadata = if ($isPullRequest) { '' } else { "+$env:BUILD_SOURCEBRANCHNAME" }
        $label = "$env:EXTENDED_VERSION-$previewLabel.$productBuildNumber$branchMetadata"
    }

    Write-Host "Build label: $label"
    Write-Host "##vso[build.updatebuildnumber]$label"
}

exit 0
