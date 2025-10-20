using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shiny.Maui.Shell.SourceGenerators;

namespace Shiny.Maui.Shell.Tests;

public class ShinyShellGeneratorTests
{
    [Fact]
    public Task GeneratesBasicShellMapWithDefaults()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<MainPage>]
            public class MainViewModel
            {
            }
            
            public class MainPage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesShellMapWithCustomRoute()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<HomePage>("home")]
            public class HomeViewModel
            {
            }
            
            public class HomePage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesShellMapWithRegisterRouteFalse()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<MainPage>(registerRoute: false)]
            public class MainViewModel
            {
            }
            
            public class MainPage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesShellMapWithRouteAndRegisterRouteFalse()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<MainPage>("main", false)]
            public class MainViewModel
            {
            }
            
            public class MainPage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesShellMapWithRequiredProperty()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<DetailsPage>]
            public class DetailsViewModel
            {
                [ShellProperty(true)]
                public string Id { get; set; }
            }
            
            public class DetailsPage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesShellMapWithOptionalProperty()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<DetailsPage>]
            public class DetailsViewModel
            {
                [ShellProperty]
                public string? Title { get; set; }
            }
            
            public class DetailsPage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesShellMapWithMultipleProperties()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<EditPage>]
            public class EditViewModel
            {
                [ShellProperty(true)]
                public int ItemId { get; set; }
                
                [ShellProperty]
                public string? Mode { get; set; }
                
                [ShellProperty]
                public bool IsReadOnly { get; set; }
            }
            
            public class EditPage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesMultipleShellMaps()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<MainPage>]
            public class MainViewModel
            {
            }
            
            [ShellMap<DetailsPage>("details")]
            public class DetailsViewModel
            {
                [ShellProperty(true)]
                public string Id { get; set; }
            }
            
            [ShellMap<SettingsPage>(registerRoute: false)]
            public class SettingsViewModel
            {
            }
            
            public class MainPage { }
            public class DetailsPage { }
            public class SettingsPage { }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task IgnoresPropertyWithoutPublicGetter()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<TestPage>]
            public class TestViewModel
            {
                [ShellProperty(true)]
                private string PrivateProp { get; set; }
                
                [ShellProperty]
                public string ValidProp { get; set; }
            }
            
            public class TestPage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesWithComplexTypes()
    {
        var source = """
            using Shiny;
            using System.Collections.Generic;
            
            namespace TestApp;
            
            [ShellMap<DataPage>]
            public class DataViewModel
            {
                [ShellProperty(true)]
                public List<string> Items { get; set; }
                
                [ShellProperty]
                public Dictionary<string, int>? Metadata { get; set; }
            }
            
            public class DataPage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesWithNamedParameters()
    {
        var source = """
            using Shiny;
            
            namespace TestApp;
            
            [ShellMap<ProfilePage>(route: "profile", registerRoute: true)]
            public class ProfileViewModel
            {
            }
            
            public class ProfilePage
            {
            }
            """;

        return TestHelper.Verify(source);
    }
}

static class TestHelper
{
    private const string AttributeSource = """
        namespace Shiny;

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
        public sealed class ShellMapAttribute<TPage>(
            string? route = null,
            bool registerRoute = true
        ) : System.Attribute
        {
            public string Route => route ?? typeof(TPage).Name;
            public bool RegisterRoute => registerRoute;
        }

        [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
        public sealed class ShellPropertyAttribute(bool required = false) : System.Attribute
        {
            public bool IsRequired => required;
        }
        """;

    public static Task Verify(string source)
    {
        // Create syntax trees - one for attributes, one for the test source
        var attributeSyntaxTree = CSharpSyntaxTree.ParseText(AttributeSource);
        var sourceSyntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create references
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
        };

        // Create compilation with both syntax trees
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { attributeSyntaxTree, sourceSyntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create generator
        var generator = new ShinyShellGenerator().AsSourceGenerator();

        // Run generator
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);

        // Verify output
        return Verifier.Verify(driver);
    }
}