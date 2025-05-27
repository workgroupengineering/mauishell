namespace Shiny;

public interface INavigateConfirm
{
    Task<bool> CanNavigate();
}