#addin nuget:?package=Xamarin.Nuget.Validator&version=1.1.1

var target = Argument("t", Argument("target", "Default"));
var configuration = Argument("c", Argument("configuration", "Release"));
var project = Argument("p", Argument("project", ""));

var BUILD_NUMBER = Argument("build-number", EnvironmentVariable("BUILD_NUMBER")) ?? "0";
var PREVIEW_LABEL = Argument("preview-label", EnvironmentVariable("PREVIEW_LABEL")) ?? "preview";

var previewTag = $"{PREVIEW_LABEL}.{BUILD_NUMBER}";

var directories = string.IsNullOrWhiteSpace(project)
    ? GetDirectories("SkiaSharp.*")
    : new DirectoryPathCollection(new [] { new DirectoryPath(project) });

Task("build")
    .Does(() =>
{
    foreach (var subProject in directories) {
        Information($"Building {subProject}...");
        foreach (var sln in GetFiles($"{subProject}/*.sln")) {
            MSBuild(sln, c => {
                c.Configuration = configuration;
                c.MSBuildPlatform = MSBuildPlatform.x86;
                c.Restore = true;
                c.Verbosity = Verbosity.Minimal;
                if (!IsRunningOnWindows())
                    c.Properties["Platform"] = new [] { "iPhoneSimulator" };
            });
        }
    }
});

Task("nuget")
    .IsDependentOn("build")
    .Does(() =>
{
    foreach (var subProject in directories) {
        Information($"Packing {subProject}...");
        foreach (var csproj in GetFiles($"{subProject}/source/**/*.csproj")) {
            MSBuild(csproj, c => {
                c.Configuration = configuration;
                c.MSBuildPlatform = MSBuildPlatform.x86;
                c.Restore = true;
                c.Verbosity = Verbosity.Minimal;
                c.Targets.Clear();
                c.Targets.Add("Pack");
            });
            MSBuild(csproj, c => {
                c.Configuration = configuration;
                c.MSBuildPlatform = MSBuildPlatform.x86;
                c.Restore = true;
                c.Verbosity = Verbosity.Minimal;
                c.Targets.Clear();
                c.Targets.Add("Pack");
                c.Properties["VersionSuffix"] = new [] { previewTag };
            });

            var output = $"output/{subProject.GetDirectoryName()}";
            EnsureDirectoryExists(output);

            var bin = csproj.GetDirectory().Combine($"bin/{configuration}/");
            CopyDirectory(bin, output);

            EnsureDirectoryExists("output/nugets");
            CopyFiles($"{bin}/*.nupkg", "output/nugets");
        }
    }
});

Task("test")
    .IsDependentOn("build")
    .Does(() =>
{
    foreach (var subProject in directories) {
        Information($"Testing {subProject}...");
        foreach (var csproj in GetFiles($"{subProject}/tests/**/*.csproj")) {
            MSBuild(csproj, c => {
                c.Configuration = configuration;
                c.MSBuildPlatform = MSBuildPlatform.x86;
                c.Restore = true;
                c.Verbosity = Verbosity.Minimal;
                c.Targets.Clear();
                c.Targets.Add("Test");
            });

            var output = $"output/{subProject.GetDirectoryName()}";
            EnsureDirectoryExists(output);

            var bin = csproj.GetDirectory().Combine($"bin/{configuration}/");
            CopyFiles($"{bin}/**/TestResult.xml", output);
        }
    }
});

Task("nuget-validation")
    .IsDependentOn("nuget")
    .Does(() =>
{
    var options = new Xamarin.Nuget.Validator.NugetValidatorOptions {
        Copyright = "© Microsoft Corporation. All rights reserved.",
        Author = "Microsoft",
        Owner = "Microsoft",
        NeedsProjectUrl = true,
        NeedsLicenseUrl = true,
        ValidateRequireLicenseAcceptance = true,
        ValidPackageNamespace = new [] { "SkiaSharp" },
    };

    var nupkgFiles = GetFiles("./output/nugets/*.nupkg");
    Information("Found ({0}) NuGets to validate", nupkgFiles.Count());
    foreach (var nupkgFile in nupkgFiles) {
        Information("Verifiying Metadata of {0}...", nupkgFile.GetFilename());

        var result = Xamarin.Nuget.Validator.NugetValidator.Validate(MakeAbsolute(nupkgFile).FullPath, options);
        if (!result.Success) {
            Information("Metadata validation failed for: {0} \n\n", nupkgFile.GetFilename());
            Information(string.Join("\n    ", result.ErrorMessages));
            throw new Exception($"Invalid Metadata for: {nupkgFile.GetFilename()}");

        } else {
            Information("Metadata validation passed for: {0}", nupkgFile.GetFilename());
        }
    }
});

Task("Default")
    .IsDependentOn("build")
    .IsDependentOn("nuget")
    .IsDependentOn("nuget-validation")
    .IsDependentOn("test");

RunTarget(target);
