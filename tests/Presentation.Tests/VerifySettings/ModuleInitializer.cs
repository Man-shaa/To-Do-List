using System.Runtime.CompilerServices;

namespace Presentation.Tests.VerifySettings;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize();
        VerifierSettings.InitializePlugins();
        // VerifierSettings.ScrubMembers("Date", "RequestUri", "Id", "Url");
        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.UseStrictJson();
    }
}
