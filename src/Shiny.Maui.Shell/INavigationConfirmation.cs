namespace Shiny;

public interface INavigationConfirmation
{
    Task<bool> CanNavigate();
}