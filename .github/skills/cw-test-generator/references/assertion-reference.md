# NUnit 4 Assertion Reference

This project uses **NUnit 4** with the **constraint-based assertion model**.

---

## Basic Assertions

```csharp
// Equality
Assert.That(actual, Is.EqualTo(expected));
Assert.That(actual, Is.Not.EqualTo(unexpected));

// Boolean
Assert.That(condition, Is.True);
Assert.That(condition, Is.False);

// Null
Assert.That(obj, Is.Null);
Assert.That(obj, Is.Not.Null);

// Same reference
Assert.That(actual, Is.SameAs(expected));
```

## Numeric Assertions

```csharp
// Comparison
Assert.That(value, Is.GreaterThan(0));
Assert.That(value, Is.LessThanOrEqualTo(100));
Assert.That(value, Is.InRange(1, 10));

// Approximate equality (for floating point)
Assert.That(value, Is.EqualTo(3.14).Within(0.01));
```

## String Assertions

```csharp
Assert.That(str, Is.EqualTo("expected"));
Assert.That(str, Does.Contain("substring"));
Assert.That(str, Does.StartWith("prefix"));
Assert.That(str, Does.EndWith("suffix"));
Assert.That(str, Is.Empty);
Assert.That(str, Is.Not.Empty);
Assert.That(str, Does.Match(@"^\d+$"));  // regex
```

## Collection Assertions

```csharp
Assert.That(list, Is.Empty);
Assert.That(list, Is.Not.Empty);
Assert.That(list, Has.Count.EqualTo(3));
Assert.That(list, Does.Contain(item));
Assert.That(list, Is.All.GreaterThan(0));
Assert.That(list, Is.Ordered);
Assert.That(list, Is.Unique);
```

## Exception Assertions

```csharp
// Basic exception check
Assert.Throws<ArgumentNullException>(() => Method(null));

// Capture exception for further assertions
var ex = Assert.Throws<ArgumentException>(() => Method("bad"));
Assert.That(ex.Message, Does.Contain("invalid"));
Assert.That(ex.ParamName, Is.EqualTo("input"));

// Async exception check
Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncMethod());
```

## Type Assertions

```csharp
Assert.That(obj, Is.TypeOf<ExpectedType>());
Assert.That(obj, Is.InstanceOf<BaseType>());
Assert.That(obj, Is.AssignableTo<InterfaceType>());
```

---

## ❌ DO NOT USE (Deprecated Classic Model)

```csharp
// These are from NUnit 3 classic model — DO NOT USE
Assert.AreEqual(expected, actual);     // ❌
Assert.IsTrue(condition);              // ❌
Assert.IsNull(obj);                    // ❌
Assert.IsNotNull(obj);                 // ❌
Assert.Greater(a, b);                  // ❌
```

Always use the constraint-based model (`Assert.That(...)`) instead.
