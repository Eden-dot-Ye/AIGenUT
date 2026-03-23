# Test Patterns Reference

This document catalogs the most common unit test patterns used in this project.

---

## Pattern 1: Simple Return Value Test

**When to use:** Method takes input and returns a value.

```csharp
[Test]
public void TestMethodName_WhenValidInput_ThenReturnsExpectedValue()
{
    var result = TargetClass.Method(validInput);
    Assert.That(result, Is.EqualTo(expectedOutput));
}
```

---

## Pattern 2: Boolean Validation Test

**When to use:** Method returns true/false for validation.

```csharp
[Test]
public void TestIsValid_WhenCorrectFormat_ThenReturnsTrue()
{
    Assert.That(Validator.IsValid("ABCD1234567"), Is.True);
}

[Test]
public void TestIsValid_WhenIncorrectFormat_ThenReturnsFalse()
{
    Assert.That(Validator.IsValid("bad-input"), Is.False);
}
```

---

## Pattern 3: Exception Test

**When to use:** Method should throw on invalid input.

```csharp
[Test]
public void TestMethod_WhenNullInput_ThenThrowsArgumentNullException()
{
    Assert.Throws<ArgumentNullException>(() => TargetClass.Method(null!));
}

[Test]
public void TestMethod_WhenNegativeValue_ThenThrowsArgumentOutOfRangeException()
{
    Assert.Throws<ArgumentOutOfRangeException>(() => TargetClass.Method(-1));
}
```

---

## Pattern 4: Collection/Aggregate Test

**When to use:** Method processes a collection and returns an aggregate.

```csharp
[Test]
public void TestCalculateTotal_WhenMultipleItems_ThenReturnsSumOfPositives()
{
    var items = new List<decimal> { 10m, 20m, 30m };
    var result = Calculator.CalculateTotal(items);
    Assert.That(result, Is.EqualTo(60m));
}

[Test]
public void TestCalculateTotal_WhenNullList_ThenReturnsZero()
{
    var result = Calculator.CalculateTotal(null);
    Assert.That(result, Is.EqualTo(0m));
}

[Test]
public void TestCalculateTotal_WhenEmptyList_ThenReturnsZero()
{
    var result = Calculator.CalculateTotal(new List<decimal>());
    Assert.That(result, Is.EqualTo(0m));
}
```

---

## Pattern 5: String Transformation Test

**When to use:** Method transforms a string.

```csharp
[Test]
public void TestNormalize_WhenMixedCase_ThenReturnsUpperCase()
{
    var result = Formatter.Normalize("Hello World");
    Assert.That(result, Is.EqualTo("HELLOWORLD"));
}

[Test]
public void TestNormalize_WhenLeadingTrailingSpaces_ThenTrimmed()
{
    var result = Formatter.Normalize("  test  ");
    Assert.That(result, Is.EqualTo("TEST"));
}
```

---

## Pattern 6: Round-Trip / Inverse Test

**When to use:** Two methods are inverses of each other (encode/decode, serialize/deserialize).

```csharp
[Test]
public void TestRoundTrip_WhenEncodedThenDecoded_ThenMatchesOriginal()
{
    var original = "SGVsbG8gV29ybGQ=";
    var encoded = Encoder.Encode(original);
    var decoded = Encoder.Decode(encoded);
    Assert.That(decoded, Is.EqualTo(original));
}
```

---

## Pattern 7: Switch/Mapping Test

**When to use:** Method maps codes to descriptions or values.

```csharp
[Test]
[TestCase("PND", "Pending")]
[TestCase("CMP", "Completed")]
[TestCase("CAN", "Cancelled")]
public void TestGetDescription_WhenKnownCode_ThenReturnsCorrectDescription(string code, string expected)
{
    var result = Mapper.GetDescription(code);
    Assert.That(result, Is.EqualTo(expected));
}

[Test]
public void TestGetDescription_WhenUnknownCode_ThenReturnsUnknown()
{
    var result = Mapper.GetDescription("XYZ");
    Assert.That(result, Is.EqualTo("Unknown"));
}
```

---

## Pattern 8: Boundary Value Test

**When to use:** Method has boundaries that affect behavior.

```csharp
[Test]
public void TestTruncate_WhenExactlyAtMaxLength_ThenUnchanged()
{
    var input = "12345";
    var result = StringHelper.Truncate(input, 5);
    Assert.That(result, Is.EqualTo("12345"));
}

[Test]
public void TestTruncate_WhenOneOverMaxLength_ThenTruncated()
{
    var input = "123456";
    var result = StringHelper.Truncate(input, 5);
    Assert.That(result, Is.EqualTo("12345"));
}
```
