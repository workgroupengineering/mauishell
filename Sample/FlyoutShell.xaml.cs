namespace Sample;

public partial class FlyoutShell : Shell
{
    public FlyoutShell()
    {
        this.InitializeComponent();
    }

    void OnSwitchToStandard(object? sender, EventArgs e) => App.SetShell(ShellType.Standard);
    void OnSwitchToTabbed(object? sender, EventArgs e) => App.SetShell(ShellType.Tabbed);
}
