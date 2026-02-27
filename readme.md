# Shiny MAUI Shell

[![NuGet](https://img.shields.io/nuget/v/Shiny.Maui.Shell?style=for-the-badge)](https://www.nuget.org/packages/Shiny.Maui.Shell)

Make .NET MAUI Shell shinier with ViewModel lifecycle management, navigation services, and source generation to remove boilerplate, reduce errors, and make your app testable.

Inspired by [Prism Library](https://prismlibrary.com) by Dan Siegel and Brian Lagunas.

[Full Documentation](https://shinylib.net/maui)

---

## Features

- **Page-ViewModel Registration** - Easy mapping with automatic BindingContext assignment
- **No special AppShell subclass** - Works with your existing AppShell.xaml
- **Testable Navigation Service** (`INavigator`)
  - Route-based and ViewModel-based navigation
  - Strongly-typed ViewModel configuration
  - GoBack, PopToRoot, SetRoot
  - Modal and tab support
  - Alert and Confirm dialogs
- **Navigation Events**
  - `Navigating` event with source ViewModel instance (pre-navigation)
  - `Navigated` event with destination ViewModel instance (post-navigation)
- **ViewModel Lifecycle**
  - OnAppearing / OnDisappearing
  - Navigation confirmation guards
  - OnNavigatingFrom parameter mutation
  - Automatic disposal when removed from the stack
- **Source Generation**
  - Static route constants
  - Strongly-typed navigation extension methods
  - DI registration via `AddGeneratedMaps()`

---

## Getting Started

### 1. Install

```bash
dotnet add package Shiny.Maui.Shell
```

### 2. Configure MauiProgram.cs

**With source generation (recommended):**
```csharp
builder
    .UseMauiApp<App>()
    .UseShinyShell(x => x.AddGeneratedMaps());
```

**Manual registration:**
```csharp
builder
    .UseMauiApp<App>()
    .UseShinyShell(x => x
        .Add<MainPage, MainViewModel>(registerRoute: false) // pages in AppShell.xaml
        .Add<DetailPage, DetailViewModel>("detail")
        .Add<SettingsPage, SettingsViewModel>("settings")
    );
```

> [!NOTE]
> The default MAUI AppShell.xaml does not need any modification to work with this library. Pages defined in AppShell.xaml should use `registerRoute: false`.

### 3. Navigate

Inject `INavigator` into your ViewModels:

```csharp
public class MyViewModel(INavigator navigator)
{
    // Route-based navigation with args
    await navigator.NavigateTo("detail", ("ItemId", "123"));

    // ViewModel-based navigation with strongly-typed configuration
    await navigator.NavigateTo<DetailViewModel>(vm => vm.ItemId = "123");

    // Source-generated strongly-typed method (preferred)
    await navigator.NavigateToDetail("123");

    // Go back with result
    await navigator.GoBack(("Result", selectedItem));

    // Go back multiple pages
    await navigator.GoBack(2);

    // Pop to root
    await navigator.PopToRoot();

    // Replace root page
    await navigator.SetRoot<DashboardViewModel>();

    // Dialogs
    await navigator.Alert("Error", "Something went wrong");
    if (await navigator.Confirm("Delete?", "Are you sure?"))
    {
        // delete
    }
}
```

> [!NOTE]
> If you're setting arguments on the ViewModel navigation, you should make them observable if they are bound on the Page.

---

## Navigation Events

`INavigator` exposes two events for observing navigation:

| Event | When | Key Data |
|-------|------|----------|
| `Navigating` | Before navigation | `FromUri`, `FromViewModel`, `ToUri`, `NavigationType`, `Parameters` |
| `Navigated` | After page resolves | `ToUri`, `ToViewModel`, `NavigationType`, `Parameters` |

```csharp
navigator.Navigating += (sender, args) =>
{
    Console.WriteLine($"Leaving {args.FromViewModel?.GetType().Name} -> {args.ToUri}");
};

navigator.Navigated += (sender, args) =>
{
    Console.WriteLine($"Arrived at {args.ToViewModel?.GetType().Name}");
};
```

Hook these in an `IMauiInitializeService` for cross-cutting concerns like logging or analytics:

```csharp
public class NavigationLogger(
    ILogger<NavigationLogger> logger,
    INavigator navigator
) : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        navigator.Navigating += (_, args) =>
            logger.LogInformation("Navigating from '{From}' to '{To}' ({Type})",
                args.FromUri, args.ToUri, args.NavigationType);

        navigator.Navigated += (_, args) =>
            logger.LogInformation("Navigated to '{To}' - ViewModel: {VM} ({Type})",
                args.ToUri, args.ToViewModel?.GetType().Name, args.NavigationType);
    }
}

// Register in MauiProgram.cs
builder.Services.AddSingleton<IMauiInitializeService, NavigationLogger>();
```

---

## ViewModel Lifecycle

Implement these interfaces on your ViewModels as needed. Works just like [Prism Library](https://prismlibrary.com).

| Interface | Method | Purpose |
|-----------|--------|---------|
| `IPageLifecycleAware` | `OnAppearing()` / `OnDisappearing()` | Page visibility hooks |
| `INavigationConfirmation` | `Task<bool> CanNavigate()` | Block navigation (e.g., unsaved changes) |
| `INavigationAware` | `OnNavigatingFrom(IDictionary<string, object>)` | Mutate parameters before leaving |
| `IQueryAttributable` | `ApplyQueryAttributes(IDictionary<string, object>)` | Receive navigation parameters |
| `IDisposable` | `Dispose()` | Cleanup when page is removed from the stack |

```csharp
[ShellMap<DetailPage>("detail")]
public partial class DetailViewModel(INavigator navigator) : ObservableObject,
    IQueryAttributable,
    IPageLifecycleAware,
    INavigationConfirmation,
    IDisposable
{
    [ShellProperty]
    [ObservableProperty]
    string itemId;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(ItemId), out var id))
            ItemId = id?.ToString();
    }

    public void OnAppearing() { /* load data */ }
    public void OnDisappearing() { /* pause */ }

    public async Task<bool> CanNavigate()
    {
        if (!hasUnsavedChanges) return true;
        return await navigator.Confirm("Unsaved Changes", "Discard changes?");
    }

    public void Dispose() { /* cleanup */ }
}
```

---

## Source Generation

Decorate your ViewModels with `[ShellMap]` and `[ShellProperty]` to eliminate boilerplate:

**Input:**
```csharp
[ShellMap<DetailPage>("detail")]
public partial class DetailViewModel : ObservableObject
{
    [ShellProperty]
    public string ItemId { get; set; }

    [ShellProperty(required: false)]
    public int Page { get; set; }
}
```

**Generated output:**

```csharp
// Routes.g.cs
public static class Routes
{
    public const string Detail = "detail";
}

// NavigationExtensions.g.cs
public static class NavigationExtensions
{
    public static Task NavigateToDetail(this INavigator navigator, string itemId, int page = default)
    {
        return navigator.NavigateTo<DetailViewModel>(x =>
        {
            x.ItemId = itemId;
            x.Page = page;
        });
    }
}

// NavigationBuilderExtensions.g.cs
public static class NavigationBuilderExtensions
{
    public static ShinyAppBuilder AddGeneratedMaps(this ShinyAppBuilder builder)
    {
        builder.Add<DetailPage, DetailViewModel>(Routes.Detail);
        return builder;
    }
}
```

Then use it:
```csharp
// MauiProgram.cs - one line to register everything
builder.UseShinyShell(x => x.AddGeneratedMaps());

// Navigate with generated extension methods - no guesswork
await navigator.NavigateToDetail("123", page: 2);
```
