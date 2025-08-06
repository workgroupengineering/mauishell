namespace Shiny;


public static class AutoHapticsExtensions
{
    public static IAutoHaptics AddButton(this IAutoHaptics haptics)
    {
        
        haptics.Hook<Button>(
            (btn, vibrate) => btn.Clicked += (sender, args) => vibrate(),
            btn => btn.Clicked -= null
        );
        return haptics;
    }    
}