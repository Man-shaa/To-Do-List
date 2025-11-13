using System.Runtime.CompilerServices;

namespace Application.Tests.VerifySettings;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize();
        VerifierSettings.InitializePlugins();
        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.UseStrictJson();
    }
}
