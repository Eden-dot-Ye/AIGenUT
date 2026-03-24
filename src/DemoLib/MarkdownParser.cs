using System.Text;
using System.Text.RegularExpressions;

namespace DemoLib;

/// <summary>
/// A lightweight Markdown to HTML converter supporting common Markdown elements.
/// Supports headings, bold, italic, code, links, images, lists, blockquotes,
/// horizontal rules, and code blocks.
/// </summary>
public static partial class MarkdownParser
{
    /// <summary>
    /// Converts a Markdown string to HTML.
    /// </summary>
    public static string ToHtml(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var html = new StringBuilder();
        var inCodeBlock = false;
        var inUnorderedList = false;
        var inOrderedList = false;
        var inBlockquote = false;
        var codeBlockContent = new StringBuilder();
        var codeBlockLanguage = string.Empty;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Code blocks (fenced)
            if (line.TrimStart().StartsWith("```"))
            {
                if (!inCodeBlock)
                {
                    CloseOpenLists(html, ref inUnorderedList, ref inOrderedList);
                    CloseBlockquote(html, ref inBlockquote);
                    inCodeBlock = true;
                    codeBlockLanguage = line.TrimStart()[3..].Trim();
                    codeBlockContent.Clear();
                }
                else
                {
                    var langAttr = string.IsNullOrEmpty(codeBlockLanguage)
                        ? ""
                        : $" class=\"language-{EscapeHtml(codeBlockLanguage)}\"";
                    html.AppendLine($"<pre><code{langAttr}>{EscapeHtml(codeBlockContent.ToString().TrimEnd())}</code></pre>");
                    inCodeBlock = false;
                }
                continue;
            }

            if (inCodeBlock)
            {
                codeBlockContent.AppendLine(line);
                continue;
            }

            // Empty line
            if (string.IsNullOrWhiteSpace(line))
            {
                CloseOpenLists(html, ref inUnorderedList, ref inOrderedList);
                CloseBlockquote(html, ref inBlockquote);
                continue;
            }

            // Horizontal rule
            if (IsHorizontalRule(line))
            {
                CloseOpenLists(html, ref inUnorderedList, ref inOrderedList);
                CloseBlockquote(html, ref inBlockquote);
                html.AppendLine("<hr />");
                continue;
            }

            // Headings
            var headingMatch = HeadingRegex().Match(line);
            if (headingMatch.Success)
            {
                CloseOpenLists(html, ref inUnorderedList, ref inOrderedList);
                CloseBlockquote(html, ref inBlockquote);
                var level = headingMatch.Groups[1].Value.Length;
                var text = ProcessInlineElements(headingMatch.Groups[2].Value.Trim());
                var id = GenerateHeadingId(headingMatch.Groups[2].Value.Trim());
                html.AppendLine($"<h{level} id=\"{id}\">{text}</h{level}>");
                continue;
            }

            // Blockquote
            if (line.TrimStart().StartsWith('>'))
            {
                CloseOpenLists(html, ref inUnorderedList, ref inOrderedList);
                if (!inBlockquote)
                {
                    html.AppendLine("<blockquote>");
                    inBlockquote = true;
                }
                var quoteContent = line.TrimStart()[1..].TrimStart();
                html.AppendLine($"<p>{ProcessInlineElements(quoteContent)}</p>");
                continue;
            }

            // Unordered list
            var ulMatch = UnorderedListRegex().Match(line);
            if (ulMatch.Success)
            {
                CloseBlockquote(html, ref inBlockquote);
                if (inOrderedList)
                    CloseOpenLists(html, ref inUnorderedList, ref inOrderedList);
                if (!inUnorderedList)
                {
                    html.AppendLine("<ul>");
                    inUnorderedList = true;
                }
                html.AppendLine($"<li>{ProcessInlineElements(ulMatch.Groups[1].Value)}</li>");
                continue;
            }

            // Ordered list
            var olMatch = OrderedListRegex().Match(line);
            if (olMatch.Success)
            {
                CloseBlockquote(html, ref inBlockquote);
                if (inUnorderedList)
                    CloseOpenLists(html, ref inUnorderedList, ref inOrderedList);
                if (!inOrderedList)
                {
                    html.AppendLine("<ol>");
                    inOrderedList = true;
                }
                html.AppendLine($"<li>{ProcessInlineElements(olMatch.Groups[1].Value)}</li>");
                continue;
            }

            // Regular paragraph
            CloseOpenLists(html, ref inUnorderedList, ref inOrderedList);
            CloseBlockquote(html, ref inBlockquote);
            html.AppendLine($"<p>{ProcessInlineElements(line)}</p>");
        }

        // Close any open blocks
        CloseOpenLists(html, ref inUnorderedList, ref inOrderedList);
        CloseBlockquote(html, ref inBlockquote);

