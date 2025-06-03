using System.Runtime.CompilerServices;

namespace Shiny.Maui.Shell.Tests;


public class VerifyInitializer
{
    [ModuleInitializer]
    public static void Init() =>
        VerifySourceGenerators.Initialize();
}