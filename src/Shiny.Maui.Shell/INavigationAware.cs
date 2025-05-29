namespace Shiny;

public interface INavigationAware
{
    void OnNavigatingFrom(IDictionary<string, object> parameters);
}