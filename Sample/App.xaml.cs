namespace Sample;

public partial class App : Application
{
    public App() => this.InitializeComponent();

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shellType = Preferences.Default.ContainsKey("ShellType")
            ? Enum.Parse<ShellType>(Preferences.Default.Get("ShellType", nameof(ShellType.Standard)))
            : ShellType.Standard;

        return new Window(CreateShell(shellType));
    }

    public static Shell CreateShell(ShellType shellType) => shellType switch
    {
        ShellType.Tabbed => new TabbedShell(),
        ShellType.Flyout => new FlyoutShell(),
        _ => new AppShell()
    };

    public static void SetShellPreference(ShellType shellType)
        => Preferences.Default.Set("ShellType", shellType.ToString());
}
