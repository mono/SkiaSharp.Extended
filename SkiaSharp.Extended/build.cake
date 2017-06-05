#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosity = Argument("verbosity", "Verbose");

Task("Build")
    .Does(() =>
{
    // build the PCL solution
    NuGetRestore("./source/SkiaSharp.Extended.NetFramework.sln");
    DotNetBuild("./source/SkiaSharp.Extended.NetFramework.sln", settings => settings.SetConfiguration(configuration));

    // copy output
    EnsureDirectoryExists("./output/portable");
    CopyFileToDirectory("./source/SkiaSharp.Extended/bin/" + configuration + "/SkiaSharp.Extended.dll", "./output/portable");

    // build the .NET Standard solution
    DotNetCoreRestore("./source/SkiaSharp.Extended.NetStandard");
    DotNetCoreBuild("./source/SkiaSharp.Extended.NetStandard.sln", new DotNetCoreBuildSettings { Configuration = configuration });

    // copy output
    EnsureDirectoryExists("./output/netstandard");
    CopyFileToDirectory("./source/SkiaSharp.Extended.NetStandard/bin/" + configuration + "/SkiaSharp.Extended.dll", "./output/netstandard");
});

Task("Package")
    .IsDependentOn("Build")
    .Does(() =>
{
    // create the package
    NuGetPack ("./nuget/SkiaSharp.Extended.nuspec", new NuGetPackSettings { 
        OutputDirectory = "./output/",
        BasePath = "./",
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    // build the tests
    NuGetRestore("./tests/SkiaSharp.Extended.Tests.sln");
    DotNetBuild("./tests/SkiaSharp.Extended.Tests.sln", settings => settings.SetConfiguration(configuration));

    // run the tests
    NUnit3("./tests/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        Results = "./output/TestResult.xml"
    });
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories ("./source/*/bin");
    CleanDirectories ("./source/*/obj");
    CleanDirectories ("./source/packages");

    CleanDirectories ("./output");
});

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    .IsDependentOn("Test");

RunTarget(target);
