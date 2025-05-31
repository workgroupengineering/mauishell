namespace Shiny;


//https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation?view=net-maui-9.0
public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseShinyShell(this MauiAppBuilder builder, Action<ShinyAppBuilder> navBuilderAction)
    {
        var navBuilder = new ShinyAppBuilder();
        navBuilderAction.Invoke(navBuilder);
        navBuilder.RegisterDependencies(builder.Services);
        
        if (!builder.Services.Any(x => x.ImplementationType == typeof(ShinyShellNavigator)))
        {
            builder.Services.AddSingleton<ShinyShellNavigator>();
            builder.Services.AddSingleton<INavigator>(
                sp => sp.GetRequiredService<ShinyShellNavigator>()
            );
            builder.Services.AddSingleton<IMauiInitializeService>(
                sp => sp.GetRequiredService<ShinyShellNavigator>()
            );
            builder.Services.AddSingleton(navBuilder);
        }
        
        return builder;
    }
}