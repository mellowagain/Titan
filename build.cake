#tool "nuget:?package=xunit.runner.console"

var config = Argument("configuration", "Release");
var runEnvironment = Argument("runenv", "local");

var buildDir = Directory("./Titan/bin/") + Directory(config);
var testDir = Directory("./TitanTest/bin/") + Directory(config);

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./Titan.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./Titan.sln", settings => settings.SetConfiguration(config));
    }
    else
    {
      // Use XBuild
      XBuild("./Titan.sln", settings => settings.SetConfiguration(config));
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

if(runEnvironment.ToLower() == "ci") {
    Task("Default").IsDependentOn("Run-Unit-Tests");
} else {
    Task("Default").IsDependentOn("Build");
}

RunTarget("Default");
