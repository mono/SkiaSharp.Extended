Param(
    [Parameter(Mandatory)]
    [string] $OutputRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

$outputPath = [IO.Path]::GetFullPath($OutputRoot)
$nugetPath = Join-Path $outputPath 'nugets'
$assetPath = Join-Path $outputPath 'arcade-assets'
$shippingPath = Join-Path $assetPath 'Shipping'
$nonShippingPath = Join-Path $assetPath 'NonShipping'
$pdbPath = Join-Path $outputPath 'pdbs'

if (-not (Test-Path $nugetPath -PathType Container)) {
    throw "NuGet output directory '$nugetPath' does not exist."
}

Remove-Item $assetPath, $pdbPath -Recurse -Force -ErrorAction Ignore
New-Item $shippingPath, $nonShippingPath, $pdbPath -ItemType Directory -Force | Out-Null

function Get-PackageIdentity {
    param([Parameter(Mandatory)][IO.Compression.ZipArchive] $Archive)

    $nuspecEntries = @($Archive.Entries | Where-Object {
        $_.FullName -notmatch '[/\\]' -and $_.Name.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase)
    })
    if ($nuspecEntries.Count -ne 1) {
        throw "Package must contain exactly one root nuspec; found $($nuspecEntries.Count)."
    }

    $reader = [IO.StreamReader]::new($nuspecEntries[0].Open())
    try {
        [xml]$nuspec = $reader.ReadToEnd()
    } finally {
        $reader.Dispose()
    }

    $idNode = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='id']")
    $versionNode = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='version']")
    if ($null -eq $idNode -or $null -eq $versionNode -or
        [string]::IsNullOrWhiteSpace($idNode.InnerText) -or
        [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
        throw 'Package nuspec must contain non-empty id and version values.'
    }

    return @{
        Id = $idNode.InnerText
        Version = $versionNode.InnerText
    }
}

function Get-SafePdbTarget {
    param(
        [Parameter(Mandatory)][string] $PackageRoot,
        [Parameter(Mandatory)][string] $EntryPath
    )

    $normalized = $EntryPath.Replace('\', '/')
    $segments = $normalized.Split('/', [StringSplitOptions]::RemoveEmptyEntries)
    if ($normalized.StartsWith('/', [StringComparison]::Ordinal) -or
        $normalized -match '^[A-Za-z]:' -or
        $segments -contains '..') {
        throw "Package entry '$EntryPath' escapes its package root."
    }

    $root = [IO.Path]::GetFullPath($PackageRoot)
    $target = [IO.Path]::GetFullPath((Join-Path $root ($normalized.Replace('/', [IO.Path]::DirectorySeparatorChar))))
    $rootPrefix = $root.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $target.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Package entry '$EntryPath' escapes its package root."
    }

    return $target
}

$productPackages = @(Get-ChildItem $nugetPath -Filter '*.nupkg' -File)
if ($productPackages.Count -eq 0) {
    throw "No product NuGet packages were found in '$nugetPath'."
}

$symbolsPackages = @(Get-ChildItem $nugetPath -Filter '*.snupkg' -File)
$publishedNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$pdbCount = 0

foreach ($package in @($productPackages + $symbolsPackages)) {
    if (-not $publishedNames.Add($package.Name)) {
        throw "Duplicate Arcade asset name '$($package.Name)'."
    }
    Copy-Item $package.FullName (Join-Path $shippingPath $package.Name)
}

foreach ($package in @($productPackages + $symbolsPackages)) {
    $stream = [IO.File]::OpenRead($package.FullName)
    $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Read, $false)
    try {
        $identity = Get-PackageIdentity $archive
        $packagePdbRoot = Join-Path $pdbPath "$($identity.Id).$($identity.Version)"

        foreach ($entry in $archive.Entries) {
            $entryPath = $entry.FullName.Replace('\', '/')
            $target = Get-SafePdbTarget $packagePdbRoot $entryPath
            if ($package.Extension -ne '.snupkg' -or
                -not $entryPath.EndsWith('.pdb', [StringComparison]::OrdinalIgnoreCase) -or
                $entryPath.StartsWith('ref/', [StringComparison]::OrdinalIgnoreCase)) {
                continue
            }

            if (Test-Path $target) {
                throw "Duplicate PDB target '$target'."
            }

            New-Item ([IO.Path]::GetDirectoryName($target)) -ItemType Directory -Force | Out-Null
            $source = $entry.Open()
            $destination = [IO.File]::Create($target)
            try {
                $source.CopyTo($destination)
            } finally {
                $destination.Dispose()
                $source.Dispose()
            }
            $pdbCount++
        }
    } finally {
        $archive.Dispose()
        $stream.Dispose()
    }
}

if ($symbolsPackages.Count -eq 0) {
    throw "No symbol packages were found in '$nugetPath'."
}

if ($pdbCount -eq 0) {
    New-Item (Join-Path $pdbPath '.empty') -ItemType File | Out-Null
}
New-Item (Join-Path $nonShippingPath '.empty') -ItemType File | Out-Null

Write-Host "Prepared $($productPackages.Count) package(s), $($symbolsPackages.Count) symbol package(s), and $pdbCount loose PDB(s)."
