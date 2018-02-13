#tool "nuget:?package=xunit.runner.console"

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
    
    CopyFile("./Titan/Properties/AssemblyInfo.cs", "./Titan/Properties/AssemblyInfo.cs.old");
    CopyFile("./TitanTest/Properties/AssemblyInfo.cs", "./TitanTest/Properties/AssemblyInfo.cs.old");
    
    TransformTextFile("./Titan/Properties/AssemblyInfo.cs")
        .WithToken("GitHash", gitHash)
        .Save("./Titan/Properties/AssemblyInfo.cs");
    
    TransformTextFile("./TitanTest/Properties/AssemblyInfo.cs")
        .WithToken("GitHash", gitHash)
        .Save("./TitanTest/Properties/AssemblyInfo.cs");
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
    XUnit2(GetFiles("./TitanTest/bin/" + config + "/TitanTest.dll"), new XUnit2Settings()
    {
        Parallelism = ParallelismOption.All,
        XmlReport = true,
        OutputDirectory = "./TitanTest/bin/" + config + "/results"
    });
});

Task("Cleanup")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    DeleteFile("./Titan/Properties/AssemblyInfo.cs");
    DeleteFile("./TitanTest/Properties/AssemblyInfo.cs");
    
    MoveFile("./Titan/Properties/AssemblyInfo.cs.old", "./Titan/Properties/AssemblyInfo.cs");
    MoveFile("./TitanTest/Properties/AssemblyInfo.cs.old", "./TitanTest/Properties/AssemblyInfo.cs");
});

RunTarget("Cleanup");
