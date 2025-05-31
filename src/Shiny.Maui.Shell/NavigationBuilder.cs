using System.Diagnostics.CodeAnalysis;

namespace Shiny;


public sealed class ShinyNavigationBuilder
{
    readonly Dictionary<string, (bool RegisterRoute, Type PageType, Type ViewModelType)> typeMap = new();
    
    public ShinyNavigationBuilder Add<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPage, 
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel
    >(string? route = null, bool registerRoute = true)
        where TPage : Page
        where TViewModel : class, INotifyPropertyChanged
    {
        route ??= typeof(TPage).Name;
        this.typeMap[route] = (registerRoute, typeof(TPage), typeof(TViewModel));
        return this;
    }
    

    public Type? GetViewModelTypeForPage(Page page)
    {
        var pageType = page.GetType();
        foreach (var pair in this.typeMap)
        {
            if (pair.Value.PageType == pageType) 
                return pair.Value.ViewModelType;
        }
        return null;
    }


    public string? GetRouteForViewModel(Type viewModelType)
    {
        foreach (var pair in this.typeMap)
        {
            if (pair.Value.ViewModelType == viewModelType)
                return pair.Key;
        }

        return null;
    }
    
    
    internal void RegisterDependencies(IServiceCollection services)
    {
        foreach (var pair in this.typeMap)
        {
            services.AddTransient(pair.Value.PageType);
            services.AddTransient(pair.Value.ViewModelType);
            if (pair.Value.RegisterRoute)
            {
                Routing.RegisterRoute(
                    pair.Key,
                    new ShinyRouteFactory(
                        pair.Value.PageType,
                        pair.Value.ViewModelType
                    )
                );
            }
        }
    }
}