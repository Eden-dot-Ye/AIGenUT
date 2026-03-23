# AI-Assisted Unit Test Generator Skill

## When to Activate

Activate this skill when the user asks to:

- Generate unit tests for a C# class
- Create test scaffolding for a source file
- Write test methods for specific functionality
- Review a class and suggest what tests are needed

## Step-by-Step Generation Process

When asked to generate tests for a C# source file, follow these steps **exactly**:

### Step 1: Analyze the Source Class

Read the source file and extract:

1. **Class name** and **namespace**
2. **All public methods** — name, parameters, return type
3. **All public properties** — name, type, whether they have setters
4. **Constructor parameters** — what dependencies are needed
5. **Special attributes** — any validation attributes, custom attributes
6. **Static vs instance** — whether the class is static or requires instantiation

### Step 2: Determine the Test Class Structure

Apply these rules to decide the test class structure:

| Source Class Type                   | Test Base Class                             | Notes                               |
| ----------------------------------- | ------------------------------------------- | ----------------------------------- |
| Static utility class                | No base class needed, use `[TestFixture]` | Direct method calls                 |
| Regular class with no dependencies  | No base class needed, use `[TestFixture]` | Create instance in test             |
| Class with constructor dependencies | Use `[TestFixture]` with `[SetUp]`      | Create mocks/stubs for dependencies |

### Step 3: Generate Test Methods

For **each public method**, generate test methods following this naming convention:

```
Test[MethodName]_When[Condition]_Then[ExpectedResult]
```

Generate tests for these categories:

#### A. Happy Path (Normal Cases)

- Method works correctly with valid inputs
- Expected return values are verified

#### B. Edge Cases

- Empty strings, zero values, boundary values
- Maximum and minimum values
- Collections with 0, 1, or many items

#### C. Error Cases

- Null parameters → expect `ArgumentNullException`
- Invalid ranges → expect `ArgumentOutOfRangeException`
- Invalid format → expect appropriate exception or false return

#### D. Special Cases

- Round-trip conversions (encode → decode should equal original)
- Idempotent operations (applying twice gives same result)

### Step 4: Apply Code Style Rules

**CRITICAL** — All generated tests MUST follow these rules:

1. **Test method names MUST start with "Test"**

   ```csharp
   // ✅ CORRECT
   public void TestCalculateWeight_WhenPositiveInput_ThenReturnsCorrectValue()

   // ❌ WRONG — missing "Test" prefix
   public void CalculateWeight_WhenPositiveInput_ThenReturnsCorrectValue()
   ```
2. **Use NUnit 4 assertions with the constraint model**

   ```csharp
   // ✅ CORRECT — NUnit 4 constraint model
   Assert.That(result, Is.EqualTo(expected));
   Assert.That(result, Is.True);
   Assert.That(result, Is.Not.Null);
   Assert.That(() => action(), Throws.TypeOf<ArgumentNullException>());

   // ❌ WRONG — classic model (deprecated)
   Assert.AreEqual(expected, result);
   Assert.IsTrue(result);
   ```
3. **Use `Assert.Throws<T>` for exception tests**

   ```csharp
   // ✅ CORRECT
   Assert.Throws<ArgumentNullException>(() => MyMethod(null));

   // Also acceptable:
   var ex = Assert.Throws<ArgumentException>(() => MyMethod("bad"));
   Assert.That(ex.Message, Does.Contain("invalid"));
   ```
4. **Each test method tests ONE behavior**

   ```csharp
   // ✅ CORRECT — one behavior per test
   [Test]
   public void TestConvert_WhenZeroInput_ThenReturnsZero()
   {
       Assert.That(Converter.Convert(0), Is.EqualTo(0));
   }

   // ❌ WRONG — testing multiple behaviors
   [Test]
   public void TestConvert_AllCases()
   {
       Assert.That(Converter.Convert(0), Is.EqualTo(0));
       Assert.That(Converter.Convert(1), Is.EqualTo(2.2));
       Assert.That(Converter.Convert(-1), Throws...);  // mixing concerns
   }
   ```
5. **Test class naming**: `[SourceClassName]Test`

   ```csharp
   // Source: WeightConverter.cs → Test: WeightConverterTest.cs
   // Source: StringHelper.cs   → Test: StringHelperTest.cs
   ```
6. **Test file location**: Mirror the source file structure in the test project

   ```
   src/DemoLib/WeightConverter.cs
   → src/DemoLib.Tests/WeightConverterTest.cs
   ```
7. **Use Arrange-Act-Assert pattern with comments when helpful**

   ```csharp
   [Test]
   public void TestCalculate_WhenValidDimensions_ThenReturnsVolumetricWeight()
   {
       // Arrange
       decimal length = 100m, width = 50m, height = 30m;

       // Act
       var result = WeightConverter.CalculateVolumetricWeight(length, width, height);

       // Assert
       Assert.That(result, Is.EqualTo(30m));
   }
   ```

### Step 5: Output the Complete Test File

Generate a complete, compilable test file with:

- Correct `using` statements
- `[TestFixture]` attribute on the class
- `[Test]` attribute on each test method
- Proper namespace matching the test project conventions

## Example: Full Test Generation

Given this source class:

```csharp
namespace DemoLib;

public static class WeightConverter
{
    public static decimal KgToLbs(decimal kg)
    {
        if (kg < 0) throw new ArgumentOutOfRangeException(nameof(kg));
        return Math.Round(kg * 2.20462m, 4);
    }
}
```

Generate this test class:

```csharp
using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class WeightConverterTest
{
    [Test]
    public void TestKgToLbs_WhenZero_ThenReturnsZero()
    {
        var result = WeightConverter.KgToLbs(0m);
        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void TestKgToLbs_WhenOneKg_ThenReturnsApprox2Point2Lbs()
    {
        var result = WeightConverter.KgToLbs(1m);
        Assert.That(result, Is.EqualTo(2.2046m));
    }

    [Test]
    public void TestKgToLbs_WhenLargeValue_ThenReturnsCorrectResult()
    {
        var result = WeightConverter.KgToLbs(1000m);
        Assert.That(result, Is.EqualTo(2204.62m));
    }

    [Test]
    public void TestKgToLbs_WhenNegative_ThenThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WeightConverter.KgToLbs(-1m));
    }
}
```

## Reference Files

See the `references/` directory for:

- `test-patterns.md` — Common test patterns and when to use them
- `assertion-reference.md` — Complete assertion syntax reference
