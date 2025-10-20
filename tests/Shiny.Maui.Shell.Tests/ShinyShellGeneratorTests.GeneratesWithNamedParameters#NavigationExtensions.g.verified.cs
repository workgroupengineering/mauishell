//HintName: NavigationExtensions.g.cs
#nullable enable
public static class NavigationExtensions
{
    public static global::System.Threading.Tasks.Task NavigateToProfile(this global::Shiny.INavigator navigator, params global::System.Collections.Generic.IEnumerable<(string Key, object Value)> args)
    {
        return navigator.NavigateTo<TestApp.ProfileViewModel>(configure: null, args: args);
    }

}
