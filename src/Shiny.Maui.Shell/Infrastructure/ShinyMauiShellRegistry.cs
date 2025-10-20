namespace Shiny.Infrastructure;


public static class ShinyMauiShellRegistry
{
    static readonly List<Action<global::Shiny.ShinyAppBuilder>> callbacks = new();
    
    public static void RegisterCallback(Action<global::Shiny.ShinyAppBuilder> callback)
    {
        callbacks.Add(callback);
    }
    
    public static void ExecuteCallbacks(global::Shiny.ShinyAppBuilder appBuilder)
    {
        foreach (var cb in callbacks)
            cb(appBuilder);
    }
}