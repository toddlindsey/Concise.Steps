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

var artifactsDir = "./Artifacts";
var buildDir = Directory(artifactsDir) + Directory(configuration);
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
	});
});

Task("BuildSolution")
    .Does(() =>
{
    MSBuild("./Concise.Steps.sln", settings => {
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
		OutputDirectory = artifactsDir,
		Version = gitVersion.NuGetVersionV2
    });

    NuGetPack("./Concise.Steps.MSTest.nuspec", new NuGetPackSettings {
		OutputDirectory = artifactsDir,
		Version = gitVersion.NuGetVersionV2,
		Dependencies = new [] { 
			new NuSpecDependency { Id = "Concise.Steps", Version = "[" + gitVersion.NuGetVersionV2 + "]" }, // Require exact version match for now
			new NuSpecDependency { TargetFramework = ".NETStandard1.6", Id = "NETStandard.Library", Version = "[1.6.1,)" },
			new NuSpecDependency { TargetFramework = ".NETStandard1.6", Id = "System.Reflection.TypeExtensions", Version = "[4.3.0,)" }
		}
    });
});

Task("Push")
    .IsDependentOn("GitVersion")
    .Does(() => 
{
    NuGetPush(artifactsDir + "/Concise.Steps." + gitVersion.SemVer + ".nupkg", new NuGetPushSettings {
		Source = "https://www.nuget.org/api/v2/package"
	});  

    NuGetPush(artifactsDir + "/Concise.Steps.MSTest." + gitVersion.SemVer + ".nupkg", new NuGetPushSettings {
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
