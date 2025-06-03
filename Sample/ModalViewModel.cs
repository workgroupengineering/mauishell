using Microsoft.Extensions.Logging;
using Shiny;

namespace Sample;

[ShellMap<ModalPage>("modal")]
public partial class ModalViewModel(
    ILogger<ModalViewModel> logger,
    INavigator navigator
) : ObservableObject, IPageLifecycleAware, INavigationAware, IDisposable
{
    [ShellProperty(false)]
    public int Arg2 { get; set; }
    
    [ShellProperty(true)]
    public string Arg1 { get; set; }
    
    [RelayCommand] Task Close() => navigator.GoBack();
    [RelayCommand] Task NavForward() => navigator.NavigateTo("another");
    public void OnNavigatingFrom(IDictionary<string, object> parameters) => logger.LogInformation("OnNavigatingFrom ModalViewModel");
    public void OnAppearing() => logger.LogInformation("OnAppearing ModalViewModel");
    public void OnDisappearing() => logger.LogInformation("OnDisappearing ModalViewModel");
    public void Dispose() => logger.LogInformation("Disposing ModalViewModel");
}