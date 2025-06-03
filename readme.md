# Shiny MAUI

Work In Progress

## Shell

Inspired entirely by Prism - there is nothing here that is an original idea.  I wanted to build this myself and
build it around Shell so I could understand the inner workings of Shell.

### Features/Roadmap
* [x] Registration
* [x] ~~ServiceScope per Page~~
* [ ] Shell XAML Integration(?)
* [ ] Navigation Service
  * [ ] Events…?
  * [ ] To Commands
  * [x] NavigateTo(string uri, args)
  * [x] NavigateTo<TViewModel>
    * [x] With Strongly Typed Init
      * [ ] Should be async??
  * [x] GoBack(args)
  * [ ] Pop To Root
  * [ ] Set Root
  * [ ] Modals/Tabs
* [x] Auto ViewModel Push on to page
* [x] Source Generation
  * [x] Static Routes Class
  * [x] Navigator extension methods for strongly typed navigation
  * [x] Dependency injection source generated method
* [ ] ViewModel lifecycle
  * [x] Strongly Typed Navigation Args (when navigating by viewmodel - Take a look at [Shiny Mediator](https://shinylib.net/mediator) shell for this
  * [x] OnAppearing/OnDisappearing
  * [x] Navigation Confirmation
  * [x] Disposable/Destroy
  * [ ] OnNavigatedTo(args) and direction of navigation(?)
  * [x] OnNavigatedFrom()
    * [ ] Direction pop, uri from where?

### Setup
1. Install Nuget [![nuget](https://img.shields.io/nuget/v/Shiny.Maui.Shell?style=for-the-badge)](https://www.nuget.org/packages/Shiny.Maui.Shell)
2. In your MauiProgram.cs, add the following
```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseShinyShell(x => x
                .Add<MainPage, MainViewModel>(registerRoute: false)
                .Add<AnotherPage, AnotherViewModel>("another")
            )
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```
3. Now you can inject `Shiny.INavigator` into your VIewModels and navigate away

### Navigation
Shiny.INavigator - TODO
* NavigateTo(string routeName)
* NavigateTo<TViewModel>(Action<TVIewModel>)
* GoBack(dictionary)

### ViewModel Lifecycle
TODO

* IDisposable - to permanently destroy any hooks
* IPageLifecycleAware
* INavigationConfirmation
* Receive arguments on your ViewModel by implementing - IQueryAttributable

### Source Generation

THIS
```csharp
// INPUT
[ShellMap<MyPage>("MyRoute")]
public class MyViewModel 
{
    [ShellProperty]
    public string Arg { get; set; }
}
```

GENERATES ALL OF THIS FOR YOU TO PLUGIN SEAMLESSLY WITH YOUR CODE
* Strong Typed Routes with arguments
* Easy Dependency Injection Registration of Routes

```csharp
public static class NavigationBuilderExtensions
{
    public static global::Shiny.ShinyAppBuilder AddGeneratedMaps(this global::Shiny.ShinyAppBuilder builder)
    {
        builder.Add<Sample.MyPage, Sample.MyViewModel>(Routes.My);
        return builder;
    }
}


public static class NavigationExtensions
{
    public static global::System.Threading.Tasks.Task NavigateToModal(this global::Shiny.INavigator navigator, string arg1)
    {
        return navigator.NavigateTo<Sample.ModalViewModel>(x => { x.Arg = arg; });
    }
}

public static class Routes
{
    public const string My = "MyPage";
}

```