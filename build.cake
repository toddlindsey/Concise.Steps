#addin "Cake.Incubator"
#tool nuget:?package=vswhere
#tool "nuget:?package=GitVersion.CommandLine"
// NOTE If you want to allow running powershell scripts with arguments without first invoking powershell.exe, read: https://www.howtogeek.com/204166/how-to-configure-windows-to-work-with-powershell-scripts-more-easily/

// Usage:  build.cmd (Will build, test and pack)
// Usage:  build.cmd -target Push --apiKey==XYZ (push to nuget)

// Details on VSWHERE: http://cakebuild.net/blog/2017/03/vswhere-and-visual-studio-2017-support

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var apiKey = Argument<string>("apiKey", null);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var artifactsDir = "./Artifacts";
var buildDir = Directory(artifactsDir) + Directory(configuration);
GitVersion gitVersion = null;

DirectoryPath vsLatestPath = VSWhereLatest();
Information("Using VS Path: " + vsLatestPath);

FilePath msBuildPathX64 = (vsLatestPath==null)
                            ? null
                            : vsLatestPath.CombineWithFilePath("./MSBuild/Current/Bin/amd64/MSBuild.exe");

Information("MSBuild Path: " + msBuildPathX64);

string semVer = "0.5.6";
string netVer = "0.5.6.0";

gitVersion = new GitVersion {
	SemVer = semVer,
	NuGetVersionV2 = semVer
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("GitVersion").Does(() => {

	// Hardcoded version
	//gitVersion = new GitVersion {
	//	SemVer = "0.4.1-preview",
	//	NuGetVersionV2 = semVer
	//};

	// Use this to determine version from the Git label
    gitVersion = GitVersion(new GitVersionSettings {
	   UpdateAssemblyInfo = true
	});

    // Information("GitResults -> {0}", gitVersion.Dump());
});

Task("CreateSolutionInfo").Does(() => {

	CreateAssemblyInfo("./SolutionInfo.cs", new AssemblyInfoSettings {
		Product = "Concise.Steps",
		Version = netVer,
		FileVersion = netVer,
	    InformationalVersion = semVer,
		Copyright = "Copyright Todd Lindsey 2019"
	});

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
		settings.ToolPath = msBuildPathX64;
		settings.PlatformTarget = PlatformTarget.MSIL;
		settings.SetConfiguration(configuration);
	});
});

Task("Tests")
    .Does(() =>
{
	FilePath vsTestPath = vsLatestPath.CombineWithFilePath("./Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe");

	VSTest("./*.UnitTests.*/bin/" + configuration + "/*.UnitTests.*.dll", new VSTestSettings {
		ToolPath = vsTestPath
	});
});

Task("Pack")
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
			new NuSpecDependency { Id = "Concise.Steps", TargetFramework = "netstandard2.0", Version = "[" + gitVersion.NuGetVersionV2 + "]" }, // Require exact version match for now
			new NuSpecDependency { Id = "MSTest.TestFramework", TargetFramework = "netstandard2.0", Version = "[1.1.18,)" },
			new NuSpecDependency { Id = "MSTest.TestAdapter", TargetFramework = "netstandard2.0", Version = "[1.1.18,)" }
		}
    });

    NuGetPack("./Concise.Steps.NUnit.nuspec", new NuGetPackSettings {
		OutputDirectory = artifactsDir,
		Version = gitVersion.NuGetVersionV2,
		Dependencies = new [] { 
			new NuSpecDependency { Id = "Concise.Steps", TargetFramework = "netstandard2.0", Version = "[" + gitVersion.NuGetVersionV2 + "]" }, // Require exact version match for now
			new NuSpecDependency { Id = "NUnit", TargetFramework = "netstandard2.0", Version = "[3.11.0,)" }
		}
    });
});

Task("Push")
    .Does(() => 
{
    NuGetPush(artifactsDir + "/Concise.Steps." + gitVersion.SemVer + ".nupkg", new NuGetPushSettings {
		Source = "https://www.nuget.org/api/v2/package",
		ApiKey = apiKey
	});  

    NuGetPush(artifactsDir + "/Concise.Steps.MSTest." + gitVersion.SemVer + ".nupkg", new NuGetPushSettings {
		Source = "https://www.nuget.org/api/v2/package",
		ApiKey = apiKey
	});  

    NuGetPush(artifactsDir + "/Concise.Steps.NUnit." + gitVersion.SemVer + ".nupkg", new NuGetPushSettings {
		Source = "https://www.nuget.org/api/v2/package",
		ApiKey = apiKey
	});  
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Clean")
    //.IsDependentOn("GitVersion")
	.IsDependentOn("CreateSolutionInfo")
    .IsDependentOn("RestoreNuGet")
    .IsDependentOn("BuildSolution")
    .IsDependentOn("Tests")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
