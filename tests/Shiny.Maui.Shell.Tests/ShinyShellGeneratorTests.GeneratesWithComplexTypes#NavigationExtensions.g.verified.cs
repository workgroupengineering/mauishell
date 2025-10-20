//HintName: NavigationExtensions.g.cs
#nullable enable
public static class NavigationExtensions
{
    public static global::System.Threading.Tasks.Task NavigateToData(this global::Shiny.INavigator navigator, System.Collections.Generic.List<string> items, System.Collections.Generic.Dictionary<string, int>? metadata = null)
    {
        return navigator.NavigateTo<TestApp.DataViewModel>(x => { x.Items = items, x.Metadata = metadata; });
    }

}
