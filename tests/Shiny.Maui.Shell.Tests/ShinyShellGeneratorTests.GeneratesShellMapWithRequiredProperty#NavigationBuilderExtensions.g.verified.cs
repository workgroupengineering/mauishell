//HintName: NavigationBuilderExtensions.g.cs
#nullable enable

internal static class __ShinyMauiNavigationRegistry
{
    [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
    public static void Initialize()
    {
        global::Shiny.Infrastructure.ShinyMauiShellRegistry.RegisterCallback(builder =>
        {
            builder.Add<global::TestApp.DetailsPage, global::TestApp.DetailsViewModel>(Routes.Details);
        });
    }
}
