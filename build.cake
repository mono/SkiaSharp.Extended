var TARGET = Argument("t", Argument("target", "Default"));
var PREVIEW_LABEL = Argument ("previewLabel", EnvironmentVariable ("PREVIEW_LABEL") ?? "preview");
var BUILD_NUMBER = EnvironmentVariable ("BUILD_NUMBER") ?? "0";
var GIT_SHA = Argument ("gitSha", EnvironmentVariable ("GIT_SHA") ?? "");
var GIT_BRANCH_NAME = Argument ("gitBranch", EnvironmentVariable ("GIT_BRANCH_NAME") ?? "");

Task("build")
	.Does(() =>
{
	var settings = new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/build.binlog")
		.SetConfiguration("Release")
		.SetMaxCpuCount(0)
		.WithRestore();

	MSBuild("./SkiaSharp.Extended.sln", settings);
});

Task("pack")
	.Does(() =>
{
	MSBuild("./SkiaSharp.Extended-Pack.slnf", new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/pack.binlog")
		.SetConfiguration("Release")
		.SetMaxCpuCount(0)
		.WithRestore()
		.WithProperty("PackageOutputPath", MakeAbsolute(new FilePath("./output/")).FullPath)
		.WithTarget("Pack"));

	var preview = PREVIEW_LABEL;
	if (!string.IsNullOrEmpty (BUILD_NUMBER)) {
		preview += $".{BUILD_NUMBER}";
	}

	MSBuild("./SkiaSharp.Extended-Pack.slnf", new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/pack-preview.binlog")
		.SetConfiguration("Release")
		.SetMaxCpuCount(0)
		.WithRestore()
		.WithProperty("PackageOutputPath", MakeAbsolute(new FilePath("./output/")).FullPath)
		.WithProperty("VersionSuffix", preview)
		.WithTarget("Pack"));

	CopyFileToDirectory("./source/SignList.xml", "./output/");
});

Task("test")
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
				Loggers = new [] { $"trx;LogFileName={csproj.GetFilenameWithoutExtension()}.trx" },
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

Task("Default")
	.IsDependentOn("build")
	.IsDependentOn("pack")
	.IsDependentOn("test");

RunTarget(TARGET);
