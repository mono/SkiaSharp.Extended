$ErrorActionPreference = "Stop"

if (!$env:BUILD_NUMBER) {
    $betaPrefix = "preview"
} else {
    $betaPrefix = "preview$env:BUILD_NUMBER"
}

if ($IsMacOS) {
    $msbuild = "msbuild"
} else {
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    $msbuild = join-path $msbuild 'MSBuild\15.0\Bin\MSBuild.exe'
}

function Build
{
    Param ([string] $solution, [string] $output)

    if ($IsMacOS) {
        $extraProperties = "/p:Platform=iPhoneSimulator"
    }

    & $msbuild $solution /v:m /t:restore /p:Configuration=Release $extraProperties
    if ($lastexitcode -ne 0) { exit $lastexitcode }

    & $msbuild $solution /v:m /t:build /p:Configuration=Release $extraProperties
    if ($lastexitcode -ne 0) { exit $lastexitcode }
}

function Pack
{
    Param ([string] $project, [string] $output)

    & $msbuild $project /v:m /t:pack /p:Configuration=Release
    if ($lastexitcode -ne 0) { exit $lastexitcode }

    & $msbuild $project /v:m /t:pack /p:Configuration=Release /p:VersionSuffix="$betaPrefix"
    if ($lastexitcode -ne 0) { exit $lastexitcode }

    $dir = [System.IO.Path]::GetDirectoryName($project)
    New-Item -Path "./output/$output" -ItemType Directory -Force | Out-Null
    Copy-Item -Path "$dir/bin/Release" -Destination "./output/$output" -Recurse -Force
}

function Test
{
    Param ([string] $project, [string] $output)

    & $msbuild $project /v:m /t:test /p:Configuration=Release
    if ($lastexitcode -ne 0) { exit $lastexitcode }

    $dir = [System.IO.Path]::GetDirectoryName($project)
    New-Item -Path "./output/$output" -ItemType Directory -Force | Out-Null
    Copy-Item -Path "$dir/bin/Release/net47/TestResult.xml" -Destination "./output/$output" -Force
}

Write-Output "MSBuild path: '$msbuild'"


Build  "./SkiaSharp.Extended/source/SkiaSharp.Extended.csproj"  "SkiaSharp.Extended"
Pack  "./SkiaSharp.Extended/source/SkiaSharp.Extended.csproj"   "SkiaSharp.Extended"


# Write-Output "Building SkiaSharp.Extended..."
# Build "./SkiaSharp.Extended/SkiaSharp.Extended.sln"                 "SkiaSharp.Extended"
# Pack  "./SkiaSharp.Extended/source/SkiaSharp.Extended.csproj"       "SkiaSharp.Extended"
# Test  "./SkiaSharp.Extended/tests/SkiaSharp.Extended.Tests.csproj"  "SkiaSharp.Extended"

# Write-Output "Building SkiaSharp.Extended.Iconify..."
# Build "./SkiaSharp.Extended.Iconify/SkiaSharp.Extended.Iconify.sln"                                                                               "SkiaSharp.Extended.Iconify"
# Pack  "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify/SkiaSharp.Extended.Iconify.csproj"                                          "SkiaSharp.Extended.Iconify"
# Pack  "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.FontAwesome/SkiaSharp.Extended.Iconify.FontAwesome.csproj"                  "SkiaSharp.Extended.Iconify"
# Pack  "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.IonIcons/SkiaSharp.Extended.Iconify.IonIcons.csproj"                        "SkiaSharp.Extended.Iconify"
# Pack  "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.MaterialDesignIcons/SkiaSharp.Extended.Iconify.MaterialDesignIcons.csproj"  "SkiaSharp.Extended.Iconify"
# Pack  "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.MaterialIcons/SkiaSharp.Extended.Iconify.MaterialIcons.csproj"              "SkiaSharp.Extended.Iconify"
# Pack  "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.Meteocons/SkiaSharp.Extended.Iconify.Meteocons.csproj"                      "SkiaSharp.Extended.Iconify"
# Pack  "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.SimpleLineIcons/SkiaSharp.Extended.Iconify.SimpleLineIcons.csproj"          "SkiaSharp.Extended.Iconify"
# Pack  "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.Typicons/SkiaSharp.Extended.Iconify.Typicons.csproj"                        "SkiaSharp.Extended.Iconify"
# Pack  "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.WeatherIcons/SkiaSharp.Extended.Iconify.WeatherIcons.csproj"                "SkiaSharp.Extended.Iconify"

# Write-Output "Building SkiaSharp.Extended.Svg..."
# Build "./SkiaSharp.Extended.Svg/SkiaSharp.Extended.Svg.sln"                 "SkiaSharp.Extended.Svg"
# Pack  "./SkiaSharp.Extended.Svg/source/SkiaSharp.Extended.Svg.csproj"       "SkiaSharp.Extended.Svg"
# Test  "./SkiaSharp.Extended.Svg/tests/SkiaSharp.Extended.Svg.Tests.csproj"  "SkiaSharp.Extended.Svg"

exit $lastexitcode;