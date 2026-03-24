using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class ExpressionEvaluatorTest
{
	private ExpressionEvaluator _evaluator = null!;

	[SetUp]
	public void SetUp()
	{
		_evaluator = new ExpressionEvaluator();
	}

	[Test]
	public void TestEvaluate_WhenAddition_ThenReturnsSum()
	{
		var result = _evaluator.Evaluate("2 + 3");
		Assert.That(result, Is.EqualTo(5.0));
	}

	[Test]
	public void TestEvaluate_WhenSubtraction_ThenReturnsDifference()
	{
		var result = _evaluator.Evaluate("10 - 4");
		Assert.That(result, Is.EqualTo(6.0));
	}

	[Test]
	public void TestEvaluate_WhenMultiplication_ThenReturnsProduct()
	{
		var result = _evaluator.Evaluate("3 * 7");
		Assert.That(result, Is.EqualTo(21.0));
	}

	[Test]
	public void TestEvaluate_WhenDivision_ThenReturnsQuotient()
	{
		var result = _evaluator.Evaluate("20 / 4");
		Assert.That(result, Is.EqualTo(5.0));
	}

	[Test]
	public void TestEvaluate_WhenModulo_ThenReturnsRemainder()
	{
		var result = _evaluator.Evaluate("10 % 3");
		Assert.That(result, Is.EqualTo(1.0));
	}

	[Test]
	public void TestEvaluate_WhenOperatorPrecedence_ThenMultiplicationFirst()
	{
		var result = _evaluator.Evaluate("2 + 3 * 4");
		Assert.That(result, Is.EqualTo(14.0));
	}

	[Test]
	public void TestEvaluate_WhenParentheses_ThenOverridesPrecedence()
	{
		var result = _evaluator.Evaluate("(2 + 3) * 4");
		Assert.That(result, Is.EqualTo(20.0));
	}

	[Test]
	public void TestEvaluate_WhenNestedParentheses_ThenEvaluatesCorrectly()
	{
		var result = _evaluator.Evaluate("((2 + 3) * (4 - 1))");
		Assert.That(result, Is.EqualTo(15.0));
	}

	[Test]
	public void TestEvaluate_WhenPowerOperator_ThenCalculatesExponent()
	{
		var result = _evaluator.Evaluate("2 ^ 10");
		Assert.That(result, Is.EqualTo(1024.0));
	}

	[Test]
	public void TestEvaluate_WhenPowerRightAssociative_ThenEvaluatesCorrectly()
	{
		// 2 ^ 3 ^ 2 = 2 ^ (3 ^ 2) = 2 ^ 9 = 512
		var result = _evaluator.Evaluate("2 ^ 3 ^ 2");
		Assert.That(result, Is.EqualTo(512.0));
	}

	[Test]
	public void TestEvaluate_WhenUnaryMinus_ThenNegatesValue()
	{
		var result = _evaluator.Evaluate("-5");
		Assert.That(result, Is.EqualTo(-5.0));
	}

	[Test]
	public void TestEvaluate_WhenUnaryMinusInExpression_ThenNegatesCorrectly()
	{
		var result = _evaluator.Evaluate("3 + -2");
		Assert.That(result, Is.EqualTo(1.0));
	}

	[Test]
	public void TestEvaluate_WhenVariableSet_ThenUsesVariable()
	{
		_evaluator.SetVariable("x", 10);
		var result = _evaluator.Evaluate("x + 5");
		Assert.That(result, Is.EqualTo(15.0));
	}

	[Test]
	public void TestEvaluate_WhenMultipleVariables_ThenUsesAll()
	{
		_evaluator.SetVariable("a", 3);
		_evaluator.SetVariable("b", 4);
		var result = _evaluator.Evaluate("a * b");
		Assert.That(result, Is.EqualTo(12.0));
	}

	[Test]
	public void TestEvaluate_WhenUndefinedVariable_ThenThrowsFormatException()
	{
		Assert.Throws<FormatException>(() => _evaluator.Evaluate("x + 1"));
	}

	[Test]
	public void TestEvaluate_WhenAbsFunction_ThenReturnsAbsoluteValue()
	{
		var result = _evaluator.Evaluate("abs(-7)");
		Assert.That(result, Is.EqualTo(7.0));
	}

	[Test]
	public void TestEvaluate_WhenSqrtFunction_ThenReturnsSquareRoot()
	{
		var result = _evaluator.Evaluate("sqrt(16)");
		Assert.That(result, Is.EqualTo(4.0));
	}

	[Test]
	public void TestEvaluate_WhenSqrtNegative_ThenThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => _evaluator.Evaluate("sqrt(-1)"));
	}

	[Test]
	public void TestEvaluate_WhenMaxFunction_ThenReturnsLargest()
	{
		var result = _evaluator.Evaluate("max(3, 7, 2)");
		Assert.That(result, Is.EqualTo(7.0));
	}

	[Test]
	public void TestEvaluate_WhenMinFunction_ThenReturnsSmallest()
	{
		var result = _evaluator.Evaluate("min(5, 2, 8)");
		Assert.That(result, Is.EqualTo(2.0));
	}

	[Test]
	public void TestEvaluate_WhenRoundFunction_ThenRoundsValue()
	{
		var result = _evaluator.Evaluate("round(3.7)");
		Assert.That(result, Is.EqualTo(4.0));
	}

	[Test]
	public void TestEvaluate_WhenRoundWithPrecision_ThenRoundsToDecimals()
	{
		var result = _evaluator.Evaluate("round(3.456, 2)");
		Assert.That(result, Is.EqualTo(3.46));
	}

	[Test]
	public void TestEvaluate_WhenPIConstant_ThenReturnsPi()
	{
		var result = _evaluator.Evaluate("PI");
		Assert.That(result, Is.EqualTo(Math.PI));
	}

	[Test]
	public void TestEvaluate_WhenEConstant_ThenReturnsE()
	{
		var result = _evaluator.Evaluate("E");
		Assert.That(result, Is.EqualTo(Math.E));
	}

	[Test]
	public void TestEvaluate_WhenDivisionByZero_ThenThrowsDivideByZeroException()
	{
		Assert.Throws<DivideByZeroException>(() => _evaluator.Evaluate("1 / 0"));
	}

	[Test]
	public void TestEvaluate_WhenModuloByZero_ThenThrowsDivideByZeroException()
	{
		Assert.Throws<DivideByZeroException>(() => _evaluator.Evaluate("10 % 0"));
	}

	[Test]
	public void TestEvaluate_WhenEmptyExpression_ThenThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(""));
	}

	[Test]
	public void TestEvaluate_WhenNullExpression_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _evaluator.Evaluate(null!));
	}

	[Test]
	public void TestEvaluate_WhenInvalidCharacter_ThenThrowsFormatException()
	{
		Assert.Throws<FormatException>(() => _evaluator.Evaluate("2 & 3"));
	}

	[Test]
	public void TestTryEvaluate_WhenValidExpression_ThenReturnsTrueAndResult()
	{
		var success = _evaluator.TryEvaluate("2 + 3", out var result);
		Assert.That(success, Is.True);
		Assert.That(result, Is.EqualTo(5.0));
	}

	[Test]
	public void TestTryEvaluate_WhenInvalidExpression_ThenReturnsFalse()
	{
		var success = _evaluator.TryEvaluate("2 +", out _);
		Assert.That(success, Is.False);
	}

	[Test]
	public void TestSetVariable_WhenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _evaluator.SetVariable(null!, 5));
	}

	[Test]
	public void TestGetVariable_WhenSet_ThenReturnsValue()
	{
		_evaluator.SetVariable("x", 42);
		Assert.That(_evaluator.GetVariable("x"), Is.EqualTo(42));
	}

	[Test]
	public void TestClearVariables_WhenCalled_ThenVariablesCleared()
	{
		_evaluator.SetVariable("x", 10);
		_evaluator.ClearVariables();
		Assert.Throws<KeyNotFoundException>(() => _evaluator.GetVariable("x"));
	}
}
