using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Spectre.Console;

namespace BuildScripts;

[TaskName("Build macOS")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildMacOSTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnMacOs();

    public override void Run(BuildContext context)
    {
        throw new NotImplementedException();
    }

    void BuildiOS(BuildContext context, string arch, string rid, bool simulator = false, string releaseDir = "")
    {
        throw new NotImplementedException();
    }

    void BuildAndroid(BuildContext context, string arch, string rid, string minNdk)
    {
        throw new NotImplementedException();
    }
}
