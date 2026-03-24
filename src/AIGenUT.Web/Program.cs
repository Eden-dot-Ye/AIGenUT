using System.Text.Json;
using AIGenUT.Web.Services;
using DemoLib;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CodeAnalysisService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

app.UseCors();
app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/index.html"));

app.MapGet("/api/analysis", (CodeAnalysisService svc) =>
    Results.Json(svc.AnalyzeProject(), jsonOptions));

app.MapGet("/api/coverage", (CodeAnalysisService svc) =>
    Results.Json(svc.GetCoverageReport(), jsonOptions));

app.MapGet("/api/source-files", (CodeAnalysisService svc) =>
    Results.Json(svc.GetSourceFiles(), jsonOptions));

app.MapGet("/api/test-files", (CodeAnalysisService svc) =>
    Results.Json(svc.GetTestFiles(), jsonOptions));

app.MapPost("/api/evaluate", (EvaluateRequest req) =>
{
    try
    {
        var evaluator = new ExpressionEvaluator();
        if (req.Variables != null)
        {
            foreach (var (name, value) in req.Variables)
                evaluator.SetVariable(name, value);
        }
        var result = evaluator.Evaluate(req.Expression);
        return Results.Json(new DemoResult { Success = true, Result = result.ToString("G") }, jsonOptions);
    }
    catch (Exception ex)
    {
        return Results.Json(new DemoResult { Success = false, Error = ex.Message }, jsonOptions);
    }
});

app.MapPost("/api/markdown", (MarkdownRequest req) =>
{
    try
    {
        var html = MarkdownParser.ToHtml(req.Markdown);
        return Results.Json(new DemoResult { Success = true, Result = html }, jsonOptions);
    }
    catch (Exception ex)
    {
        return Results.Json(new DemoResult { Success = false, Error = ex.Message }, jsonOptions);
    }
});

app.MapPost("/api/csv/parse", (CsvParseRequest req) =>
{
    try
    {
        var parser = new CsvParser(hasHeader: req.HasHeader);
        var doc = parser.Parse(req.Csv);
        var result = new
        {
            headers = doc.Headers,
            rowCount = doc.RowCount,
            columnCount = doc.ColumnCount,
            rows = doc.Rows.Select(row => new
            {
                rowIndex = row.RowIndex,
                fields = row.Fields
            }).ToList()
        };
        return Results.Json(result, jsonOptions);
    }
    catch (Exception ex)
    {
        return Results.Json(new DemoResult { Success = false, Error = ex.Message }, jsonOptions);
    }
});

app.MapGet("/api/demo/stats", (CodeAnalysisService svc) =>
{
    var analysis = svc.AnalyzeProject();
    var stats = new
    {
        projectName = "DemoLib",
        framework = "net8.0",
        testFramework = "NUnit",
        analysis.Statistics.TotalSourceFiles,
        analysis.Statistics.TotalTestFiles,
        analysis.Statistics.TotalClasses,
        analysis.Statistics.TotalMethods,
        analysis.Statistics.TotalTests,
        analysis.Statistics.TotalSourceLines,
        analysis.Statistics.TotalTestLines,
        analysis.Statistics.CoveragePercent
    };
    return Results.Json(stats, jsonOptions);
});

app.Run();
