namespace Shiny;

public interface IPageLifecycleAware
{
    void OnAppearing();
    void OnDisappearing();
}