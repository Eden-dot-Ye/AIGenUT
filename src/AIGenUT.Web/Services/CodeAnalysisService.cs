using System.Text.RegularExpressions;

namespace AIGenUT.Web.Services;

public partial class CodeAnalysisService
{
    private readonly string _sourceDir;
    private readonly string _testDir;

    public CodeAnalysisService(IWebHostEnvironment env)
    {
        _sourceDir = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "DemoLib"));
        _testDir = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "DemoLib.Tests"));
    }

    public ProjectAnalysis AnalyzeProject()
    {
        var sourceFiles = AnalyzeSourceFiles();
        var testFiles = AnalyzeTestFiles();

        MapTestCoverage(sourceFiles, testFiles);

        var coverage = BuildCoverageReport(sourceFiles);
        var statistics = BuildStatistics(sourceFiles, testFiles, coverage);

        return new ProjectAnalysis
        {
            SourceFiles = sourceFiles,
            TestFiles = testFiles,
            Coverage = coverage,
            Statistics = statistics
        };
    }

    public CoverageReport GetCoverageReport()
    {
        var sourceFiles = AnalyzeSourceFiles();
        var testFiles = AnalyzeTestFiles();
        MapTestCoverage(sourceFiles, testFiles);
        return BuildCoverageReport(sourceFiles);
    }

    public List<SourceFileInfo> GetSourceFiles()
    {
        var sourceFiles = AnalyzeSourceFiles();
        var testFiles = AnalyzeTestFiles();
        MapTestCoverage(sourceFiles, testFiles);
        return sourceFiles;
    }

    public List<TestFileInfo> GetTestFiles()
    {
        return AnalyzeTestFiles();
    }

    private List<SourceFileInfo> AnalyzeSourceFiles()
    {
        var files = new List<SourceFileInfo>();

        if (!Directory.Exists(_sourceDir))
            return files;

        foreach (var filePath in Directory.GetFiles(_sourceDir, "*.cs").OrderBy(f => f))
        {
            var fileName = Path.GetFileName(filePath);
            var content = File.ReadAllText(filePath);
            var lines = File.ReadAllLines(filePath);

            var className = ExtractClassName(content);
            if (string.IsNullOrEmpty(className))
                continue;

            var methods = ExtractMethods(content);

            files.Add(new SourceFileInfo
            {
                Name = fileName,
                Path = filePath,
                ClassName = className,
                Methods = methods,
                LineCount = lines.Length
            });
        }

        return files;
    }

    private List<TestFileInfo> AnalyzeTestFiles()
    {
        var files = new List<TestFileInfo>();

        if (!Directory.Exists(_testDir))
            return files;

        foreach (var filePath in Directory.GetFiles(_testDir, "*.cs").OrderBy(f => f))
        {
            var fileName = Path.GetFileName(filePath);
            var content = File.ReadAllText(filePath);

            var className = ExtractClassName(content);
            if (string.IsNullOrEmpty(className))
                continue;

            var testMethods = ExtractTestMethods(content);

            files.Add(new TestFileInfo
            {
                Name = fileName,
                Path = filePath,
                ClassName = className,
                TestMethods = testMethods
            });
        }

        return files;
    }

    private static string ExtractClassName(string content)
    {
        var match = ClassNameRegex().Match(content);
        return match.Success ? match.Groups["name"].Value : string.Empty;
    }

    private static List<MethodInfo> ExtractMethods(string content)
    {
        var methods = new List<MethodInfo>();
        var matches = MethodSignatureRegex().Matches(content);

        foreach (Match match in matches)
        {
            var returnType = match.Groups["returnType"].Value.Trim();
            var methodName = match.Groups["methodName"].Value;
            var isStatic = match.Groups["static"].Success;

            // Skip constructors, properties, and common non-method patterns
            if (string.IsNullOrEmpty(returnType) || returnType == "class" || returnType == "interface" || returnType == "enum")
                continue;

            methods.Add(new MethodInfo
            {
                Name = methodName,
                ReturnType = returnType,
                IsStatic = isStatic
            });
        }

        return methods;
    }

    private static List<string> ExtractTestMethods(string content)
    {
        var testMethods = new List<string>();
        var matches = TestMethodRegex().Matches(content);

        foreach (Match match in matches)
        {
            testMethods.Add(match.Groups["name"].Value);
        }

        return testMethods;
    }

    private static void MapTestCoverage(List<SourceFileInfo> sourceFiles, List<TestFileInfo> testFiles)
    {
        var allTestMethods = testFiles
            .SelectMany(tf => tf.TestMethods)
            .ToList();

        foreach (var sourceFile in sourceFiles)
        {
            foreach (var method in sourceFile.Methods)
            {
                // Match test methods by naming convention: Test{MethodName}_When...
                var matchingTest = allTestMethods.FirstOrDefault(t =>
                    t.StartsWith($"Test{method.Name}_", StringComparison.Ordinal) ||
                    t.StartsWith($"Test{method.Name}(", StringComparison.Ordinal) ||
                    t.Equals($"Test{method.Name}", StringComparison.Ordinal));

                if (matchingTest != null)
                {
                    method.HasTest = true;
                    method.TestName = matchingTest;
                }
            }
        }
    }

    private static CoverageReport BuildCoverageReport(List<SourceFileInfo> sourceFiles)
    {
        var allMethods = sourceFiles.SelectMany(sf => sf.Methods).ToList();
        var tested = allMethods.Where(m => m.HasTest).ToList();
        var untested = allMethods.Where(m => !m.HasTest).ToList();
        var totalMethods = allMethods.Count;

        return new CoverageReport
        {
            TotalMethods = totalMethods,
            TestedMethods = tested.Count,
            UntestedMethods = untested.Count,
            CoveragePercent = totalMethods > 0
                ? Math.Round(100.0 * tested.Count / totalMethods, 1)
                : 0,
            TestedMethodNames = tested.Select(m => m.Name).ToList(),
            UntestedMethodNames = untested.Select(m => m.Name).ToList()
        };
    }

    private static ProjectStatistics BuildStatistics(
        List<SourceFileInfo> sourceFiles,
        List<TestFileInfo> testFiles,
        CoverageReport coverage)
    {
        return new ProjectStatistics
        {
            TotalSourceFiles = sourceFiles.Count,
            TotalTestFiles = testFiles.Count,
            TotalClasses = sourceFiles.Count,
            TotalMethods = coverage.TotalMethods,
            TotalTests = testFiles.Sum(tf => tf.TestMethods.Count),
            TotalSourceLines = sourceFiles.Sum(sf => sf.LineCount),
            TotalTestLines = testFiles.Sum(tf =>
            {
                try { return File.ReadAllLines(tf.Path).Length; }
                catch { return 0; }
            }),
            CoveragePercent = coverage.CoveragePercent
        };
    }

    [GeneratedRegex(@"public\s+(?:static\s+)?(?:partial\s+)?class\s+(?<name>\w+)", RegexOptions.Compiled)]
    private static partial Regex ClassNameRegex();

    [GeneratedRegex(@"public\s+(?<static>static\s+)?(?:virtual\s+|override\s+|abstract\s+|new\s+)?(?<returnType>[\w<>\[\],\s\?]+?)\s+(?<methodName>\w+)\s*\(", RegexOptions.Compiled)]
    private static partial Regex MethodSignatureRegex();

    [GeneratedRegex(@"\[Test(?:Case)?\b[^\]]*\]\s*(?:\[.*?\]\s*)*public\s+\w+\s+(?<name>\w+)\s*\(", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex TestMethodRegex();
}
