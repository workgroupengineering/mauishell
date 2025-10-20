//HintName: NavigationExtensions.g.cs
#nullable enable
public static class NavigationExtensions
{
    public static global::System.Threading.Tasks.Task NavigateToDetails(this global::Shiny.INavigator navigator, string id)
    {
        return navigator.NavigateTo<TestApp.DetailsViewModel>(x => { x.Id = id; });
    }

}
