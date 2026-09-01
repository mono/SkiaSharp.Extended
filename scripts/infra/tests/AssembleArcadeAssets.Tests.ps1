$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

$scriptPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../assemble-arcade-assets.ps1'))
$pwsh = (Get-Command pwsh).Source
$root = Join-Path ([IO.Path]::GetTempPath()) "extended-arcade-assets-$([Guid]::NewGuid().ToString('N'))"

function New-Package {
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $Id,
        [Parameter(Mandatory)][string] $Version,
        [Parameter(Mandatory)][hashtable] $Entries
    )

    $allEntries = @{
        "$Id.nuspec" = "<package><metadata><id>$Id</id><version>$Version</version></metadata></package>"
    }
    foreach ($pair in $Entries.GetEnumerator()) {
        $allEntries[$pair.Key] = $pair.Value
    }

    $stream = [IO.File]::Create($Path)
    $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create, $false)
    try {
        foreach ($pair in $allEntries.GetEnumerator()) {
            $entry = $archive.CreateEntry($pair.Key)
            $writer = [IO.StreamWriter]::new($entry.Open())
            try {
                $writer.Write($pair.Value)
            } finally {
                $writer.Dispose()
            }
        }
    } finally {
        $archive.Dispose()
        $stream.Dispose()
    }
}

function Invoke-Assembly {
    param([Parameter(Mandatory)][string] $OutputRoot, [switch] $ExpectFailure)

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $pwsh
    foreach ($argument in @('-NoLogo', '-NoProfile', '-File', $scriptPath, '-OutputRoot', $OutputRoot)) {
        $startInfo.ArgumentList.Add($argument)
    }
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $process.Start() | Out-Null
    $output = $process.StandardOutput.ReadToEnd()
    $errorOutput = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($ExpectFailure) {
        if ($process.ExitCode -eq 0) {
            throw "Asset assembly unexpectedly succeeded.`n$output"
        }
    } elseif ($process.ExitCode -ne 0) {
        throw "Asset assembly failed with exit code $($process.ExitCode).`n$output`n$errorOutput"
    }

    return "$output`n$errorOutput"
}

try {
    $normal = Join-Path $root 'normal'
    $normalNugets = New-Item (Join-Path $normal 'nugets') -ItemType Directory -Force
    New-Package (Join-Path $normalNugets 'Foo.1.0.0.nupkg') 'Foo' '1.0.0' @{
        'lib/net8.0/Foo.dll' = 'dll'
        'lib/net8.0/Foo.pdb' = 'pdb'
        'ref/net8.0/Foo.pdb' = 'reference'
        'runtimes/win-x64/native/Foo.pdb' = 'native'
    }
    New-Package (Join-Path $normalNugets 'Foo.1.0.0.snupkg') 'Foo' '1.0.0' @{
        'lib/net8.0/Foo.pdb' = 'symbols'
        'ref/net8.0/Foo.pdb' = 'reference-symbols'
        'runtimes/win-x64/native/Foo.pdb' = 'native-symbols'
    }

    Invoke-Assembly $normal | Out-Null

    foreach ($path in @(
        'arcade-assets/Shipping/Foo.1.0.0.nupkg',
        'arcade-assets/Shipping/Foo.1.0.0.snupkg',
        'arcade-assets/NonShipping/.empty',
        'pdbs/Foo.1.0.0/lib/net8.0/Foo.pdb',
        'pdbs/Foo.1.0.0/runtimes/win-x64/native/Foo.pdb')) {
        if (-not (Test-Path (Join-Path $normal $path) -PathType Leaf)) {
            throw "Missing expected Arcade asset '$path'."
        }
    }
    if (Test-Path (Join-Path $normal 'pdbs/Foo.1.0.0/ref/net8.0/Foo.pdb')) {
        throw 'Reference-assembly PDBs must not be published.'
    }

    $empty = Join-Path $root 'empty'
    $emptyNugets = New-Item (Join-Path $empty 'nugets') -ItemType Directory -Force
    New-Package (Join-Path $emptyNugets 'Empty.1.0.0.nupkg') 'Empty' '1.0.0' @{
        'lib/net8.0/Empty.dll' = 'dll'
    }
    New-Package (Join-Path $emptyNugets 'Empty.1.0.0.snupkg') 'Empty' '1.0.0' @{
        'README.md' = 'symbols'
    }
    Invoke-Assembly $empty | Out-Null
    $emptyPdbFiles = @(Get-ChildItem (Join-Path $empty 'pdbs') -File -Recurse -Force)
    if ($emptyPdbFiles.Count -ne 1 -or $emptyPdbFiles[0].Name -ne '.empty') {
        throw 'PdbArtifacts must contain only .empty when no eligible PDB exists.'
    }

    $escaping = Join-Path $root 'escaping'
    $escapingNugets = New-Item (Join-Path $escaping 'nugets') -ItemType Directory -Force
    New-Package (Join-Path $escapingNugets 'Escaping.1.0.0.nupkg') 'Escaping' '1.0.0' @{
        '../escape.pdb' = 'escape'
    }
    New-Package (Join-Path $escapingNugets 'Escaping.1.0.0.snupkg') 'Escaping' '1.0.0' @{
        'README.md' = 'symbols'
    }
    $failure = Invoke-Assembly $escaping -ExpectFailure
    if ($failure -notmatch 'escapes its package root') {
        throw "Escaping package failed for the wrong reason.`n$failure"
    }

    Write-Host 'Arcade asset assembly tests passed.'
} finally {
    Remove-Item $root -Recurse -Force -ErrorAction Ignore
}
