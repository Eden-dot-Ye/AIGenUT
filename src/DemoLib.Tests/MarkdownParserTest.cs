using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class MarkdownParserTest
{
	[Test]
	public void TestToHtml_WhenH1Heading_ThenConvertsToH1Tag()
	{
		var result = MarkdownParser.ToHtml("# Hello");
		Assert.That(result, Does.Contain("<h1 id=\"hello\">Hello</h1>"));
	}

	[Test]
	public void TestToHtml_WhenH3Heading_ThenConvertsToH3Tag()
	{
		var result = MarkdownParser.ToHtml("### Level 3");
		Assert.That(result, Does.Contain("<h3 id=\"level-3\">Level 3</h3>"));
	}

	[Test]
	public void TestToHtml_WhenH6Heading_ThenConvertsToH6Tag()
	{
		var result = MarkdownParser.ToHtml("###### Deep");
		Assert.That(result, Does.Contain("<h6 id=\"deep\">Deep</h6>"));
	}

	[Test]
	public void TestToHtml_WhenBoldWithAsterisks_ThenConvertsToStrongTag()
	{
		var result = MarkdownParser.ToHtml("This is **bold** text");
		Assert.That(result, Does.Contain("<strong>bold</strong>"));
	}

	[Test]
	public void TestToHtml_WhenItalicWithAsterisks_ThenConvertsToEmTag()
	{
		var result = MarkdownParser.ToHtml("This is *italic* text");
		Assert.That(result, Does.Contain("<em>italic</em>"));
	}

	[Test]
	public void TestToHtml_WhenBoldItalic_ThenConvertsToStrongEmTags()
	{
		var result = MarkdownParser.ToHtml("This is ***bold italic*** text");
		Assert.That(result, Does.Contain("<strong><em>bold italic</em></strong>"));
	}

	[Test]
	public void TestToHtml_WhenInlineCode_ThenConvertsToCodeTag()
	{
		var result = MarkdownParser.ToHtml("Use `console.log` here");
		Assert.That(result, Does.Contain("<code>console.log</code>"));
	}

	[Test]
	public void TestToHtml_WhenLink_ThenConvertsToAnchorTag()
	{
		var result = MarkdownParser.ToHtml("[Google](https://google.com)");
		Assert.That(result, Does.Contain("<a href=\"https://google.com\">Google</a>"));
	}

	[Test]
	public void TestToHtml_WhenImage_ThenConvertsToImgTag()
	{
		var result = MarkdownParser.ToHtml("![Alt](image.png)");
		Assert.That(result, Does.Contain("<img src=\"image.png\" alt=\"Alt\" />"));
	}

	[Test]
	public void TestToHtml_WhenUnorderedList_ThenConvertsToUlLiTags()
	{
		var result = MarkdownParser.ToHtml("- Item 1\n- Item 2");
		Assert.That(result, Does.Contain("<ul>"));
		Assert.That(result, Does.Contain("<li>Item 1</li>"));
		Assert.That(result, Does.Contain("<li>Item 2</li>"));
		Assert.That(result, Does.Contain("</ul>"));
	}

	[Test]
	public void TestToHtml_WhenOrderedList_ThenConvertsToOlLiTags()
	{
		var result = MarkdownParser.ToHtml("1. First\n2. Second");
		Assert.That(result, Does.Contain("<ol>"));
		Assert.That(result, Does.Contain("<li>First</li>"));
		Assert.That(result, Does.Contain("<li>Second</li>"));
		Assert.That(result, Does.Contain("</ol>"));
	}

	[Test]
	public void TestToHtml_WhenBlockquote_ThenConvertsToBlockquoteTag()
	{
		var result = MarkdownParser.ToHtml("> Quote text");
		Assert.That(result, Does.Contain("<blockquote>"));
		Assert.That(result, Does.Contain("<p>Quote text</p>"));
		Assert.That(result, Does.Contain("</blockquote>"));
	}

	[Test]
	public void TestToHtml_WhenHorizontalRule_ThenConvertsToHrTag()
	{
		var result = MarkdownParser.ToHtml("---");
		Assert.That(result, Does.Contain("<hr />"));
	}

	[Test]
	public void TestToHtml_WhenHorizontalRuleWithAsterisks_ThenConvertsToHrTag()
	{
		var result = MarkdownParser.ToHtml("***");
		Assert.That(result, Does.Contain("<hr />"));
	}

	[Test]
	public void TestToHtml_WhenCodeBlock_ThenConvertsToPreCodeTags()
	{
		var markdown = "```csharp\nvar x = 1;\n```";
		var result = MarkdownParser.ToHtml(markdown);
		Assert.That(result, Does.Contain("<pre><code class=\"language-csharp\">"));
		Assert.That(result, Does.Contain("var x = 1;"));
		Assert.That(result, Does.Contain("</code></pre>"));
	}

	[Test]
	public void TestToHtml_WhenCodeBlockNoLanguage_ThenNoClassAttribute()
	{
		var markdown = "```\ncode here\n```";
		var result = MarkdownParser.ToHtml(markdown);
		Assert.That(result, Does.Contain("<pre><code>"));
	}

	[Test]
	public void TestToHtml_WhenParagraphText_ThenWrapsInPTag()
	{
		var result = MarkdownParser.ToHtml("Just a paragraph");
		Assert.That(result, Does.Contain("<p>Just a paragraph</p>"));
	}

	[Test]
	public void TestToHtml_WhenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => MarkdownParser.ToHtml(null!));
	}

	[Test]
	public void TestToHtml_WhenEmpty_ThenReturnsEmpty()
	{
		var result = MarkdownParser.ToHtml("");
		Assert.That(result, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestToHtml_WhenWhitespaceOnly_ThenReturnsEmpty()
	{
		var result = MarkdownParser.ToHtml("   ");
		Assert.That(result, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestEscapeHtml_WhenSpecialChars_ThenEscaped()
	{
		var result = MarkdownParser.EscapeHtml("<div class=\"test\">&</div>");
		Assert.That(result, Is.EqualTo("&lt;div class=&quot;test&quot;&gt;&amp;&lt;/div&gt;"));
	}

	[Test]
	public void TestEscapeHtml_WhenEmpty_ThenReturnsEmpty()
	{
		var result = MarkdownParser.EscapeHtml("");
		Assert.That(result, Is.EqualTo(""));
	}

	[Test]
	public void TestGenerateHeadingId_WhenSimpleText_ThenLowercaseWithDashes()
	{
		var result = MarkdownParser.GenerateHeadingId("Hello World");
		Assert.That(result, Is.EqualTo("hello-world"));
	}

	[Test]
	public void TestGenerateHeadingId_WhenSpecialChars_ThenRemoved()
	{
		var result = MarkdownParser.GenerateHeadingId("Hello, World!");
		Assert.That(result, Is.EqualTo("hello-world"));
	}

	[Test]
	public void TestGenerateHeadingId_WhenEmpty_ThenReturnsEmpty()
	{
		var result = MarkdownParser.GenerateHeadingId("");
		Assert.That(result, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestExtractTableOfContents_WhenMultipleHeadings_ThenAllExtracted()
	{
		var markdown = "# Title\n## Section\n### Subsection";
		var toc = MarkdownParser.ExtractTableOfContents(markdown);
		Assert.That(toc.Count, Is.EqualTo(3));
		Assert.That(toc[0].Level, Is.EqualTo(1));
		Assert.That(toc[0].Text, Is.EqualTo("Title"));
		Assert.That(toc[1].Level, Is.EqualTo(2));
		Assert.That(toc[2].Level, Is.EqualTo(3));
	}

	[Test]
	public void TestExtractTableOfContents_WhenNoHeadings_ThenReturnsEmpty()
	{
		var toc = MarkdownParser.ExtractTableOfContents("Just a paragraph");
		Assert.That(toc, Is.Empty);
	}

	[Test]
	public void TestCountWords_WhenSimpleText_ThenCountsCorrectly()
	{
		var count = MarkdownParser.CountWords("Hello world how are you");
		Assert.That(count, Is.EqualTo(5));
	}

	[Test]
	public void TestCountWords_WhenMarkdownWithFormatting_ThenCountsWords()
	{
		var count = MarkdownParser.CountWords("# Hello **world**");
		Assert.That(count, Is.GreaterThan(0));
	}

	[Test]
	public void TestCountWords_WhenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => MarkdownParser.CountWords(null!));
	}

	[Test]
	public void TestCountWords_WhenEmpty_ThenReturnsZero()
	{
		var count = MarkdownParser.CountWords("");
		Assert.That(count, Is.EqualTo(0));
	}

	[Test]
	public void TestToHtml_WhenStrikethrough_ThenConvertsToDelTag()
	{
		var result = MarkdownParser.ToHtml("This is ~~deleted~~ text");
		Assert.That(result, Does.Contain("<del>deleted</del>"));
	}
}
