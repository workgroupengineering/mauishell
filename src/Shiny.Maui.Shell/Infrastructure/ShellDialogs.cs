namespace Shiny.Infrastructure;

public class ShellDialogs(IDispatcher dispatcher) : IDialogs
{
    public Task Alert(string? title, string message, string acceptText = "OK")
        => dispatcher.DispatchAsync(() =>
            Shell.Current.DisplayAlertAsync(title, message, acceptText)
        );
    

    public Task<bool> Confirm(string? title, string message, string acceptText = "Yes", string cancelText = "No")
        => Shell.Current.DisplayAlertAsync(title, message, acceptText, cancelText);


    public Task<string?> Prompt(
        string? title,
        string message,
        string acceptText = "OK",
        string cancelText = "Cancel",
        string? placeholder = null,
        string initialValue = "",
        int maxLength = -1,
        Keyboard? keyboard = null
    ) => Shell.Current.DisplayPromptAsync(
        title ?? String.Empty,
        message,
        acceptText,
        cancelText,
        placeholder,
        maxLength,
        keyboard,
        initialValue
    );


    public Task<string> ActionSheet(string? title, string? cancel, string? destruction, params string[] buttons)
        => Shell.Current.DisplayActionSheetAsync(title, cancel, destruction, buttons);
}