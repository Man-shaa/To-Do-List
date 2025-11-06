using System.Runtime.CompilerServices;

namespace Presentation.Tests.VerifySettings;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize();
        VerifierSettings.InitializePlugins();
        VerifierSettings.ScrubMembers("Date", "RequestUri");
        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.UseStrictJson();
    }
}
