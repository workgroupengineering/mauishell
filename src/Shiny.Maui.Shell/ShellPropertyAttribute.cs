namespace Shiny;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ShellPropertyAttribute(bool required = true) : Attribute
{
    public bool IsRequired => required;
}