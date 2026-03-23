# AIGenUT — AI-Assisted Unit Test Generation Demo

A demonstration project showcasing two AI-powered approaches to improve unit test development:

1. **GitHub Copilot Skill** — generates unit tests following project conventions
2. **GitHub Actions Workflow** — automatically reviews test coverage on every PR

## Project Structure

```
AIGenUT/
├── .github/
│   ├── skills/
│   │   └── cw-test-generator/         # Copilot Skill for test generation
│   │       ├── SKILL.md               # Main skill definition
│   │       └── references/
│   │           ├── test-patterns.md    # Common test patterns
│   │           └── assertion-reference.md
│   └── workflows/
│       └── ai-test-review.yml         # PR test coverage checker
├── src/
│   ├── DemoLib/                        # Source code (demo library)
│   │   ├── StringHelper.cs            # String utility methods
│   │   ├── ShipmentValidator.cs       # Shipment data validation
│   │   └── WeightConverter.cs         # Weight unit conversions
│   └── DemoLib.Tests/                  # Unit tests
│       └── StringHelperTest.cs         # Example: tests for StringHelper
├── AIGenUT.sln                         # Solution file
└── README.md                           # This file
```

## Quick Start

```bash
# Build
dotnet build

# Run tests
dotnet test
```

## How It Works

See the [Setup Guide](SETUP-GUIDE.md) for complete instructions.
