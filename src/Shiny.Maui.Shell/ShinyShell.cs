namespace Shiny;

public class ShinyShell : Shell
{
    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        var page = this.CurrentPage;
        if (page == null)
            return;

        var services = this.Handler?.MauiContext?.Services
            ?? IPlatformApplication.Current?.Services;
        if (services == null)
            return;

        var navBuilder = services.GetService<ShinyAppBuilder>();
        var viewModelType = navBuilder?.GetViewModelTypeForPage(page);
        if (viewModelType == null)
            return;

        // BindingContext inherits down the visual tree, so it may be the Shell
        // instance rather than null — check if it's already the correct ViewModel
        if (page.BindingContext != null && viewModelType.IsInstanceOfType(page.BindingContext))
            return;

        page.BindingContext = services.GetService(viewModelType);
    }
}
