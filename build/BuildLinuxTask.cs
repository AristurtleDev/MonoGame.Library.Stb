using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BuildScripts;

[TaskName("Build Linux")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildLinuxTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnLinux();

    public override void Run(BuildContext context)
    {
        Architecture architecture = RuntimeInformation.ProcessArchitecture;
        string arch = architecture == Architecture.Arm64 ? "arm64" : "x64";
        string buildWorkingDir = "native/build_linux";

        context.CreateDirectory(buildWorkingDir);
        context.CreateDirectory($"{context.ArtifactsDir}/linux-{arch}/");

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = "-G Ninja -DCMAKE_BUILD_TYPE=Release -DMGSTB_BUILD_SHARED=ON .."
        });

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = "--build ."
        });

        context.StartProcess("strip", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = "--strip-all libstb.so"
        });

        context.CopyFile($"{buildWorkingDir}/libstb.so", $"{context.ArtifactsDir}/linux-{arch}/libstb.so");

        BuildAndroid(context, "arm64-v8a", "android-arm64", "23");
        BuildAndroid(context, "armeabi-v7a", "android-arm", "23");
        BuildAndroid(context, "x86", "android-x86", "23");
        BuildAndroid(context, "x86_64", "android-x64", "23");
    }

    private static void BuildAndroid(BuildContext context, string arch, string rid, string minNdk)
    {
        string? ndk = Environment.GetEnvironmentVariable("ANDROID_NDK_HOME");
        if (string.IsNullOrEmpty(ndk))
        {
            return;
        }

        string strip = System.IO.Path.Combine(ndk, "toolchains", "llvm", "prebuilt", "linux-x86_64", "bin", "llvm-strip");
        string buildWorkingDir = $"native/build_android_{arch}";

        context.CreateDirectory(buildWorkingDir);
        context.CreateDirectory($"{context.ArtifactsDir}/{rid}");

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = $"-G Ninja -DANDROID_ABI={arch} -DANDROID_PLATFORM={minNdk} -DCMAKE_TOOLCHAIN_FILE={ndk}/build/cmake/android.toolchain.cmake -DANDROID_NDK={ndk} -DCMAKE_BUILD_TYPE=Release -DMGSTB_BUILD_SHARED=ON .."
        });

        context.StartProcess("cmake", new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = "--build ."
        });

        context.StartProcess(strip, new ProcessSettings
        {
            WorkingDirectory = buildWorkingDir,
            Arguments = "--strip-all libstb.so"
        });

        context.CopyFile($"{buildWorkingDir}/libstb.so", $"{context.ArtifactsDir}/{rid}/libstb.so");
    }
}
