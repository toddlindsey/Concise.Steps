#addin "Cake.Incubator"
#tool nuget:?package=vswhere
#tool "nuget:?package=GitVersion.CommandLine"
// NOTE If you want to allow running powershell scripts with arguments without first invoking powershell.exe, read: https://www.howtogeek.com/204166/how-to-configure-windows-to-work-with-powershell-scripts-more-easily/

// Details on VSWHERE: http://cakebuild.net/blog/2017/03/vswhere-and-visual-studio-2017-support

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

DirectoryPath vsLatestPath = VSWhereLatest(new VSWhereLatestSettings { Requires = "Microsoft.VisualStudio.Workload.ManagedDesktop"});
Information("Using VS Path: " + vsLatestPath);

FilePath msBuildPathX64 = (vsLatestPath==null)
                            ? null
                            : vsLatestPath.CombineWithFilePath("./MSBuild/15.0/Bin/amd64/MSBuild.exe");

Information("MSBuild Path: " + msBuildPathX64);

//DirectoryPath vs2017Path = VSWhereLegacy(new VSWhereLegacySettings { Version = "15.0"}).First();

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("CreateSolutionInfo").Does(() => {

	string semVer = "0.4.0-preview1";
	string netVer = "0.4.0.0";

	CreateAssemblyInfo("./SolutionInfo.cs", new AssemblyInfoSettings {
		Product = "Concise.Steps",
		Version = netVer,
		FileVersion = netVer,
	    InformationalVersion = semVer,
		Copyright = "Copyright Todd Lindsey 2017"
	});

	gitVersion = new GitVersion {
		SemVer = semVer,
		NuGetVersionV2 = semVer
	};
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
		//settings.ToolVersion = MSBuildToolVersion.VS2017;
		settings.ToolPath = msBuildPathX64;
		settings.PlatformTarget = PlatformTarget.MSIL;
		settings.SetConfiguration(configuration);
	});
});

Task("Tests")
    .Does(() =>
{
	FilePath vsTestPath = vsLatestPath.CombineWithFilePath("./Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe");

	VSTest("./*.UnitTests.*/**/bin/" + configuration + "/*.UnitTests.*.dll", new VSTestSettings {
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
			new NuSpecDependency { Id = "MSTest.TestFramework", Version = "[1.1.18,)" },
			new NuSpecDependency { Id = "MSTest.TestAdapter", Version = "[1.1.18,)" },
			new NuSpecDependency { TargetFramework = ".NETStandard2.0", Id = "NETStandard.Library", Version = "[2.0.0-preview2-25401-01,)" }
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
	//.IsDependentOn("CreateSolutionInfo")
    .IsDependentOn("RestoreNuGet")
    .IsDependentOn("BuildSolution")
    .IsDependentOn("Tests")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
