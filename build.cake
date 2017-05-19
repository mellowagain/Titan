#tool "nuget:?package=xunit.runners&version=1.9.2"

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
    XUnit(GetFiles("./TitanTest/bin/" + config + "/TitanTest.dll"), new XUnitSettings()
    {
        OutputDirectory = "./TitanTest/bin/" + config + "/results"
    });
});

if(runEnvironment.ToLower() == "ci") {
    Task("Default").IsDependentOn("Run-Unit-Tests");
} else {
    Task("Default").IsDependentOn("Build");
}

RunTarget("Default");
