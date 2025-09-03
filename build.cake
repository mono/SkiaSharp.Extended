var TARGET = Argument("t", Argument("target", "Default"));
var CONFIGURATION = Argument("c", Argument("configuration", "Release"));
var PREVIEW_LABEL = Argument("previewLabel", EnvironmentVariable("PREVIEW_LABEL") ?? "preview");
var BUILD_NUMBER = Argument("buildNumber", EnvironmentVariable("BUILD_NUMBER") ?? "0");
var GIT_SHA = Argument("gitSha", EnvironmentVariable("GIT_SHA") ?? "");
var GIT_BRANCH_NAME = Argument("gitBranch", EnvironmentVariable("GIT_BRANCH_NAME") ?? "");

var OUTPUT_ROOT = MakeAbsolute((DirectoryPath)"./output/");

ProcessArgumentBuilder AppendForwardingLogger(ProcessArgumentBuilder args)
{
	if (BuildSystem.IsLocalBuild)
	    return args;

	// URL copied from https://github.com/microsoft/azure-pipelines-tasks/blob/7faf3e8146d43753b9f360edfae3d2e75ad78c76/Tasks/DotNetCoreCLIV2/make.json
	var loggerUrl = "https://vstsagenttools.blob.core.windows.net/tools/msbuildlogger/3/msbuildlogger.zip";

	var loggerDir = OUTPUT_ROOT.Combine("msbuildlogger");
	EnsureDirectoryExists(loggerDir);

	var loggerZip = loggerDir.CombineWithFilePath("msbuildlogger.zip");
	if (!FileExists(loggerZip))
		DownloadFile(loggerUrl, loggerZip);

	var loggerDll = loggerDir.CombineWithFilePath("Microsoft.TeamFoundation.DistributedTask.MSBuild.Logger.dll");
	if (!FileExists(loggerDll))
		Unzip(loggerZip, loggerDir);

	return args.Append($"-dl:CentralLogger,\"{loggerDll}\"*ForwardingLogger,\"{loggerDll}\"");
}

Task("build")
	.Does(() =>
{
	DotNetBuild("./SkiaSharp.Extended.sln", new DotNetBuildSettings
	{
		Configuration = CONFIGURATION,
		MSBuildSettings = new DotNetMSBuildSettings()
			.EnableBinaryLogger(OUTPUT_ROOT.Combine("binlogs").CombineWithFilePath("build.binlog").FullPath),
		ArgumentCustomization = AppendForwardingLogger
	});
});

Task("pack")
	.Does(() =>
{
	DotNetPack("./scripts/SkiaSharp.Extended-Pack.slnf", new DotNetPackSettings
	{
		Configuration = CONFIGURATION,
		MSBuildSettings = new DotNetMSBuildSettings()
			.EnableBinaryLogger(OUTPUT_ROOT.Combine("binlogs").CombineWithFilePath("pack.binlog").FullPath),
		OutputDirectory = OUTPUT_ROOT.Combine("nugets"),
		ArgumentCustomization = AppendForwardingLogger
	});

	var preview = PREVIEW_LABEL;
	if (!string.IsNullOrEmpty(BUILD_NUMBER))
	{
		preview += $".{BUILD_NUMBER}";
	}

	DotNetPack("./scripts/SkiaSharp.Extended-Pack.slnf", new DotNetPackSettings
	{
		Configuration = CONFIGURATION,
		MSBuildSettings = new DotNetMSBuildSettings()
			.EnableBinaryLogger(OUTPUT_ROOT.Combine("binlogs").CombineWithFilePath("pack-preview.binlog").FullPath),
		OutputDirectory = OUTPUT_ROOT.Combine("nugets"),
		VersionSuffix = preview,
		ArgumentCustomization = AppendForwardingLogger
	});

	CopyFileToDirectory("./source/SignList.xml", "./output/nugets");
});

Task("test")
	.Does(() =>
{
	DotNetTest("./scripts/SkiaSharp.Extended-Test.slnf", new DotNetTestSettings
	{
		Configuration = CONFIGURATION,
		Loggers = ["trx"],
		ResultsDirectory = OUTPUT_ROOT.Combine("test-results"),
		MSBuildSettings = new DotNetMSBuildSettings()
			.EnableBinaryLogger(OUTPUT_ROOT.Combine("binlogs").CombineWithFilePath("test.binlog").FullPath),
		ArgumentCustomization = AppendForwardingLogger
	});
});

Task("Default")
	.IsDependentOn("build")
	.IsDependentOn("pack")
	.IsDependentOn("test");

RunTarget(TARGET);
