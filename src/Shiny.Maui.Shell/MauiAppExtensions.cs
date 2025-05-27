using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shiny;


//https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation?view=net-maui-9.0
public static class NavigationExtensions
{
    public static MauiAppBuilder UseShinyNavigation(this MauiAppBuilder builder, Action<ShinyNavigationBuilder> navBuilderAction)
    {
        var navBuilder = new ShinyNavigationBuilder();
        navBuilderAction.Invoke(navBuilder);
        navBuilder.RegisterDependencies(builder.Services);
        
        // builder.Services.AddSingletonAsImplementedInterfaces<ShinyShellNavigator>();
        builder.Services.TryAddSingleton(navBuilder);
        return builder;
    }
}