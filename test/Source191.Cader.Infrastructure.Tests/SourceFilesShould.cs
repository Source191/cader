using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// The parser behind every source-text fact reads C# 15, so a file that uses its syntax is never read as broken text and
/// skipped or misjudged by <see cref="SourceConventionsShould"/> and <see cref="OneTypePerFileShould"/>.
/// </summary>
public sealed class SourceFilesShould
{
    private const string CSharp15 = """
        namespace Northwind.Probe;

        public closed record Shape
        {
            public sealed record Circle : Shape;
        }

        public union Pet(Cat, Dog);

        public sealed record Cat;

        public sealed record Dog;

        public static class Uses
        {
            public static HashSet<string> Names()
            {
                return [with(StringComparer.Ordinal), "Northwind"];
            }

            public static bool AnyPositive(int[][] rows)
            {
                bool found = false;
                outer: foreach (int[] row in rows)
                {
                    foreach (int cell in row)
                    {
                        if (cell > 0)
                        {
                            found = true;
                            break outer;
                        }
                    }
                }

                return found;
            }
        }
        """;

    [Fact]
    public void ParseCSharp15WithoutASyntaxError()
    {
        IEnumerable<Diagnostic> errors = SourceFiles.Parse(CSharp15).GetDiagnostics(TestContext.Current.CancellationToken).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        errors.Select(error => error.ToString()).ShouldBeEmpty();
    }

    [Fact]
    public void ReadAUnionAndAClosedRecordAsTypeDeclarations()
    {
        List<BaseTypeDeclarationSyntax> types = [.. SourceFiles.Parse(CSharp15).GetCompilationUnitRoot(TestContext.Current.CancellationToken)
            .DescendantNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Single()
            .Members
            .OfType<BaseTypeDeclarationSyntax>()];

        types.Select(type => type.Identifier.Text).ShouldBe(["Shape", "Pet", "Cat", "Dog", "Uses"]);
        types[0].Modifiers.Select(modifier => modifier.Text).ShouldBe(["public", "closed"]);
    }
}