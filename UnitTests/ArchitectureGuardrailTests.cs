using System.Text;
using System.Text.RegularExpressions;

namespace UnitTests;

public class ArchitectureGuardrailTests
{
    private static readonly DirectoryInfo RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void WinUI_DoesNotReferenceInfrastructureOutsideComposition()
    {
        Violation[] violations = EnumerateSourceFiles("WinUI", ".cs", ".xaml")
            .Where(path => !IsUnder(path, "WinUI", "Composition"))
            .SelectMany(path => FindMatches(
                path,
                @"\bInfrastructure\.|\busing\s+Infrastructure\b",
                "WinUI may depend on Infrastructure only from the Composition bootstrap layer."))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void ViewsAndUserControls_DoNotUseAppHostServiceLocator()
    {
        Violation[] violations = EnumerateSourceFiles(Path.Combine("WinUI", "Views"), ".cs", ".xaml")
            .SelectMany(path => FindMatches(
                path,
                @"\bApp\.Host\b",
                "Views/UserControls should receive state through binding or dependency properties, not App.Host."))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void ViewModels_DoNotDependOnRepositoriesDirectly()
    {
        Violation[] violations = EnumerateSourceFiles(Path.Combine("WinUI", "ViewModels"), ".cs")
            .SelectMany(path => FindMatches(
                path,
                @"\bIRepository\s*<",
                "ViewModels should depend on Application services/use cases, not IRepository<T>."))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void Application_DoesNotContainPresentationDialogContracts()
    {
        Violation[] violations = EnumerateSourceFiles("Application", ".cs")
            .SelectMany(path => FindMatches(
                path,
                @"\bDialogKey\b|\bIDialogService\b|\bContentDialog\b|\bMicrosoft\.UI\.Xaml\b|\bWinUI\.",
                "Application should expose use-case contracts/results, not WinUI dialog or presentation contracts."))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void WinUI_ServiceLocatorUsageIsConfinedToBootstrapAndComposition()
    {
        Violation[] violations = EnumerateSourceFiles("WinUI", ".cs")
            .Where(path => !IsUnder(path, "WinUI", "Composition"))
            .Where(path => !IsPath(path, "WinUI", "App.xaml.cs"))
            .SelectMany(path => FindMatches(
                path,
                @"\bIServiceProvider\b|\bGetRequiredService\s*<|\bGetService\s*(?:<|\()",
                "WinUI services/builders/ViewModels should receive concrete dependencies; IServiceProvider belongs in bootstrap/composition."))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void WinUI_DoesNotUseMockImageAsDataFallback()
    {
        Violation[] violations = EnumerateSourceFiles("WinUI", ".cs", ".xaml")
            .SelectMany(path => FindMatches(
                path,
                @"Mock\.png|MockBrush|DefaultImageUri",
                "Missing images should stay empty in state and be rendered by the View as placeholder UI."))
            .ToArray();

        AssertNoViolations(violations);
    }

    private static IEnumerable<string> EnumerateSourceFiles(string relativeRoot, params string[] extensions)
    {
        string rootPath = Path.Combine(RepositoryRoot.FullName, relativeRoot);
        if (!Directory.Exists(rootPath))
        {
            return [];
        }

        HashSet<string> allowedExtensions = extensions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(path => allowedExtensions.Contains(Path.GetExtension(path)))
            .Where(path => !IsInGeneratedOutput(path));
    }

    private static IEnumerable<Violation> FindMatches(string path, string pattern, string reason)
    {
        Regex regex = new(pattern, RegexOptions.CultureInvariant);
        int lineNumber = 0;

        foreach (string line in File.ReadLines(path))
        {
            lineNumber++;
            if (regex.IsMatch(line))
            {
                yield return new Violation(ToRepositoryPath(path), lineNumber, reason, line.Trim());
            }
        }
    }

    private static bool IsInGeneratedOutput(string path)
    {
        string relativePath = ToRepositoryPath(path);
        return relativePath.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || relativePath.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUnder(string path, params string[] relativeSegments)
    {
        string expectedPrefix = string.Join('/', relativeSegments).TrimEnd('/') + "/";
        return ToRepositoryPath(path).StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPath(string path, params string[] relativeSegments)
    {
        string expectedPath = string.Join('/', relativeSegments);
        return string.Equals(ToRepositoryPath(path), expectedPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string ToRepositoryPath(string path)
    {
        return Path.GetRelativePath(RepositoryRoot.FullName, path).Replace('\\', '/');
    }

    private static void AssertNoViolations(IReadOnlyCollection<Violation> violations)
    {
        if (violations.Count == 0)
        {
            return;
        }

        StringBuilder message = new("Architecture guardrail violation(s):");
        foreach (Violation violation in violations)
        {
            message
                .AppendLine()
                .Append("- ")
                .Append(violation.File)
                .Append(':')
                .Append(violation.Line)
                .Append(" - ")
                .Append(violation.Reason)
                .Append(" [")
                .Append(violation.Text)
                .Append(']');
        }

        Assert.Fail(message.ToString());
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "PlayPointPOS.slnx")))
            {
                return directory;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing PlayPointPOS.slnx.");
    }

    private sealed record Violation(string File, int Line, string Reason, string Text);
}
