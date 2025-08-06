namespace Shiny;

public static class Registration
{
    public static MauiAppBuilder UseAutoHaptics(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<Impl.AutoHaptics>();
        builder.Services.AddSingleton<IAutoHaptics>(sp => sp.GetRequiredService<Impl.AutoHaptics>());
        builder.Services.AddSingleton<IMauiInitializeService>(sp => sp.GetRequiredService<Impl.AutoHaptics>());

        IAutoHaptics haptics = null!;
        haptics.Hook<Page>((page, call) => page.Appearing += (s, a) => call(), page => page.Appearing -= null);
        return builder;
    }
}