﻿using System.Runtime.CompilerServices;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyDiffPlex.Initialize();
        VerifySourceGenerators.Initialize();

        VerifierSettings.ScrubLinesContaining("Version:", "SHA:", "GeneratedCodeAttribute");
    }
}
