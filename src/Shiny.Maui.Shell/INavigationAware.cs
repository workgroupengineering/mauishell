namespace Shiny;

public interface INavigationAware
{
    /// <summary>
    /// Invoked before navigation away from the current view.  You are able to mutate the arguments before they are passed to the next view.
    /// </summary>
    /// <param name="parameters">A dictionary containing parameters to pass to the next view. Keys represent parameter names, and values
    /// represent their corresponding data.</param>
    void OnNavigatingFrom(IDictionary<string, object> parameters);
}