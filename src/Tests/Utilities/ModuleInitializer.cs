using System.Runtime.CompilerServices;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();

        VerifierSettings.ScrubLinesContaining("Version:", "SHA:", "GeneratedCodeAttribute");
    }
}
