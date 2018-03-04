#!/bin/bash
set -e

[ -z "$BUILD_NUMBER" ] && BUILD_NUMBER="0"

# Building SkiaSharp.Extended...
msbuild "./SkiaSharp.Extended/SkiaSharp.Extended.sln" /m /t:restore /p:Configuration=Release /p:Platform=iPhoneSimulator /v:minimal /flp:logfile="./output/SkiaSharp.Extended/restore.log;verbosity=normal"
msbuild "./SkiaSharp.Extended/SkiaSharp.Extended.sln" /m /t:build /p:Configuration=Release /p:Platform=iPhoneSimulator /v:minimal /flp:logfile="./output/SkiaSharp.Extended/build.log;verbosity=normal"
msbuild "./SkiaSharp.Extended/source/SkiaSharp.Extended.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended/source/SkiaSharp.Extended.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended/source/bin/Release/" "./output/SkiaSharp.Extended/"
msbuild "./SkiaSharp.Extended/tests/SkiaSharp.Extended.Tests.csproj" /m /t:test /p:Configuration=Release /v:minimal /flp:logfile="./output/SkiaSharp.Extended/test.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended/tests/bin/Release/net47/TestResult.xml" "./output/SkiaSharp.Extended/"

# Building SkiaSharp.Extended.Iconify...
msbuild "./SkiaSharp.Extended.Iconify/SkiaSharp.Extended.Iconify.sln" /m /t:restore /p:Configuration=Release /p:Platform=iPhoneSimulator /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/restore.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/SkiaSharp.Extended.Iconify.sln" /m /t:build /p:Configuration=Release /p:Platform=iPhoneSimulator /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/build.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify/SkiaSharp.Extended.Iconify.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify/SkiaSharp.Extended.Iconify.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify/bin/Release/" "./output/SkiaSharp.Extended.Iconify/"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.FontAwesome/SkiaSharp.Extended.Iconify.FontAwesome.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.FontAwesome/SkiaSharp.Extended.Iconify.FontAwesome.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.FontAwesome/bin/Release/" "./output/SkiaSharp.Extended.Iconify/"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.IonIcons/SkiaSharp.Extended.Iconify.IonIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.IonIcons/SkiaSharp.Extended.Iconify.IonIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.IonIcons/bin/Release/" "./output/SkiaSharp.Extended.Iconify/"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.MaterialDesignIcons/SkiaSharp.Extended.Iconify.MaterialDesignIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.MaterialDesignIcons/SkiaSharp.Extended.Iconify.MaterialDesignIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.MaterialDesignIcons/bin/Release/" "./output/SkiaSharp.Extended.Iconify/"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.MaterialIcons/SkiaSharp.Extended.Iconify.MaterialIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.MaterialIcons/SkiaSharp.Extended.Iconify.MaterialIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.MaterialIcons/bin/Release/" "./output/SkiaSharp.Extended.Iconify/"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.Meteocons/SkiaSharp.Extended.Iconify.Meteocons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.Meteocons/SkiaSharp.Extended.Iconify.Meteocons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.Meteocons/bin/Release/" "./output/SkiaSharp.Extended.Iconify/"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.SimpleLineIcons/SkiaSharp.Extended.Iconify.SimpleLineIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.SimpleLineIcons/SkiaSharp.Extended.Iconify.SimpleLineIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.SimpleLineIcons/bin/Release/" "./output/SkiaSharp.Extended.Iconify/"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.Typicons/SkiaSharp.Extended.Iconify.Typicons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.Typicons/SkiaSharp.Extended.Iconify.Typicons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.Typicons/bin/Release/" "./output/SkiaSharp.Extended.Iconify/"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.WeatherIcons/SkiaSharp.Extended.Iconify.WeatherIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.WeatherIcons/SkiaSharp.Extended.Iconify.WeatherIcons.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Iconify/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Iconify/source/SkiaSharp.Extended.Iconify.WeatherIcons/bin/Release/" "./output/SkiaSharp.Extended.Iconify/"


# Building SkiaSharp.Extended.Svg...
msbuild "./SkiaSharp.Extended.Svg/SkiaSharp.Extended.Svg.sln" /m /t:restore /p:Configuration=Release /p:Platform=iPhoneSimulator /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Svg/restore.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Svg/SkiaSharp.Extended.Svg.sln" /m /t:build /p:Configuration=Release /p:Platform=iPhoneSimulator /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Svg/build.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Svg/source/SkiaSharp.Extended.Svg.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Svg/pack.log;verbosity=normal"
msbuild "./SkiaSharp.Extended.Svg/source/SkiaSharp.Extended.Svg.csproj" /m /t:pack /p:Configuration=Release /p:VersionSuffix=".$BUILD_NUMBER-beta" /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Svg/pack-beta.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Svg/source/bin/Release/" "./output/SkiaSharp.Extended.Svg/"
msbuild "./SkiaSharp.Extended.Svg/tests/SkiaSharp.Extended.Svg.Tests.csproj" /m /t:test /p:Configuration=Release /v:minimal /flp:logfile="./output/SkiaSharp.Extended.Svg/test.log;verbosity=normal"
cp -rf "./SkiaSharp.Extended.Svg/tests/bin/Release/net47/TestResult.xml" "./output/SkiaSharp.Extended.Svg/"

# exit $lastexitcode;