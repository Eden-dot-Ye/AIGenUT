namespace AIGenUT.Web.Services;

public class ProjectAnalysis
{
    public List<SourceFileInfo> SourceFiles { get; set; } = [];
    public List<TestFileInfo> TestFiles { get; set; } = [];
    public CoverageReport Coverage { get; set; } = new();
    public ProjectStatistics Statistics { get; set; } = new();
}

public class SourceFileInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public List<MethodInfo> Methods { get; set; } = [];
    public int LineCount { get; set; }
}

public class MethodInfo
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public bool HasTest { get; set; }
    public string? TestName { get; set; }
}

public class TestFileInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public List<string> TestMethods { get; set; } = [];
}

public class CoverageReport
{
    public int TotalMethods { get; set; }
    public int TestedMethods { get; set; }
    public int UntestedMethods { get; set; }
    public double CoveragePercent { get; set; }
    public List<string> TestedMethodNames { get; set; } = [];
    public List<string> UntestedMethodNames { get; set; } = [];
}

public class ProjectStatistics
{
    public int TotalSourceFiles { get; set; }
    public int TotalTestFiles { get; set; }
    public int TotalClasses { get; set; }
    public int TotalMethods { get; set; }
    public int TotalTests { get; set; }
    public int TotalSourceLines { get; set; }
    public int TotalTestLines { get; set; }
    public double CoveragePercent { get; set; }
}

public class DemoResult
{
    public bool Success { get; set; }
    public string? Result { get; set; }
    public string? Error { get; set; }
}

public class EvaluateRequest
{
    public string Expression { get; set; } = string.Empty;
    public Dictionary<string, double>? Variables { get; set; }
}

public class MarkdownRequest
{
    public string Markdown { get; set; } = string.Empty;
}

public class CsvParseRequest
{
    public string Csv { get; set; } = string.Empty;
    public bool HasHeader { get; set; } = true;
}
