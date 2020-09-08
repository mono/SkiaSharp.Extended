var TARGET = Argument("t", Argument("target", "ci"));

var PREVIEW_LABEL = Argument ("previewLabel", EnvironmentVariable ("PREVIEW_LABEL") ?? "preview");
var BUILD_NUMBER = EnvironmentVariable ("BUILD_NUMBER") ?? "0";
var GIT_SHA = Argument ("gitSha", EnvironmentVariable ("GIT_SHA") ?? "");
var GIT_BRANCH_NAME = Argument ("gitBranch", EnvironmentVariable ("GIT_BRANCH_NAME") ?? "");

Task("libs")
	.WithCriteria(Context.Environment.Platform.Family != PlatformFamily.Linux)
	.Does(() =>
{
	var settings = new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/libs.binlog")
		.SetConfiguration("Release")
		.WithRestore();

	var sln = IsRunningOnWindows()
		? "./SkiaSharp.Extended.sln"
		: "./SkiaSharp.Extended.Unix.sln";

	MSBuild(sln, settings);
});

Task("nugets")
	.IsDependentOn("libs")
	.Does(() =>
{
	var sln = IsRunningOnWindows()
		? "./source/Source.sln"
		: "./source/Source.Unix.sln";

	MSBuild(sln, new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/nugets.binlog")
		.SetConfiguration("Release")
		.WithRestore()
		.WithProperty("PackageOutputPath", MakeAbsolute(new FilePath("./output/")).FullPath)
		.WithTarget("Pack"));

	var preview = PREVIEW_LABEL;
	if (!string.IsNullOrEmpty (BUILD_NUMBER)) {
		preview += $".{BUILD_NUMBER}";
	}

	MSBuild(sln, new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/nugets-preview.binlog")
		.SetConfiguration("Release")
		.WithRestore()
		.WithProperty("PackageOutputPath", MakeAbsolute(new FilePath("./output/")).FullPath)
		.WithProperty("VersionSuffix", preview)
		.WithTarget("Pack"));
});

Task("tests")
	.IsDependentOn("libs")
	.Does(() =>
{
	var failed = 0;

	foreach (var csproj in GetFiles("./tests/*/*.csproj")) {
		// skip WPF on non-Windows
		if (!IsRunningOnWindows() && csproj.GetFilename().FullPath.Contains(".WPF."))
			continue;

		try {
			DotNetCoreTest(csproj.FullPath, new DotNetCoreTestSettings {
				Configuration = "Release",
				Logger = $"trx;LogFileName={csproj.GetFilenameWithoutExtension()}.trx",
			});
		} catch (Exception) {
			failed++;
		}
	}

	var output = $"./output/test-results/";
	EnsureDirectoryExists(output);
	CopyFiles($"./tests/**/TestResults/*.trx", output);

	if (failed > 0)
		throw new Exception($"{failed} tests have failed.");
});

Task("samples")
	.WithCriteria(Context.Environment.Platform.Family != PlatformFamily.Linux)
	.IsDependentOn("nugets")
	.Does(() =>
{
	var settings = new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/samples.binlog")
		.SetConfiguration("Release")
		.WithRestore();

	var sln = IsRunningOnWindows()
		? "./SkiaSharp.Extended.sln"
		: "./SkiaSharp.Extended.Unix.sln";

	MSBuild(sln, settings);
});

Task("ci")
	.IsDependentOn("libs")
	.IsDependentOn("nugets")
	.IsDependentOn("tests")
	.IsDependentOn("samples");

RunTarget(TARGET);
