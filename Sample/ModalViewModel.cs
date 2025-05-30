using Microsoft.Extensions.Logging;
using Shiny;

namespace Sample;

public partial class ModalViewModel(
    ILogger<ModalViewModel> logger,
    INavigator navigator
) : ObservableObject, IPageLifecycleAware, INavigationAware, IDisposable
{
    [RelayCommand] Task Close() => navigator.GoBack();
    [RelayCommand] Task NavForward() => navigator.NavigateTo("another");
    public void OnNavigatingFrom(IDictionary<string, object> parameters) => logger.LogInformation("OnNavigatingFrom ModalViewModel");
    public void OnAppearing() => logger.LogInformation("OnAppearing ModalViewModel");
    public void OnDisappearing() => logger.LogInformation("OnDisappearing ModalViewModel");
    public void Dispose() => logger.LogInformation("Disposing ModalViewModel");
}