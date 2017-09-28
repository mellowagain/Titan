#addin "Cake.Incubator"
#tool "nuget:?package=xunit.runner.console"

Task("Clean")
    .Does(() =>
{
    DotNetCoreClean("./Titan");
    DotNetCoreClean("./Titan.Test");
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    if(!NuGetHasSource("https://www.myget.org/F/eto/api/v3/index.json")) {
        // First add MyGet Eto source
        NuGetAddSource(
            name: "MyGet.org Eto",
            source: "https://www.myget.org/F/eto/api/v3/index.json"
        );
    }

    NuGetRestore("./Titan.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreBuild("./Titan/");
    DotNetCoreBuild("./Titan.Test/");
});

/*Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2(GetFiles("./TitanTest/bin/" + config + "/Titan*.dll"), new XUnit2Settings()
    {
        Parallelism = ParallelismOption.All,
        OutputDirectory = "./TitanTest/bin/" + config + "/results"
    });
});*/

Task("Default").IsDependentOn(/*"Run-Unit-Tests"*/ "Build");
RunTarget("Default");
