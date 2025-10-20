//HintName: NavigationExtensions.g.cs
#nullable enable
public static class NavigationExtensions
{
    public static global::System.Threading.Tasks.Task NavigateToTest(this global::Shiny.INavigator navigator, string validProp = null)
    {
        return navigator.NavigateTo<TestApp.TestViewModel>(x => { x.ValidProp = validProp; });
    }

}
