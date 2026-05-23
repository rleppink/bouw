using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace FileLineLimit;

/// <summary>
/// Fails the build when a source file exceeds the configured line limit.
/// The limit defaults to 300 and can be overridden in .editorconfig via
/// <c>max_file_lines</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FileLineCountAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LINE0001";

    private const int DefaultMaxLines = 300;
    private const string MaxLinesOption = "max_file_lines";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "File exceeds the maximum line count",
        messageFormat: "'{0}' has {1} lines, exceeding the {2}-line limit; split it into smaller files",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Large files are hard to read and review. Keep files within the configured line limit."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxTreeAction(Analyze);
    }

    private static void Analyze(SyntaxTreeAnalysisContext context)
    {
        SyntaxTree tree = context.Tree;
        int maxLines = ResolveMaxLines(context, tree);

        SourceText text = tree.GetText(context.CancellationToken);
        int lineCount = text.Lines.Count;

        // SourceText counts a trailing newline as an extra empty line; a file of
        // N content lines ending in a newline should count as N, not N + 1.
        if (lineCount > 0 && text.Lines[lineCount - 1].Span.IsEmpty)
        {
            lineCount--;
        }

        if (lineCount <= maxLines)
        {
            return;
        }

        Location location = Location.Create(tree, TextSpan.FromBounds(0, 0));
        context.ReportDiagnostic(
            Diagnostic.Create(Rule, location, tree.FilePath, lineCount, maxLines)
        );
    }

    private static int ResolveMaxLines(SyntaxTreeAnalysisContext context, SyntaxTree tree)
    {
        AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(
            tree
        );

        return
            options.TryGetValue(MaxLinesOption, out string? raw)
            && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            && parsed > 0
            ? parsed
            : DefaultMaxLines;
    }
}
