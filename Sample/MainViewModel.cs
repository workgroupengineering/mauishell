using Shiny;

namespace Sample;

public partial class MainViewModel(INavigator navigator) : ObservableObject
{
    [ObservableProperty] string navArg;
    [RelayCommand] Task NavByUri() => navigator.NavigateTo("another", ("Arg", this.NavArg));

    [RelayCommand]
    Task NavByViewModel() => navigator.NavigateTo<AnotherViewModel>(
        x => x.IsNavFromViewModel = true, 
        ("Arg", this.NavArg)
    );
}