        return html.ToString().Trim();
    }

    /// <summary>
    /// Processes inline Markdown elements: bold, italic, code, links, images.
    /// </summary>
    public static string ProcessInlineElements(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Inline code (must be first to prevent inner processing)
        text = InlineCodeRegex().Replace(text, m => $"<code>{EscapeHtml(m.Groups[1].Value)}</code>");

        // Images (before links, since images use similar syntax)
        text = ImageRegex().Replace(text, "<img src=\"$2\" alt=\"$1\" />");

        // Links
        text = LinkRegex().Replace(text, "<a href=\"$2\">$1</a>");

        // Bold+Italic (***text*** or ___text___)
        text = BoldItalicRegex().Replace(text, "<strong><em>$1</em></strong>");

        // Bold (**text** or __text__)
        text = BoldRegex().Replace(text, "<strong>$1</strong>");

        // Italic (*text* or _text_)
        text = ItalicRegex().Replace(text, "<em>$1</em>");

        // Strikethrough (~~text~~)
        text = StrikethroughRegex().Replace(text, "<del>$1</del>");

        return text;
    }

    /// <summary>
    /// Escapes HTML special characters.
    /// </summary>
    public static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    /// <summary>
    /// Generates a URL-friendly ID from heading text.
    /// </summary>
    public static string GenerateHeadingId(string headingText)
    {
        if (string.IsNullOrWhiteSpace(headingText))
            return string.Empty;

        return HeadingIdCleanupRegex().Replace(
            headingText.ToLowerInvariant().Replace(' ', '-'),
            ""
        ).Trim('-');
    }

    /// <summary>
    /// Extracts all headings from a Markdown document as a table of contents.
    /// </summary>
    public static List<(int Level, string Text, string Id)> ExtractTableOfContents(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        var toc = new List<(int Level, string Text, string Id)>();
        var lines = markdown.Replace("\r\n", "\n").Split('\n');

        foreach (var line in lines)
        {
            var match = HeadingRegex().Match(line);
            if (match.Success)
            {
                var level = match.Groups[1].Value.Length;
                var text = match.Groups[2].Value.Trim();
                var id = GenerateHeadingId(text);
                toc.Add((level, text, id));
            }
        }

        return toc;
    }

    /// <summary>
    /// Counts words in a Markdown document (excluding markup).
    /// </summary>
    public static int CountWords(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        if (string.IsNullOrWhiteSpace(markdown))
            return 0;

        // Strip markdown syntax
        var text = StripMarkdownRegex().Replace(markdown, " ");
        text = MultiSpaceRegex().Replace(text, " ").Trim();

        return string.IsNullOrWhiteSpace(text) ? 0 : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static bool IsHorizontalRule(string line)
    {
        var trimmed = line.Trim();
        return HorizontalRuleRegex().IsMatch(trimmed);
    }

    private static void CloseOpenLists(StringBuilder html, ref bool inUnorderedList, ref bool inOrderedList)
    {
        if (inUnorderedList) { html.AppendLine("</ul>"); inUnorderedList = false; }
        if (inOrderedList) { html.AppendLine("</ol>"); inOrderedList = false; }
    }

    private static void CloseBlockquote(StringBuilder html, ref bool inBlockquote)
    {
        if (inBlockquote) { html.AppendLine("</blockquote>"); inBlockquote = false; }
    }

    [GeneratedRegex(@"^(#{1,6})\s+(.+)$")]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"^[-*+]\s+(.+)$")]
    private static partial Regex UnorderedListRegex();

    [GeneratedRegex(@"^\d+\.\s+(.+)$")]
    private static partial Regex OrderedListRegex();

    [GeneratedRegex(@"`([^`]+)`")]
    private static partial Regex InlineCodeRegex();

    [GeneratedRegex(@"!\[([^\]]*)\]\(([^)]+)\)")]
    private static partial Regex ImageRegex();

    [GeneratedRegex(@"\[([^\]]+)\]\(([^)]+)\)")]
    private static partial Regex LinkRegex();

    [GeneratedRegex(@"\*\*\*(.+?)\*\*\*|___(.+?)___")]
    private static partial Regex BoldItalicRegex();

    [GeneratedRegex(@"\*\*(.+?)\*\*|__(.+?)__")]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)|(?<!_)_(?!_)(.+?)(?<!_)_(?!_)")]
    private static partial Regex ItalicRegex();

    [GeneratedRegex(@"~~(.+?)~~")]
    private static partial Regex StrikethroughRegex();

    [GeneratedRegex(@"^[-*_]{3,}$")]
    private static partial Regex HorizontalRuleRegex();

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex HeadingIdCleanupRegex();

    [GeneratedRegex(@"[#*_`\[\]()!~>|]")]
    private static partial Regex StripMarkdownRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultiSpaceRegex();
}
