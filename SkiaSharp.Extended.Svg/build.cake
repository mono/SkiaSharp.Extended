#load "../common.cake"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosity = Argument("verbosity", "Verbose");

var buildSpec = new BuildSpec {
    Libs = new ISolutionBuilder [] {
        new DefaultSolutionBuilder {
            PreBuildAction = () => {
                // restore netstandard
                var res = StartProcess("dotnet", new ProcessSettings {
                    Arguments = "restore ./source/SkiaSharp.Extended.Svg.NetStandard.sln"
                });
                if (res != 0) throw new Exception("dotnet returned " + res);
            },
            PostBuildAction = () => {
                // build netstandard
                var res = StartProcess("dotnet", new ProcessSettings {
                    Arguments = "build -c " + configuration + " ./source/SkiaSharp.Extended.Svg.NetStandard.sln"
                });
                if (res != 0) throw new Exception("dotnet returned " + res);
            },
            AlwaysUseMSBuild = true,
            BuildsOn = BuildPlatforms.Windows | BuildPlatforms.Mac,
            SolutionPath = "./source/SkiaSharp.Extended.Svg.NetFramework.sln",
            Configuration = configuration,
            OutputFiles = new [] { 
                new OutputFileCopy {
                    FromFile = "./source/SkiaSharp.Extended.Svg/bin/Release/SkiaSharp.Svg.dll",
                    ToDirectory = "./output/portable"
                },
                new OutputFileCopy {
                    FromFile = "./source/SkiaSharp.Extended.Svg.NetStandard/bin/Release/SkiaSharp.Svg.dll",
                    ToDirectory = "./output/netstandard"
                },
            }
        },
    },

    // Samples = new ISolutionBuilder [] {
    //     new DefaultSolutionBuilder { SolutionPath = "./samples/SkiaSharpDemo.sln" },
    // },

    NuGets = new [] {
        new NuGetInfo { NuSpec = "./nuget/SkiaSharp.Extended.Svg.nuspec"},
    },
};

Task("tests")
    .IsDependentOn("libs")
    .Does(() =>
{
    // build the tests
    NuGetRestore("./tests/SkiaSharp.Extended.Svg.Tests.sln");
    DotNetBuild("./tests/SkiaSharp.Extended.Svg.Tests.sln", settings => settings.SetConfiguration(configuration));

    // run the tests
    NUnit3("./tests/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        Results = "./output/TestResult.xml",
        ResultFormat = "nunit2",
    });
});

Task("Default")
    .IsDependentOn("libs")
    .IsDependentOn("nuget")
    .IsDependentOn("tests");

SetupXamarinBuildTasks (buildSpec, Tasks, Task);

RunTarget(target);
