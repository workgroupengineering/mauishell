using Shiny;

namespace Sample;

public partial class FlyoutShell : ShinyShell
{
    public FlyoutShell()
    {
        this.InitializeComponent();
    }

    async void OnSwitchToStandard(object? sender, EventArgs e)
    {
        var navigator = Handler!.MauiContext!.Services.GetRequiredService<INavigator>();
        App.SetShellPreference(ShellType.Standard);
        await navigator.SwitchShell(App.CreateShell(ShellType.Standard));
    }

    async void OnSwitchToTabbed(object? sender, EventArgs e)
    {
        var navigator = Handler!.MauiContext!.Services.GetRequiredService<INavigator>();
        App.SetShellPreference(ShellType.Tabbed);
        await navigator.SwitchShell(App.CreateShell(ShellType.Tabbed));
    }
}
