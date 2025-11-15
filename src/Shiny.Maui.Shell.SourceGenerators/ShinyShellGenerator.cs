using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Shiny.Maui.Shell.SourceGenerators;


[Generator(LanguageNames.CSharp)]
public class ShinyShellGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find classes with ShellMapAttribute
        var shellMapClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetShellMapClass(ctx))
            .Where(static m => m is not null)
            .Collect();

        context.RegisterSourceOutput(shellMapClasses, (spc, classes) => GenerateCode(spc, classes));
    }

    static ShellMapInfo? GetShellMapClass(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is IMethodSymbol attributeSymbol)
                {
                    var attributeClass = attributeSymbol.ContainingType;
                    if (attributeClass.Name == "ShellMapAttribute" && attributeClass.IsGenericType)
                    {
                        var pageType = attributeClass.TypeArguments[0];
                        var route = GetRouteFromAttribute(attribute);
                        var registerRoute = GetRegisterRouteFromAttribute(attribute);
                        var properties = GetShellProperties(classDeclaration, context.SemanticModel);
                        
                        var viewModelSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
                        return new ShellMapInfo(
                            classDeclaration.Identifier.ValueText,
                            viewModelSymbol?.ToDisplayString() ?? classDeclaration.Identifier.ValueText, // Fully qualified viewmodel name
                            pageType.Name, // Use just the name for constant generation
                            pageType.ToDisplayString(), // Full qualified name for type references
                            route ?? pageType.Name,
                            registerRoute, // Use the registerRoute parameter from attribute
                            properties
                        );
                    }
                }
            }
        }
        
        return null;
    }

    static string? GetRouteFromAttribute(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList?.Arguments.Count > 0)
        {
            // Look for the route parameter specifically
            foreach (var arg in attribute.ArgumentList.Arguments)
            {
                // Check if it's a named argument for "route"
                if (arg.NameColon?.Name.Identifier.ValueText == "route")
                {
                    if (arg.Expression is LiteralExpressionSyntax literal)
                    {
                        return literal.Token.ValueText;
                    }
                }
                // If it's the first positional argument and not a named argument for registerRoute
                else if (arg == attribute.ArgumentList.Arguments[0] && 
                         arg.NameColon?.Name.Identifier.ValueText != "registerRoute")
                {
                    if (arg.Expression is LiteralExpressionSyntax literal &&
                        literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                    {
                        return literal.Token.ValueText;
                    }
                }
            }
        }
        return null;
    }

    static bool GetRegisterRouteFromAttribute(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList?.Arguments.Count > 0)
        {
            var arguments = attribute.ArgumentList.Arguments;
            
            // Look for the registerRoute parameter specifically
            foreach (var arg in arguments)
            {
                // Check if it's a named argument for "registerRoute"
                if (arg.NameColon?.Name.Identifier.ValueText == "registerRoute")
                {
                    if (arg.Expression is LiteralExpressionSyntax literal)
                    {
                        if (literal.Token.IsKind(SyntaxKind.FalseKeyword))
                            return false;
                        if (literal.Token.IsKind(SyntaxKind.TrueKeyword))
                            return true;
                    }
                }
            }
            
            // Check positional arguments
            for (int i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];
                
                // If it's the second positional argument (index 1) and not a named argument
                if (i == 1 && arg.NameColon == null)
                {
                    if (arg.Expression is LiteralExpressionSyntax literal)
                    {
                        if (literal.Token.IsKind(SyntaxKind.FalseKeyword))
                            return false;
                        if (literal.Token.IsKind(SyntaxKind.TrueKeyword))
                            return true;
                    }
                }
                // Handle case where registerRoute is the first argument (when route is omitted)
                else if (i == 0 && 
                         arg.NameColon == null &&
                         arg.Expression is LiteralExpressionSyntax literal &&
                         (literal.Token.IsKind(SyntaxKind.TrueKeyword) || literal.Token.IsKind(SyntaxKind.FalseKeyword)))
                {
                    if (literal.Token.IsKind(SyntaxKind.FalseKeyword))
                        return false;
                    if (literal.Token.IsKind(SyntaxKind.TrueKeyword))
                        return true;
                }
            }
        }
        // Default value is true according to the attribute definition
        return true;
    }

    static ImmutableArray<ShellPropertyInfo> GetShellProperties(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        var properties = ImmutableArray.CreateBuilder<ShellPropertyInfo>();
        
        foreach (var member in classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            foreach (var attributeList in member.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(attribute);
                    if (symbolInfo.Symbol is IMethodSymbol attributeSymbol &&
                        attributeSymbol.ContainingType.Name == "ShellPropertyAttribute")
                    {
                        var isRequired = GetIsRequiredFromAttribute(attribute);
                        var propertySymbol = semanticModel.GetDeclaredSymbol(member) as IPropertySymbol;
                        
                        if (propertySymbol != null)
                        {
                            // Check if property has public get/set
                            var hasPublicGetter = propertySymbol.GetMethod?.DeclaredAccessibility == Accessibility.Public;
                            var hasPublicSetter = propertySymbol.SetMethod?.DeclaredAccessibility == Accessibility.Public;
                            
                            if (!hasPublicGetter || !hasPublicSetter)
                            {
                                // This would ideally be a diagnostic error, but for now we'll skip
                                continue;
                            }
                            else
                            {
                                properties.Add(new ShellPropertyInfo(
                                    member.Identifier.ValueText,
                                    propertySymbol.Type.ToDisplayString(),
                                    isRequired
                                ));
                            }
                        }
                    }
                }
            }
        }
        
        return properties.ToImmutable();
    }

    static bool GetIsRequiredFromAttribute(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList?.Arguments.Count > 0)
        {
            var firstArg = attribute.ArgumentList.Arguments[0];
            if (firstArg.Expression is LiteralExpressionSyntax literal &&
                literal.Token.IsKind(SyntaxKind.TrueKeyword))
            {
                return true;
            }
        }
        return false;
    }

    static void GenerateCode(SourceProductionContext context, ImmutableArray<ShellMapInfo?> classes)
    {
        var validClasses = classes.Where(c => c != null).Cast<ShellMapInfo>().ToImmutableArray();
        
        if (validClasses.IsEmpty)
            return;

        // Generate Routes class
        GenerateRoutesClass(context, validClasses);
        
        // Generate NavigationExtensions class
        GenerateNavigationExtensions(context, validClasses);
        
        // Generate NavigationBuilderExtensions class
        GenerateNavigationBuilderExtensions(context, validClasses);
    }

    static void GenerateRoutesClass(SourceProductionContext context, ImmutableArray<ShellMapInfo> classes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("public static class Routes");
        sb.AppendLine("{");
        
        foreach (var cls in classes)
        {
            var constantName = cls.PageTypeName.Replace("Page", "");
            sb.AppendLine($"    public const string {constantName} = \"{cls.Route}\";");
        }
        
        sb.AppendLine("}");
        
        context.AddSource("Routes.g.cs", sb.ToString());
    }

    static void GenerateNavigationExtensions(SourceProductionContext context, ImmutableArray<ShellMapInfo> classes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("public static class NavigationExtensions");
        sb.AppendLine("{");
        
        foreach (var cls in classes)
        {
            var methodName = $"NavigateTo{cls.PageTypeName.Replace("Page", "")}";
            var requiredParams = cls.Properties.Where(p => p.IsRequired).ToList();
            var optionalParams = cls.Properties.Where(p => !p.IsRequired).ToList();
            
            sb.Append($"    public static global::System.Threading.Tasks.Task {methodName}(this global::Shiny.INavigator navigator");
            
            // Add required parameters first
            foreach (var prop in requiredParams)
            {
                sb.Append($", {prop.TypeName} {ToCamelCase(prop.Name)}");
            }
            
            // Add optional parameters last
            foreach (var prop in optionalParams)
            {
                var defaultValue = GetDefaultValue(prop.TypeName);
                sb.Append($", {prop.TypeName} {ToCamelCase(prop.Name)} = {defaultValue}");
            }
            
            // If no properties, add the params argument
            if (!cls.Properties.Any())
            {
                sb.Append(", params global::System.Collections.Generic.IEnumerable<(string Key, object Value)> args");
            }
            
            sb.AppendLine(")");
            sb.AppendLine("    {");
            
            if (cls.Properties.Any())
            {
                sb.Append($"        return navigator.NavigateTo<{cls.ViewModelFullName}>(x => ");
                sb.Append("{ ");
                
                var assignments = cls.Properties.Select(p => $"x.{p.Name} = {ToCamelCase(p.Name)}");
                sb.Append(string.Join("; ", assignments));
                sb.Append(";");
                
                sb.AppendLine(" });");
            }
            else
            {
                sb.AppendLine($"        return navigator.NavigateTo<{cls.ViewModelFullName}>(configure: null, args: args);");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine();
        }
        
        sb.AppendLine("}");
        
        context.AddSource("NavigationExtensions.g.cs", sb.ToString());
    }

    static void GenerateNavigationBuilderExtensions(SourceProductionContext context, ImmutableArray<ShellMapInfo> classes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("public static class NavigationBuilderExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    public static global::Shiny.ShinyAppBuilder AddGeneratedMaps(this global::Shiny.ShinyAppBuilder builder)");
        sb.AppendLine("    {");
        
        foreach (var cls in classes)
        {
            var constantName = cls.PageTypeName.Replace("Page", "");
            if (cls.RegisterRoute)
            {
                sb.AppendLine($"        builder.Add<{cls.PageTypeFullName}, {cls.ViewModelFullName}>(Routes.{constantName});");
            }
            else
            {
                sb.AppendLine($"        builder.Add<{cls.PageTypeFullName}, {cls.ViewModelFullName}>(Routes.{constantName}, registerRoute: false);");
            }
        }
        
        sb.AppendLine("        return builder;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        context.AddSource("NavigationBuilderExtensions.g.cs", sb.ToString());
    }

    static string ToCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text) || char.IsLower(text[0]))
            return text;
        return char.ToLower(text[0]) + text.Substring(1);
    }

    static string GetDefaultValue(string typeName)
    {
        return typeName.EndsWith("?") || typeName == "string" ? "null" : "default";
    }
}

record ShellMapInfo(
    string ViewModelName,
    string ViewModelFullName,
    string PageTypeName,
    string PageTypeFullName,
    string Route,
    bool RegisterRoute,
    ImmutableArray<ShellPropertyInfo> Properties
);

record ShellPropertyInfo(
    string Name,
    string TypeName,
    bool IsRequired
);