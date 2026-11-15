using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// The conventions CLAUDE.md states for the code under src that no analyser holds, read from the syntax tree so a comment,
/// a string holding <c>!=</c> or a doc comment naming a keyword is not mistaken for code.
/// </summary>
public sealed class SourceConventionsShould
{
    private const string CoreFolder = @"src\Source191.Cader.Core\";
    private const string ApplicationFolder = @"src\Source191.Cader.Core\Application";
    private const string SharedSnapshotFile = @"src\Source191.Cader.Core\Application\SharedSnapshot.cs";

    private static readonly string[] ArgumentExceptionFiles =
    [
        @"src\Source191.Cader.Infrastructure\Claude\ClaudeSettings.cs",
    ];

    /// <summary>
    /// The static initialisers Core keeps because the value's identity is its meaning; neither holds anything Stryker
    /// mutates. <c>StatusMessage.None</c>: the reducers and the screen compare status messages with ReferenceEquals.
    /// <c>DetailRequests.NoRow</c>: an object that stands for no row by being no other object.
    /// </summary>
    private static readonly string[] KeptStaticInitialisers = ["StatusMessage.None", "DetailRequests.NoRow"];

    private static readonly string[] StateFileRecords = ["UserConfig", "EditorSetting", "TabFile", "UpdateCheckFile", "LastRunFile"];

    private static readonly string[] AmericanWords =
    [
        "color", "colors", "colored", "behavior", "center", "centered", "canceled", "favorite", "gray", "initialize",
        "initialized", "organize", "analyze", "catalog", "normalize", "recognize", "customize", "disk",
    ];

