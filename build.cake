#tool "nuget:?package=xunit.runner.console"

var config = Argument("configuration", "Release");

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
    if (!NuGetHasSource("https://www.myget.org/F/eto/api/v3/index.json")) {
        // First add MyGet Eto source
        NuGetAddSource(
            name: "MyGet.org Eto",
            source: "https://www.myget.org/F/eto/api/v3/index.json"
        );
    }

    DotNetCoreRestore();
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreBuild("./Titan");
    DotNetCoreBuild("./Titan.Test");
});

/*Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2(GetFiles("./TitanTest/bin/" + config + "/TitanTest.dll"), new XUnit2Settings()
    {
        Parallelism = ParallelismOption.All,
        XmlReport = true,
        OutputDirectory = "./TitanTest/bin/" + config + "/results"
    });
});*/

RunTarget(/*"Run-Unit-Tests"*/ "Build");
