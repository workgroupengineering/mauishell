namespace Shiny;

public class ShinyShell : Shell
{
    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        var page = this.CurrentPage;
        if (page == null || page.BindingContext != null)
            return;

        var services = this.Handler?.MauiContext?.Services
            ?? IPlatformApplication.Current?.Services;
        if (services == null)
            return;

        var navBuilder = services.GetService<ShinyAppBuilder>();
        var viewModelType = navBuilder?.GetViewModelTypeForPage(page);
        if (viewModelType == null)
            return;

        page.BindingContext = services.GetService(viewModelType);
    }
}
