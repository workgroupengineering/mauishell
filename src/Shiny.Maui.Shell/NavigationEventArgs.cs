namespace Shiny;

public record NavigationEventArgs(
    string? FromUri,
    object? FromViewModel,
    string ToUri,
    NavigationType NavigationType,
    IReadOnlyDictionary<string, object> Parameters
);

public record NavigatedEventArgs(
    string ToUri,
    object? ToViewModel,
    NavigationType NavigationType,
    IReadOnlyDictionary<string, object> Parameters
);
