using System.Runtime.CompilerServices;

namespace Infrastructure.Tests.Repositories.VerifySettings;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize();
        VerifierSettings.InitializePlugins();
        VerifierSettings.UseStrictJson();
    }
}
