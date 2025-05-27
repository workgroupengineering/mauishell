using Shiny;

namespace Sample;

public partial class AnotherViewModel(
    INavigator navigator
) : ObservableObject, IQueryAttributable, IPageLifecycleAware, INavigationConfirmation, IDisposable
{
    [ObservableProperty] string arg;
    
    
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Console.WriteLine("AnotherViewModel ApplyQueryAttributes");
        if (query.TryGetValue("Arg", out var value))
            this.Arg = (string)value;
    }

    public void Dispose() => Console.WriteLine("AnotherViewModel Dispose");

    public void OnAppearing()
    {
        Console.WriteLine("AnotherViewModel OnAppearing");
    }

    public void OnDisappearing()
    {
        Console.WriteLine("AnotherViewModel OnDisappearing");
    }

    public Task<bool> CanNavigate() => navigator.Confirm(
        "Confirm", 
        "Are you sure you want to navigate?", 
        "Yes", 
        "No"
    );
}