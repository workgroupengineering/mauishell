using Microsoft.Extensions.Logging;
using Shiny;

namespace Sample;

public class NavigationLogger(
    ILogger<NavigationLogger> logger,
    INavigator navigator
) : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        navigator.Navigating += (_, args) =>
        {
            logger.LogInformation(
                "Navigating from '{FromUri}' to '{ToUri}' | Type: {NavigationType} | FromViewModel: {FromViewModel} | Parameters: {Parameters}",
                args.FromUri,
                args.ToUri,
                args.NavigationType,
                args.FromViewModel?.GetType().Name,
                string.Join(", ", args.Parameters.Select(p => $"{p.Key}={p.Value}"))
            );
        };

        navigator.Navigated += (_, args) =>
        {
            logger.LogInformation(
                "Navigated to '{ToUri}' | Type: {NavigationType} | ToViewModel: {ToViewModel} | Parameters: {Parameters}",
                args.ToUri,
                args.NavigationType,
                args.ToViewModel?.GetType().Name,
                string.Join(", ", args.Parameters.Select(p => $"{p.Key}={p.Value}"))
            );
        };
    }
}
