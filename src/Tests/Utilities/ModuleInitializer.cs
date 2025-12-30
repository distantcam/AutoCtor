using System.Runtime.CompilerServices;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyDiffPlex.Initialize();
        VerifySourceGenerators.Initialize();

        VerifierSettings.ScrubLinesContaining("Version:", "SHA:", "GeneratedCodeAttribute");
    }
}
