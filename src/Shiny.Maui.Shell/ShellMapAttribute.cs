namespace Shiny;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ShellMapAttribute<TPage>(
    string? route = null,
    bool registerRoute = true
) : Attribute
{
    public string Route => route ?? typeof(TPage).Name;
    public bool RegisterRoute => registerRoute;
}