using Microsoft.Extensions.Logging;
using Shiny;

namespace Sample;


public partial class AnotherViewModel(
    ILogger<AnotherViewModel> logger,
    INavigator navigator
) : ObservableObject, IQueryAttributable, IPageLifecycleAware, INavigationConfirmation, INavigationAware, IDisposable
{
    [ObservableProperty] string backArg;
    [ObservableProperty] string arg;
    [ObservableProperty] bool isNavFromViewModel;
    
    [RelayCommand] Task GoBack() => navigator.GoBack(("ToTheBack", this.BackArg));
    
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        logger.LogInformation("AnotherViewModel ApplyQueryAttributes");
        if (query.TryGetValue("Arg", out var value))
            this.Arg = (string)value;
    }

    public void Dispose() => logger.LogInformation("AnotherViewModel Dispose");
    public void OnAppearing() => logger.LogInformation("AnotherViewModel OnAppearing");
    public void OnDisappearing() => logger.LogInformation("AnotherViewModel OnDisappearing");

    public Task<bool> CanNavigate() => navigator.Confirm(
        "Confirm", 
        "Are you sure you want to navigate?", 
        "Yes", 
        "No"
    );
    
    public void OnNavigatingFrom(IDictionary<string, object> parameters)
    {
        if (parameters["ToTheBack"] == null)
        {
            parameters["ToTheBack"] = "SET BY OnNavigatingFrom"; // mutate/change parameters before actually leaving
        }
        logger.LogInformation("OnNavigatingFrom AnotherViewModel");
    }
}