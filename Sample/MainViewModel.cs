using Shiny;

namespace Sample;

public partial class MainViewModel(INavigator navigator) : ObservableObject, IQueryAttributable
{
    [ObservableProperty] string navArg;
    
    [NotifyPropertyChangedFor(nameof(ShowBackArg))]
    [ObservableProperty] 
    string? backArg;
    public bool ShowBackArg => !String.IsNullOrWhiteSpace(BackArg);
    
    [RelayCommand] Task NavByUri() => navigator.NavigateTo("another", ("Arg", this.NavArg));

    [RelayCommand]
    Task NavByViewModel() => navigator.NavigateTo<AnotherViewModel>(
        x => x.IsNavFromViewModel = true, 
        ("Arg", this.NavArg)
    );

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.Count > 0)
        {
            var pair = query.First();
            this.BackArg = $"{pair.Key}={pair.Value}";
        }
    }
}