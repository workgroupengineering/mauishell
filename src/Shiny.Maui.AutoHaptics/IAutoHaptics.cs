namespace Shiny;

public interface IAutoHaptics
{
    bool Enabled { get; set; } // all
    
    void Hook<TElement>(Action<TElement, Action> hook, Action<TElement> unhook)
        where TElement : Element;
}