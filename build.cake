#tool "nuget:?package=xunit.runner.console"
#addin "nuget:?package=Cake.FileHelpers&version=2.0.0"

var config = Argument("configuration", "Release");
var gitHash = Argument("githash", "Unknown Git Hash");
var buildDir = Directory("./Titan/bin/") + Directory(config);

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    if (!NuGetHasSource("https://www.myget.org/F/eto/api/v3/index.json")) {
        // First add MyGet Eto source
        NuGetAddSource(
            name: "MyGet.org Eto",
            source: "https://www.myget.org/F/eto/api/v3/index.json"
        );
    }

    NuGetRestore("./Titan.sln");
});

Task("Set-Version-To-Current-Git-Hash")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    Information("Building for Git Hash: " + gitHash);
    
    ReplaceRegexInFiles("./Titan/Properties/AssemblyInfo.cs", 
                        "(?<=AssemblyInformationalVersion\\(\")(.+?)(?=\"\\))",
                        "1.6.0-" + gitHash);
    ReplaceRegexInFiles("./TitanTest/Properties/AssemblyInfo.cs", 
                            "(?<=AssemblyInformationalVersion\\(\")(.+?)(?=\"\\))",
                            "1.6.0-" + gitHash);
});

Task("Build")
    .IsDependentOn("Set-Version-To-Current-Git-Hash")
    .Does(() =>
{
    if (IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./Titan.sln", settings => settings.SetConfiguration(config));
    }
    else
    {
      // Use MSBuild 15 provided by Mono
      MSBuild("./Titan.sln", new MSBuildSettings {
              Configuration = config,
              ToolPath = "/usr/lib/mono/msbuild/15.0/bin/MSBuild.dll"
      });
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2(GetFiles("./TitanTest/bin/" + config + "/Titan*.dll"), new XUnit2Settings()
    {
        Parallelism = ParallelismOption.All,
        OutputDirectory = "./TitanTest/bin/" + config + "/results"
    });
});

RunTarget("Run-Unit-Tests");
