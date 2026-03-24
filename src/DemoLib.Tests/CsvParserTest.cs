using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class CsvParserTest
{
	[Test]
	public void TestParse_WhenSimpleCsv_ThenParsesCorrectly()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name,Age\nAlice,30\nBob,25");
		Assert.That(doc.RowCount, Is.EqualTo(2));
		Assert.That(doc.Headers, Is.EqualTo(new[] { "Name", "Age" }));
		Assert.That(doc.Rows[0].Fields, Is.EqualTo(new[] { "Alice", "30" }));
		Assert.That(doc.Rows[1].Fields, Is.EqualTo(new[] { "Bob", "25" }));
	}

	[Test]
	public void TestParse_WhenQuotedFields_ThenHandlesQuotes()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name,City\n\"Alice\",\"New York\"");
		Assert.That(doc.Rows[0]["Name"], Is.EqualTo("Alice"));
		Assert.That(doc.Rows[0]["City"], Is.EqualTo("New York"));
	}

	[Test]
	public void TestParse_WhenEscapedQuotes_ThenDoubleQuotesUnescaped()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Value\n\"He said \"\"hello\"\"\"");
		Assert.That(doc.Rows[0].Fields[0], Is.EqualTo("He said \"hello\""));
	}

	[Test]
	public void TestParse_WhenMultilineField_ThenPreservesNewlines()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name,Bio\nAlice,\"Line 1\nLine 2\"");
		Assert.That(doc.Rows[0]["Bio"], Does.Contain("Line 1\nLine 2"));
	}

	[Test]
	public void TestParse_WhenCustomDelimiter_ThenUsesIt()
	{
		var parser = new CsvParser(delimiter: ';');
		var doc = parser.Parse("A;B\n1;2");
		Assert.That(doc.Rows[0].Fields, Is.EqualTo(new[] { "1", "2" }));
	}

	[Test]
	public void TestParse_WhenTrimFields_ThenTrimsWhitespace()
	{
		var parser = new CsvParser(trimFields: true);
		var doc = parser.Parse("A,B\n  hello , world  ");
		Assert.That(doc.Rows[0].Fields[0], Is.EqualTo("hello"));
		Assert.That(doc.Rows[0].Fields[1], Is.EqualTo("world"));
	}

	[Test]
	public void TestParse_WhenNoHeader_ThenNoHeaders()
	{
		var parser = new CsvParser(hasHeader: false);
		var doc = parser.Parse("Alice,30\nBob,25");
		Assert.That(doc.Headers, Is.Empty);
		Assert.That(doc.RowCount, Is.EqualTo(2));
		Assert.That(doc.Rows[0].Fields, Is.EqualTo(new[] { "Alice", "30" }));
	}

	[Test]
	public void TestParse_WhenEmptyCsv_ThenReturnsEmptyDocument()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("");
		Assert.That(doc.RowCount, Is.EqualTo(0));
	}

	[Test]
	public void TestParse_WhenNull_ThenThrowsArgumentNullException()
	{
		var parser = new CsvParser();
		Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
	}

	[Test]
	public void TestSerialize_WhenRoundTrip_ThenPreservesData()
	{
		var parser = new CsvParser();
		var original = "Name,Age\nAlice,30\nBob,25";
		var doc = parser.Parse(original);
		var serialized = parser.Serialize(doc);
		var reparsed = parser.Parse(serialized);
		Assert.That(reparsed.RowCount, Is.EqualTo(2));
		Assert.That(reparsed.Rows[0]["Name"], Is.EqualTo("Alice"));
		Assert.That(reparsed.Rows[1]["Age"], Is.EqualTo("25"));
	}

	[Test]
	public void TestSerialize_WhenFieldContainsDelimiter_ThenQuoted()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name,Value\nAlice,\"Hello, World\"");
		var serialized = parser.Serialize(doc);
		Assert.That(serialized, Does.Contain("\"Hello, World\""));
	}

	[Test]
	public void TestValidate_WhenConsistentColumns_ThenNoErrors()
	{
		var parser = new CsvParser();
		var errors = parser.Validate("A,B\n1,2\n3,4");
		Assert.That(errors, Is.Empty);
	}

	[Test]
	public void TestValidate_WhenInconsistentColumns_ThenReturnsErrors()
	{
		var parser = new CsvParser();
		var errors = parser.Validate("A,B\n1,2,3");
		Assert.That(errors, Has.Count.GreaterThan(0));
		Assert.That(errors[0], Does.Contain("Row 2"));
	}

	[Test]
	public void TestValidate_WhenEmptyCsv_ThenReturnsError()
	{
		var parser = new CsvParser();
		var errors = parser.Validate("");
		Assert.That(errors, Has.Count.GreaterThan(0));
	}

	[Test]
	public void TestGetColumn_WhenByName_ThenReturnsColumnValues()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name,Age\nAlice,30\nBob,25");
		var names = doc.GetColumn("Name");
		Assert.That(names, Is.EqualTo(new[] { "Alice", "Bob" }));
	}

	[Test]
	public void TestGetColumn_WhenByIndex_ThenReturnsColumnValues()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name,Age\nAlice,30\nBob,25");
		var ages = doc.GetColumn(1);
		Assert.That(ages, Is.EqualTo(new[] { "30", "25" }));
	}

	[Test]
	public void TestGetColumn_WhenInvalidName_ThenThrowsKeyNotFoundException()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name,Age\nAlice,30");
		Assert.Throws<KeyNotFoundException>(() => doc.GetColumn("Email"));
	}

	[Test]
	public void TestCsvRow_WhenIndexerByName_ThenReturnsFieldValue()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name,Age\nAlice,30");
		Assert.That(doc.Rows[0]["Name"], Is.EqualTo("Alice"));
	}

	[Test]
	public void TestCsvRow_WhenIndexerByIndex_ThenReturnsFieldValue()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name,Age\nAlice,30");
		Assert.That(doc.Rows[0][0], Is.EqualTo("Alice"));
		Assert.That(doc.Rows[0][1], Is.EqualTo("30"));
	}

	[Test]
	public void TestCsvRow_WhenIndexOutOfRange_ThenThrowsIndexOutOfRangeException()
	{
		var parser = new CsvParser();
		var doc = parser.Parse("Name\nAlice");
		Assert.Throws<IndexOutOfRangeException>(() => _ = doc.Rows[0][5]);
	}

	[Test]
	public void TestDelimiterProperty_WhenCreated_ThenReturnsConfiguredValue()
	{
		var parser = new CsvParser(delimiter: '\t');
		Assert.That(parser.Delimiter, Is.EqualTo('\t'));
	}
}
