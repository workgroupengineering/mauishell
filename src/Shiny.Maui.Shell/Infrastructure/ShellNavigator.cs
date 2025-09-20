using Microsoft.Extensions.Logging;

namespace Shiny.Infrastructure;


// Absolute routes don't work with pages that are registered with the Routing.RegisterRoute method.
// TODO: //PageName and ../../PageName or ../Page1/Page2
// https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation?view=net-maui-9.0
// TODO: replace route, backCount = 2, relative
public class ShinyShellNavigator(
    ILogger<ShinyShellNavigator> logger,
    IApplication application,
    IServiceProvider services,
    IDispatcher dispatcher,
    ShinyAppBuilder navBuilder
) : INavigator, IMauiInitializeService, IDisposable
{
    public void Initialize(IServiceProvider _)
    {
        if (application is not Application app)
            throw new InvalidOperationException($"Invalid MAUI Application - {application.GetType()}");

        app.DescendantAdded += this.AppOnDescendantAdded;
        app.DescendantRemoved += this.AppOnDescendantRemoved;
        app.PageAppearing += this.AppOnPageAppearing;
        app.PageDisappearing += this.AppOnPageDisappearing;
    }
    
    
    public void Dispose()
    {
        if (application is Application app)
        {
            app.DescendantAdded -= this.AppOnDescendantAdded;
            app.DescendantRemoved -= this.AppOnDescendantRemoved;
            app.PageAppearing -= this.AppOnPageAppearing;
            app.PageDisappearing -= this.AppOnPageDisappearing;
        }
    }

    
    public Task NavigateTo(string uri, params IEnumerable<(string Key, object Value)> args) =>
        dispatcher.DispatchAsync(() =>
        {
            var shell = Shell.Current;
            var parameters = args.ToDictionary(x => x.Key, x => x.Value);

            if (shell.CurrentPage?.BindingContext is INavigationAware navAware)
                navAware.OnNavigatingFrom(parameters);

            return shell.GoToAsync(uri, true, parameters);
        });


    public async Task NavigateTo<TViewModel>(
        Action<TViewModel>? configure = null,
        params IEnumerable<(string Key, object Value)> args
    )
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
                tcs.TrySetException(
                    new InvalidOperationException($"Page BindingContext is not of type '{typeof(TViewModel)}'"));
        });

        try
        {
            var parameters = args.ToDictionary(x => x.Key, x => x.Value);
            if (Shell.Current.CurrentPage?.BindingContext is INavigationAware navAware)
                navAware.OnNavigatingFrom(parameters);

            ShinyRouteFactory.PageResolved += handler;
            await dispatcher.DispatchAsync(() => Shell.Current.GoToAsync(route, true, parameters));
            await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            ShinyRouteFactory.PageResolved -= handler;
        }
    }

    public Task PopToRoot(params IEnumerable<(string Key, object Value)> args) 
    {
        var uri = "..";
        
        // we already have 1 page covered and we don't want to pop the last page
        var count = Shell.Current.Navigation.NavigationStack.Count - 2;
        for (var i = 0; i < count; i++)
            uri += "/..";
        
        var parameters = args.ToDictionary(x => x.Key, x => x.Value);
        return dispatcher.DispatchAsync(() => Shell.Current.GoToAsync(uri, true, parameters));
    }

    
    public Task GoBack(params IEnumerable<(string Key, object Value)> args) => dispatcher.DispatchAsync(() =>
    {
        var shell = Shell.Current;
        var parameters = args.ToDictionary(x => x.Key, x => x.Value);
        if (shell.CurrentPage?.BindingContext is INavigationAware navAware)
            navAware.OnNavigatingFrom(parameters);
        
        return shell.GoToAsync("..", true, parameters);
    });


    public async Task Alert(string? title, string message, string acceptText = "OK")
    {
        var tcs = new TaskCompletionSource();
        await dispatcher.DispatchAsync(async () =>
        {
            await Shell.Current.DisplayAlert(title, message, acceptText);
            tcs.SetResult();
        });
        await tcs.Task;
    }
    

    public async Task<bool> Confirm(string? title, string message, string acceptText = "Yes", string cancelText = "No")
    {
        var tcs = new TaskCompletionSource<bool>();
        await dispatcher.DispatchAsync(async () =>
        {
            var result = await Shell.Current.DisplayAlert(title, message, acceptText, cancelText);
            tcs.SetResult(result);
        });
        return await tcs.Task.ConfigureAwait(false);
    }
    
    
    void AppOnDescendantAdded(object? sender, ElementEventArgs args)
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
    }
    
    
    void AppOnDescendantRemoved(object? sender, ElementEventArgs args)
    {
        if (args.Element is Page { BindingContext: IDisposable disposable })
        {
            logger.LogDebug("[Dispose] ViewModel '{type}'", disposable.GetType());
            disposable.Dispose();
        }
    }

    
    void AppOnPageAppearing(object? sender, Page page)
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
    }
    
    
    void AppOnPageDisappearing(object? sender, Page page)
    {
        if (page.BindingContext is IPageLifecycleAware lc)
        {
            logger.LogDebug("[OnAppearing] ViewModel '{type}' ", lc.GetType());
            lc.OnDisappearing();
        }
    }
}