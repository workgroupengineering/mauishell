using Shiny;

namespace Sample;

public partial class MainViewModel(INavigator navigator) : ObservableObject
{
    [ObservableProperty] string navArg;
    // [RelayCommand] Task NavByViewModel
    [RelayCommand] Task NavByUri() => navigator.NavigateTo("another", ("Arg", this.NavArg));
}