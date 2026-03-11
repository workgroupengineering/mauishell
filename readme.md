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
- **Dialog Service** (`IDialogs`)
  - Alert, Confirm, Prompt, and ActionSheet dialogs
  - Thread-safe — dispatches to the UI thread automatically
  - Inject separately from `INavigator` for clean separation of concerns
- **Navigation Events**
  - `Navigating` event with source ViewModel instance (pre-navigation)
  - `Navigated` event with destination ViewModel instance (post-navigation)
- **ViewModel Lifecycle**
  - OnAppearing / OnDisappearing
  - Navigation confirmation guards
  - OnNavigatingFrom parameter mutation
  - Automatic disposal when removed from the stack
- **Source Generation**
  - Static route constants (route name drives the constant name)
  - Strongly-typed navigation extension methods
  - DI registration via `AddGeneratedMaps()`
  - Configurable — disable route constants or nav extensions via MSBuild properties
  - Invalid route names produce compiler errors (SHINY001)

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
        .Add<DetailPage, DetailViewModel>("Detail")
        .Add<SettingsPage, SettingsViewModel>("Settings")
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
    await navigator.NavigateTo("Detail", ("ItemId", "123"));

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
}
```

> [!NOTE]
> If you're setting arguments on the ViewModel navigation, you should make them observable if they are bound on the Page.

### 4. Dialogs

Inject `IDialogs` for user-facing dialogs:

```csharp
public class MyViewModel(IDialogs dialogs)
{
    // Alert
    await dialogs.Alert("Error", "Something went wrong");

    // Confirm
    if (await dialogs.Confirm("Delete?", "Are you sure?"))
    {
        // delete
    }

    // Prompt for text input
    var name = await dialogs.Prompt("Name", "Enter your name", placeholder: "John Doe");
    if (name != null)
    {
        // user entered a value
    }

    // Action sheet
    var choice = await dialogs.ActionSheet("Options", "Cancel", "Delete", "Edit", "Share");
}
```

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
[ShellMap<DetailPage>("Detail")]
public partial class DetailViewModel(INavigator navigator, IDialogs dialogs) : ObservableObject,
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
        return await dialogs.Confirm("Unsaved Changes", "Discard changes?");
    }

    public void Dispose() { /* cleanup */ }
}
```

---

## Source Generation

Decorate your ViewModels with `[ShellMap]` and `[ShellProperty]` to eliminate boilerplate:

**Input:**
```csharp
[ShellMap<DetailPage>("Detail")]
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
// Routes.g.cs — constant name matches the route parameter
public static class Routes
{
    public const string Detail = "Detail";
}

// NavigationExtensions.g.cs — method name matches the route parameter
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

// NavigationBuilderExtensions.g.cs — uses string literals (not Routes.*)
public static class NavigationBuilderExtensions
{
    public static ShinyAppBuilder AddGeneratedMaps(this ShinyAppBuilder builder)
    {
        builder.Add<DetailPage, DetailViewModel>("Detail");
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

### Route Naming

The `route` parameter in `[ShellMap]` drives the generated constant and method names. It must be a valid C# identifier — invalid names produce a **SHINY001** compiler error.

```csharp
// Route drives the constant and method name
[ShellMap<HomePage>("Dashboard")]
// → Routes.Dashboard = "Dashboard"
// → NavigateToDashboard(...)

// No route — falls back to page type name without "Page" suffix
[ShellMap<HomePage>]
// → Routes.Home = "HomePage"
// → NavigateToHome(...)
```

### Configuring Source Generation

Disable individual generated files via MSBuild properties:

```xml
<PropertyGroup>
    <!-- Disable Routes.g.cs -->
    <ShinyMauiShell_GenerateRouteConstants>false</ShinyMauiShell_GenerateRouteConstants>

    <!-- Disable NavigationExtensions.g.cs -->
    <ShinyMauiShell_GenerateNavExtensions>false</ShinyMauiShell_GenerateNavExtensions>
</PropertyGroup>
```

`NavigationBuilderExtensions.g.cs` (`AddGeneratedMaps()`) is always generated — even when no `[ShellMap]` attributes exist yet — so you can wire up `MauiProgram.cs` immediately.