    private static readonly Regex AmericanWord = new($@"\b({string.Join('|', AmericanWords)})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    [Fact]
    public void UseNoNullForgivingOperator()
    {
        List<string> offenders = [.. Find(["src"], item => item.AsNode() is PostfixUnaryExpressionSyntax postfix && postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression)).Select(Describe)];

        offenders.ShouldBeEmpty($"{offenders.Count} uses of the null-forgiving operator:{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    [Fact]
    public void KeepVolatileInsideSharedSnapshot()
    {
        List<string> files = [.. Find(["src"], item => item.IsToken && item.IsKind(SyntaxKind.VolatileKeyword)).Select(found => found.RelativePath).Distinct(StringComparer.Ordinal)];

        files.ShouldBe([SharedSnapshotFile]);
    }

    [Fact]
    public void CatchArgumentExceptionOnlyWhereNamed()
    {
        List<string> files = [.. Find(["src"], item => item.AsNode() is CatchDeclarationSyntax { Type: var type } && type.ToString() is "ArgumentException" or "System.ArgumentException")
            .Select(found => found.RelativePath)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

        files.ShouldBe([.. ArgumentExceptionFiles.Order(StringComparer.Ordinal)]);
    }

    [Fact]
    public void GiveNoParameterADefaultValue()
    {
        List<string> offenders = [.. Find(["src"], IsParameterWithDefaultValue).Select(Describe)];

        offenders.ShouldBeEmpty($"{offenders.Count} parameters with a default value:{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    private static bool IsParameterWithDefaultValue(SyntaxNodeOrToken item)
    {
        bool hasDefault = item.AsNode() is ParameterSyntax { Default: not null };

        return hasDefault && !IsStateFileRecordParameter(item.AsNode());
    }

    private static bool IsStateFileRecordParameter(SyntaxNode? parameter)
    {
        RecordDeclarationSyntax? record = parameter?.Parent?.Parent as RecordDeclarationSyntax;

        return record is not null && StateFileRecords.Contains(record.Identifier.Text);
    }

    [Fact]
    public void SpellBritish()
    {
        List<string> offenders = [.. Find(["src"], IsWordingText)
            .SelectMany(found => AmericanWord.Matches(found.Text).Select(match => $"{Describe(found)}: {match.Value}"))];

        offenders.ShouldBeEmpty($"{offenders.Count} American spellings in Wording and the command line:{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    private static bool IsWordingText(SyntaxNodeOrToken item)
    {
        bool isText = item.IsToken && (item.IsKind(SyntaxKind.StringLiteralToken) || item.IsKind(SyntaxKind.InterpolatedStringTextToken) || item.IsKind(SyntaxKind.Utf8StringLiteralToken));
        string? file = item.SyntaxTree?.FilePath;

        return isText && file is not null && IsWordingFile(file);
    }

    private static bool IsWordingFile(string relativePath)
    {
        string name = Path.GetFileName(relativePath);
        bool inApplication = string.Equals(Path.GetDirectoryName(relativePath), ApplicationFolder, StringComparison.Ordinal);

        return inApplication && (name == "CommandLine.cs" || name.StartsWith("Wording", StringComparison.Ordinal));
    }

    /// <summary>
    /// Core declares no static initialiser, so mutation testing never meets a value built once per test process (Stryker
    /// switches mutants without restarting it, stryker-mutator/stryker-net#3742): a table or sentinel is a computed
    /// property, and each mutant in it is covered by the tests that read it. A constant stays a <c>const</c>.
    /// </summary>
    [Fact]
    public void GiveCoreNoStaticInitialiser()
    {
        List<string> offenders = [.. Find(["src"], IsStaticInitialiserInCore).Select(found => $"{Describe(found)}: {found.Text.Split('\n')[0].Trim()}")];

        offenders.ShouldBeEmpty($"{offenders.Count} static initialisers in Core:{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    private static bool IsStaticInitialiserInCore(SyntaxNodeOrToken item)
    {
        string file = item.SyntaxTree?.FilePath ?? string.Empty;
        (bool isInitialiser, string member) = item.AsNode() switch
        {
            VariableDeclaratorSyntax { Initializer: not null, Parent.Parent: FieldDeclarationSyntax field } declarator => (field.Modifiers.Any(SyntaxKind.StaticKeyword), declarator.Identifier.Text),
            PropertyDeclarationSyntax { Initializer: not null } property => (property.Modifiers.Any(SyntaxKind.StaticKeyword), property.Identifier.Text),
            ConstructorDeclarationSyntax constructor => (constructor.Modifiers.Any(SyntaxKind.StaticKeyword), constructor.Identifier.Text),
            _ => (false, string.Empty),
        };
        string? type = item.AsNode()?.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;

        return isInitialiser && file.StartsWith(CoreFolder, StringComparison.Ordinal) && !KeptStaticInitialisers.Contains($"{type}.{member}");
    }

    /// <summary>
    /// A hierarchy outside Core is a <c>closed</c> record, so its switches need no discard arm. Core is held at C# 14 until
    /// Stryker compiles C# 15 (its README), and its hierarchies stay abstract until then.
    /// </summary>
    [Fact]
    public void DeclareEveryHierarchyOutsideCoreClosed()
    {
        List<string> offenders = [.. Find(["src", "tools"], IsAbstractRecordOutsideCore).Select(found => $"{Describe(found)}: {found.Text.Split('\n')[0].Trim()}")];

        offenders.ShouldBeEmpty($"{offenders.Count} abstract records outside Core:{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    private static bool IsAbstractRecordOutsideCore(SyntaxNodeOrToken item)
    {
        bool isAbstractRecord = item.AsNode() is RecordDeclarationSyntax record && record.Modifiers.Any(SyntaxKind.AbstractKeyword);
        string file = item.SyntaxTree?.FilePath ?? string.Empty;

        return isAbstractRecord && !file.StartsWith(CoreFolder, StringComparison.Ordinal);
    }

    [Fact]
    public void FindAThrowingDiscardOnlyInASwitchOverAClosedType()
    {
        SyntaxTree probe = SourceFiles.Parse(ClosedSwitchProbe, "probe.cs");

        ThrowingDiscardsOverClosedTypes([probe]).ShouldBe(["probe.cs(18)"]);
    }

    /// <summary>
    /// The compiler knows every case of a closed type, so a switch expression over one needs no discard arm and a new case
    /// fails the build with CS8509 where an arm that throws would hide it until run time. A switch statement is not checked
    /// by the compiler, so it keeps its throwing default.
    /// </summary>
    [Fact]
    public void KeepNoThrowingDiscardInASwitchOverAClosedType()
    {
        List<SyntaxTree> trees = [.. SourceFiles.Under("src", "tools").Select(relativePath => SourceFiles.Parse(SourceFiles.Read(relativePath), relativePath))];

        List<string> offenders = [.. ThrowingDiscardsOverClosedTypes(trees)];

        offenders.ShouldBeEmpty($"{offenders.Count} throwing discard arms over a closed type:{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    private const string ClosedSwitchProbe = """
        namespace Northwind.Probe;

        public closed record Shape
        {
            public sealed record Circle : Shape;

            public sealed record Square(int Side) : Shape;
        }

        public abstract record Open;

        public static class Uses
        {
            public static int Sides(Shape shape) => shape switch
            {
                Shape.Circle => 0,
                Shape.Square square => 4,
                _ => throw new System.Diagnostics.UnreachableException(),
            };

            public static int Opened(Open open) => open switch
            {
                Open => 1,
                _ => throw new System.Diagnostics.UnreachableException(),
            };
        }
        """;

    private static IEnumerable<string> ThrowingDiscardsOverClosedTypes(IReadOnlyList<SyntaxTree> trees)
    {
        HashSet<string> closed = [.. trees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            .Where(type => type.Modifiers.Any(modifier => modifier.Text == "closed"))
            .Select(type => type.Identifier.Text)];

        return trees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<SwitchExpressionSyntax>())
            .Where(switchExpression => switchExpression.Arms.Any(arm => NamesACaseOf(arm.Pattern, closed)))
            .SelectMany(switchExpression => switchExpression.Arms.Where(IsThrowingDiscard))
            .Select(arm => $"{arm.SyntaxTree.FilePath}({arm.GetLocation().GetLineSpan().StartLinePosition.Line + 1})");
    }

    private static bool IsThrowingDiscard(SwitchExpressionArmSyntax arm) => arm is { Pattern: DiscardPatternSyntax, WhenClause: null, Expression: ThrowExpressionSyntax };

    // shortcut: a case named bare (a switch inside its own closed type) is not seen; none exists, so read the governing type with a semantic model once one does.
    private static bool NamesACaseOf(PatternSyntax pattern, HashSet<string> closed) => pattern.DescendantNodesAndSelf().Select(Qualifier).Any(name => name is not null && closed.Contains(name));

    /// <summary>The type a case is reached through: <c>Shape</c> in <c>Shape.Circle</c>, whether it is read as a type or a constant.</summary>
    private static string? Qualifier(SyntaxNode node) => node switch
    {
        QualifiedNameSyntax qualified => LastName(qualified.Left),
        MemberAccessExpressionSyntax access => LastName(access.Expression),
        _ => null,
    };

    private static string? LastName(SyntaxNode node) => node switch
    {
        SimpleNameSyntax simple => simple.Identifier.Text,
        QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
        MemberAccessExpressionSyntax access => access.Name.Identifier.Text,
        _ => null,
    };

    private static string Describe(Found found) => $"{found.RelativePath}({found.Line})";

    private static IEnumerable<Found> Find(string[] folders, Func<SyntaxNodeOrToken, bool> matches)
    {
        foreach (string relativePath in SourceFiles.Under(folders))
        {
            SyntaxTree tree = SourceFiles.Parse(SourceFiles.Read(relativePath), relativePath);
            foreach (SyntaxNodeOrToken item in tree.GetRoot().DescendantNodesAndTokens().Where(matches))
            {
                yield return new Found(relativePath, item.GetLocation()?.GetLineSpan().StartLinePosition.Line + 1 ?? 0, item.ToString());
            }
        }
    }

    private sealed record Found(string RelativePath, int Line, string Text);
}