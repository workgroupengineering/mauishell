//HintName: NavigationExtensions.g.cs
#nullable enable
public static class NavigationExtensions
{
    public static global::System.Threading.Tasks.Task NavigateToEdit(this global::Shiny.INavigator navigator, int itemId, string? mode = null, bool isReadOnly = default)
    {
        return navigator.NavigateTo<TestApp.EditViewModel>(x => { x.ItemId = itemId, x.Mode = mode, x.IsReadOnly = isReadOnly; });
    }

}
