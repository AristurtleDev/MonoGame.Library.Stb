namespace BuildScripts;

[TaskName("Build Windows")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildWindowsTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        BuildArchitecture(context, "x64", "win-x64");
        BuildArchitecture(context, "ARM64", "win-arm64");
    }

    private static void BuildArchitecture(BuildContext context, string architecture, string rid)
    {
        string buildWorkingDir = $"native/build_windows_{rid}";
        string outputDir = $"{context.ArtifactsDir}/{rid}";
        string directoryBuildPropsPath = $"{buildWorkingDir}/Directory.Build.props";

        context.CreateDirectory(buildWorkingDir);
        context.CreateDirectory(outputDir);
        context.FileWriteText(directoryBuildPropsPath, GetDirectoryBuildProps());

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = $"-A {architecture} -DMGSTB_BUILD_SHARED=ON .."
        });

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = "--build . --config Release"
        });

        context.CopyFile($"{buildWorkingDir}/Release/stb.dll", $"{outputDir}/stb.dll");
    }

    private static string GetDirectoryBuildProps()
    {
        return """
        <Project>
            <PropertyGroup>
                <BaseIntermediateOutputPath>$(MSBuildProjectDirectory)\obj\</BaseIntermediateOutputPath>
                <BaseOutputPath>$(MSBuildProjectDirectory)\bin\</BaseOutputPath>
                <OutputPath>$(BaseOutputPath)$(Configuration)\</OutputPath>
            </PropertyGroup>
        </Project>
        """;
    }
}
