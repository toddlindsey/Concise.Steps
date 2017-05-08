#addin "Cake.Incubator"
#tool nuget:?package=vswhere
#tool "nuget:?package=GitVersion.CommandLine"
// NOTE If you want to allow running powershell scripts with arguments without first invoking powershell.exe, read: https://www.howtogeek.com/204166/how-to-configure-windows-to-work-with-powershell-scripts-more-easily/

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var buildDir = Directory("./Artifacts") + Directory(configuration);
GitVersion gitVersion = null; 
DirectoryPath vs2017Path = VSWhereLegacy(new VSWhereLegacySettings { Version = "15.0"}).First();

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("GitVersion").Does(() => {
    gitVersion = GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true
	});

    Information("GitResults -> {0}", gitVersion.Dump());
});

Task("RestoreNuGet")
    .Does(() =>
{
    NuGetRestore("./Concise.Steps.sln", new NuGetRestoreSettings 
	{ 
		Verbosity = NuGetVerbosity.Detailed
		//ToolPath = "./build/nuget.exe"
	});
});

Task("BuildSolution")
    .Does(() =>
{
    MSBuild("./Concise.Steps.sln", settings => {
		// settings.ToolPath = String.IsNullOrEmpty(toolpath) ? settings.ToolPath : toolpath;
		settings.ToolVersion = MSBuildToolVersion.VS2017;
		settings.PlatformTarget = PlatformTarget.MSIL;
		settings.SetConfiguration(configuration);
	});
});

Task("Tests")
    .Does(() =>
{
	FilePath vsTestPath = vs2017Path.CombineWithFilePath("./Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe");

	VSTest("./*.UnitTests/**/bin/" + configuration + "/*.UnitTests.dll", new VSTestSettings {
		ToolPath = vsTestPath
	});
});

Task("Pack")
    .IsDependentOn("GitVersion")
    .Does(() => 
{
    NuGetPack("./Concise.Steps.nuspec", new NuGetPackSettings {
		OutputDirectory = "./Artifacts",
		Version = gitVersion.NuGetVersionV2
    });  
});

Task("Push")
    .IsDependentOn("GitVersion")
    .Does(() => 
{
	var nupkgPath = "./Artifacts/Concise.Steps." + gitVersion.SemVer + ".nupkg";

    NuGetPush(nupkgPath, new NuGetPushSettings {
		Source = "https://www.nuget.org/api/v2/package"
	});  
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("GitVersion")
    .IsDependentOn("RestoreNuGet")
    .IsDependentOn("BuildSolution")
    .IsDependentOn("Tests")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
