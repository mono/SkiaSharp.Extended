#addin "Cake.FileHelpers"
#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosity = Argument("verbosity", "Verbose");

var names = Argument("Names", Argument("names", ""));

Task("Default")
    .Does(() =>
{
    DirectoryPathCollection projects;
    if (string.IsNullOrWhiteSpace(names)) {
        projects = GetDirectories("./SkiaSharp.Extended.*");
        projects.Add("./SkiaSharp.Extended");
    } else {
        projects = GetDirectories(names);
    }

    if (projects.Count == 0) {
        Error("No projects matched the names: '{0}'.", names);
    }

    foreach (var projectDirectory in projects) {
        var directory = MakeAbsolute(projectDirectory);
        var cake = directory.CombineWithFilePath("build.cake");
        var output = "./output/" + directory.GetDirectoryName();

        Information("Building {0}...", cake);
        CakeExecuteScript(cake, new CakeSettings { 
            Arguments = new Dictionary<string, string> {
                { "target", target },
                { "configuration", configuration },
                { "verbosity", verbosity },
            }
        });

        Information("Copying output for {0} to {1}...", cake, output);
        EnsureDirectoryExists(output);
        CopyDirectory(directory.Combine("output"), output);
    }
});

RunTarget("Default");
