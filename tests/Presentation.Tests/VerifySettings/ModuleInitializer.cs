using System.Runtime.CompilerServices;

namespace Presentation.Tests.VerifySettings;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.InitializePlugins();
        VerifierSettings.ScrubMember("RequestUri");
        VerifierSettings.ScrubMember("Date");
    }
}
