using Microsoft.Extensions.Logging;

namespace Shiny.Infrastructure;


// https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation?view=net-maui-9.0
public class ShinyShellNavigator(
    ILogger<ShinyShellNavigator> logger,
    IApplication application,
    IServiceProvider services,
    ShinyNavigationBuilder navBuilder
) : INavigator, IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        if (application is not Application app)
            throw new InvalidOperationException($"Invalid MAUI Application - {application.GetType()}");

        // TODO: don't use anonymous events when I release
        app.DescendantAdded += (_, args) =>
        {
            if (args.Element is Shell shell)
            {
                shell.Navigating += async (_, shellArgs) =>
                {
                    var vm = shell.CurrentPage?.BindingContext;
                    
                    if (vm is INavigationConfirmation confirm)
                    {
                        var deferral = shellArgs.GetDeferral();
                        var canNav = await confirm.CanNavigate();
                        if (!canNav)
                            shellArgs.Cancel();

                        deferral.Complete();
                    }
                };
            }
        };
        
        app.DescendantRemoved += (_, args) =>
        {
            if (args.Element is Page { BindingContext: IDisposable disposable })
            {
                logger.LogDebug("[Dispose] ViewModel '{type}'", disposable.GetType());
                disposable.Dispose();
            }
        };
        
        app.PageAppearing += (_, page) =>
        {
            if (page.BindingContext == null)
            {
                // needed for initial pags - IQueryAttributable would be missed
                var viewModelType = navBuilder.GetViewModelTypeForPage(page);
                if (viewModelType == null)
                {
                    logger.LogDebug("No ViewModel found for page");
                }
                else
                {
                    var vm = services.GetService(viewModelType);
                    page.BindingContext = vm;
                    logger.LogDebug("[Binding] ViewModel {type} set on page", viewModelType);
                }
            }
            
            if (page.BindingContext is IPageLifecycleAware lc)
            {
                logger.LogDebug("[OnAppearing] ViewModel '{type}' ", lc.GetType());
                lc.OnAppearing();
            }
        };

        app.PageDisappearing += (_, page) =>
        {
            if (page.BindingContext is IPageLifecycleAware lc)
            {
                logger.LogDebug("[OnAppearing] ViewModel '{type}' ", lc.GetType());
                lc.OnDisappearing();
            }
        };
    }
    
    
    public async Task NavigateTo(string uri, params IEnumerable<(string Key, object Value)> args)
    {
        var parameters = args.ToDictionary(x => x.Key, x => x.Value);
        if (Shell.Current.CurrentPage?.BindingContext is INavigationAware navAware)
            navAware.OnNavigatingFrom(parameters);

        // force main thread?
        await Shell.Current.GoToAsync(uri, true, parameters);
    }
    

    public async Task NavigateTo<TViewModel>(Action<TViewModel>? configure = null, params IEnumerable<(string Key, object Value)> args)
    {
        var route = navBuilder.GetRouteForViewModel(typeof(TViewModel));
        if (route == null)
            throw new InvalidOperationException($"Could not find a route for viewmodel '{typeof(TViewModel)}'");

        var tcs = new TaskCompletionSource();
        var handler = new EventHandler<Page>((_, page) =>
        {
            if (page.BindingContext is TViewModel vm)
            {
                logger.LogDebug("Pre-Configuring ViewModel");
                configure?.Invoke(vm);
                tcs.TrySetResult();
            }
            else
                tcs.TrySetException(new InvalidOperationException($"Page BindingContext is not of type '{typeof(TViewModel)}'"));
        });
        
        try
        {
            var parameters = args.ToDictionary(x => x.Key, x => x.Value);
            if (Shell.Current.CurrentPage?.BindingContext is INavigationAware navAware)
                navAware.OnNavigatingFrom(parameters);

            ShinyRouteFactory.PageResolved += handler;

            
            // force main thread?
            await Shell.Current.GoToAsync(route, true, parameters);
            await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            ShinyRouteFactory.PageResolved -= handler;
        }
    }


    public Task GoBack(params IEnumerable<(string Key, object Value)> args) => MainThread.InvokeOnMainThreadAsync(async () =>
    {
        // TODO: force main thread?
        var parameters = args.ToDictionary(x => x.Key, x => x.Value);
        if (Shell.Current.CurrentPage?.BindingContext is INavigationAware navAware)
            navAware.OnNavigatingFrom(parameters);
        
        await Shell.Current.GoToAsync("..", true, parameters);
    });


    public async Task Alert(string title, string message, string acceptText)
    {
        var tcs = new TaskCompletionSource();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.DisplayAlert(title, message, acceptText);
            tcs.SetResult();
        });
        await tcs.Task;
    }
    

    public async Task<bool> Confirm(string title, string message, string acceptText, string cancelText)
    {
        var tcs = new TaskCompletionSource<bool>();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var result = await Shell.Current.DisplayAlert(title, message, acceptText, cancelText);
            tcs.SetResult(result);
        });
        return await tcs.Task.ConfigureAwait(false);
    }
}