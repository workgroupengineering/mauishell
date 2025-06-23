namespace Shiny;

public interface IAutoHaptics
{
    bool Enabled { get; set; } // all
    
    bool PageNavigationEnabled { get; set; }
    bool ButtonClickEnabled { get; set; }
}