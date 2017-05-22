#tool "nuget:?package=xunit.runner.console"

var config = Argument("configuration", "Release");

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

Task("Default").IsDependentOn("Run-Unit-Tests");
RunTarget("Default");
