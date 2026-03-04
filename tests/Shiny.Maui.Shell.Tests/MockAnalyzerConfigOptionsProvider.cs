using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Shiny.Maui.Shell.Tests;

public class MockAnalyzerConfigOptionsProvider(Dictionary<string, string> globalOptions) : AnalyzerConfigOptionsProvider
{
    public override AnalyzerConfigOptions GlobalOptions { get; } = new MockAnalyzerConfigOptions(globalOptions);
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => MockAnalyzerConfigOptions.Empty;
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => MockAnalyzerConfigOptions.Empty;
}

public class MockAnalyzerConfigOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
{
    public static MockAnalyzerConfigOptions Empty { get; } = new(new Dictionary<string, string>());

    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        => options.TryGetValue(key, out value);
}
