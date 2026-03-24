# AIGenUT — AI-Assisted Unit Test Generation Demo

A comprehensive demonstration project showcasing AI-powered approaches to improve unit test development, featuring a modern web dashboard for visualization and interactive demos.

## Demo

https://github.com/user-attachments/assets/7676eb4c-0249-46e2-8f94-e5477426be00

## Features

- 🤖 **GitHub Copilot Skill** — generates unit tests following project conventions
- 🔄 **GitHub Actions Workflow** — automatically reviews test coverage on every PR
- 📊 **Web Dashboard** — modern visualization of code analysis, test coverage, and interactive demos
- 🧪 **216 Unit Tests** — comprehensive test suite covering 9 source classes with 82.8% method coverage
- 🎮 **Interactive Playground** — test Expression Evaluator, Markdown Parser, and CSV Parser in the browser

## Project Structure

```
AIGenUT/
├── .github/
│   └── workflows/
│       └── ai-test-review.yml             # PR test coverage checker
├── skills/
│   └── cw-test-generator/                 # Copilot Skill for test generation
│       ├── SKILL.md                       # Main skill definition
│       └── references/
│           ├── test-patterns.md           # Common test patterns
│           └── assertion-reference.md     # NUnit 4 assertion reference
├── src/
│   ├── DemoLib/                           # Source library (9 classes)
│   │   ├── StringHelper.cs               # String utility methods
│   │   ├── ShipmentValidator.cs          # Shipment data validation (ISO 6346, HS codes)
│   │   ├── WeightConverter.cs            # Weight unit conversions
│   │   ├── ExpressionEvaluator.cs        # Math expression parser with variables & functions
│   │   ├── OrderProcessor.cs             # Order state machine with business rules
│   │   ├── DataPipeline.cs               # Fluent data transformation pipeline & statistics
│   │   ├── MarkdownParser.cs             # Markdown to HTML converter
│   │   ├── RateLimiter.cs                # Token bucket & sliding window rate limiters
│   │   └── CsvParser.cs                  # RFC 4180 CSV parser
│   ├── DemoLib.Tests/                     # Unit tests (216 tests)
│   │   ├── StringHelperTest.cs           # 14 tests
│   │   ├── ShipmentValidatorTest.cs      # 25 tests
│   │   ├── WeightConverterTest.cs        # 17 tests
│   │   ├── ExpressionEvaluatorTest.cs    # 33 tests
│   │   ├── OrderProcessorTest.cs         # 27 tests
│   │   ├── DataPipelineTest.cs           # 21 tests
│   │   ├── MarkdownParserTest.cs         # 33 tests
│   │   ├── RateLimiterTest.cs            # 23 tests
│   │   └── CsvParserTest.cs             # 21 tests
│   └── AIGenUT.Web/                       # Web dashboard
│       ├── Program.cs                     # API endpoints
│       ├── Services/                      # Code analysis service
│       └── wwwroot/                       # Modern SPA frontend
├── AIGenUT.sln                            # Solution file
└── README.md
```

## Quick Start

```bash
# Build all projects
dotnet build

# Run all 216 tests
dotnet test

# Launch the web dashboard
dotnet run --project src/AIGenUT.Web
# Then open http://localhost:5000 in your browser
```

## Web Dashboard

The interactive dashboard provides:

| Tab                  | Description                                                                           |
| -------------------- | ------------------------------------------------------------------------------------- |
| **Dashboard**  | Overview metrics, coverage donut chart, methods per class bar chart, LOC distribution |
| **Coverage**   | Method-level coverage map with green/red indicators per class                         |
| **Playground** | Interactive Expression Evaluator, Markdown Converter, and CSV Parser                  |
| **Source**     | File browser with method details and test mapping                                     |
| **Tests**      | Complete test suite overview with 216 tests across 9 test classes                     |

## Tech Stack

| Component   | Technology                                |
| ----------- | ----------------------------------------- |
| Language    | C# 12 / .NET 8                            |
| Testing     | NUnit 4.3.2 (constraint-based assertions) |
| Web Backend | ASP.NET Core Minimal API                  |
| Frontend    | Tailwind CSS + Alpine.js + Chart.js       |
| CI/CD       | GitHub Actions                            |
| AI          | GitHub Copilot Skill                      |
