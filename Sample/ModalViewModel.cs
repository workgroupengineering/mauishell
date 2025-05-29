using Shiny;

namespace Sample;

public partial class ModalViewModel(INavigator navigator) : ObservableObject
{
    [RelayCommand]
    async Task Close()
    {
        
    }
}