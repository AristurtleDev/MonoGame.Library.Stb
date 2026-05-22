using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BuildScripts;

[TaskName("Build macOS")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildMacOSTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnMacOs();

    public override void Run(BuildContext context)
    {
        string buildWorkingDir = "native/build_osx";

        context.CreateDirectory(buildWorkingDir);
        context.CreateDirectory($"{context.ArtifactsDir}/osx/");

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = "-G Ninja -DCMAKE_BUILD_TYPE=Release -DCMAKE_OSX_DEPLOYMENT_TARGET=10.15 -DCMAKE_OSX_ARCHITECTURES=\"x86_64;arm64\" -DMGSTB_BUILD_SHARED=ON .."
        });

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = "--build ."
        });

        context.CopyFile($"{buildWorkingDir}/libstb.dylib", $"{context.ArtifactsDir}/osx/libstb.dylib");

        BuildiOS(context, "arm64", "ios-arm64", false, "Release-iphoneos");
        BuildiOS(context, "x86_64", "iossimulator-x64", true, "Release-iphonesimulator");
        BuildiOS(context, "arm64", "iossimulator-arm64", true, "Release-iphonesimulator");
    }

    private static void BuildiOS(BuildContext context, string arch, string rid, bool simulator = false, string releaseDir = "")
    {
        string buildWorkingDir = $"native/build_{rid}";
        string sdk = string.Empty;

        context.CreateDirectory(buildWorkingDir);
        context.CreateDirectory($"{context.ArtifactsDir}/{rid}/");

        if (simulator)
        {
            IEnumerable<string> output;
            context.StartProcess("xcodebuild", new ProcessSettings
            {
                WorkingDirectory = buildWorkingDir,
                RedirectStandardOutput = true,
                Arguments = "-version -sdk iphonesimulator Path"
            }, out output);

            sdk = $" -DCMAKE_OSX_SYSROOT={output.First()}";
        }

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = $"-GXcode -DCMAKE_SYSTEM_NAME=iOS -DCMAKE_OSX_ARCHITECTURES=\"{arch}\" -DCMAKE_BUILD_TYPE=Release -DMGSTB_BUILD_SHARED=OFF{sdk} .."
        });

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = "--build . --config Release"
        });

        string[] staticLibraryFiles = Directory.GetFiles(System.IO.Path.Combine(buildWorkingDir, releaseDir), "libstb.a", SearchOption.TopDirectoryOnly);
        if (staticLibraryFiles.Length > 0)
        {
            context.CopyFile(staticLibraryFiles[0], $"{context.ArtifactsDir}/{rid}/libstb.a");
        }
    }
}
