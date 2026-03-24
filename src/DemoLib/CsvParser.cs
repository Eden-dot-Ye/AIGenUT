using System.Text;

namespace DemoLib;

/// <summary>
/// A comprehensive CSV parser that handles quoted fields, escaped characters,
/// multiline fields, custom delimiters, and various edge cases per RFC 4180.
/// </summary>
public class CsvParser
{
    private readonly char _delimiter;
    private readonly char _quoteChar;
    private readonly bool _hasHeader;
    private readonly bool _trimFields;

    /// <summary>
    /// Gets the delimiter character used for parsing.
    /// </summary>
    public char Delimiter => _delimiter;

    /// <summary>
    /// Gets whether the CSV has a header row.
    /// </summary>
    public bool HasHeader => _hasHeader;

    /// <summary>
    /// Creates a new CSV parser with the specified options.
    /// </summary>
    /// <param name="delimiter">Field delimiter (default: comma).</param>
    /// <param name="quoteChar">Quote character for enclosing fields (default: double quote).</param>
    /// <param name="hasHeader">Whether the first row is a header row.</param>
    /// <param name="trimFields">Whether to trim whitespace from field values.</param>
    public CsvParser(char delimiter = ',', char quoteChar = '"', bool hasHeader = true, bool trimFields = false)
    {
        _delimiter = delimiter;
        _quoteChar = quoteChar;
        _hasHeader = hasHeader;
        _trimFields = trimFields;
    }

    /// <summary>
    /// Parses a CSV string into a CsvDocument.
    /// </summary>
    public CsvDocument Parse(string csv)
    {
        ArgumentNullException.ThrowIfNull(csv);
        if (string.IsNullOrWhiteSpace(csv))
            return new CsvDocument(Array.Empty<string>(), new List<CsvRow>());

        var rows = ParseRows(csv);

        string[] headers;
        List<CsvRow> dataRows;

        if (_hasHeader && rows.Count > 0)
        {
            headers = rows[0];
            dataRows = rows.Skip(1).Select((fields, index) => new CsvRow(index, fields, headers)).ToList();
        }
        else
        {
            headers = Array.Empty<string>();
            dataRows = rows.Select((fields, index) => new CsvRow(index, fields, null)).ToList();
        }

        return new CsvDocument(headers, dataRows);
    }

    /// <summary>
    /// Parses CSV content into a list of string arrays (raw rows).
    /// </summary>
    public List<string[]> ParseRows(string csv)
    {
        ArgumentNullException.ThrowIfNull(csv);
        var rows = new List<string[]>();
        var currentRow = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;
        var i = 0;

        while (i < csv.Length)
        {
            var ch = csv[i];

            if (inQuotes)
            {
                if (ch == _quoteChar)
                {
                    // Check for escaped quote (double quote)
                    if (i + 1 < csv.Length && csv[i + 1] == _quoteChar)
                    {
                        currentField.Append(_quoteChar);
                        i += 2;
                        continue;
                    }
                    inQuotes = false;
                    i++;
                    continue;
                }
                currentField.Append(ch);
                i++;
            }
            else
            {
                if (ch == _quoteChar)
                {
                    inQuotes = true;
                    i++;
                }
                else if (ch == _delimiter)
                {
                    AddField(currentRow, currentField);
                    i++;
                }
                else if (ch == '\r')
                {
                    AddField(currentRow, currentField);
                    rows.Add(currentRow.ToArray());
                    currentRow = new List<string>();
                    i++;
                    if (i < csv.Length && csv[i] == '\n')
                        i++;
                }
                else if (ch == '\n')
                {
                    AddField(currentRow, currentField);
                    rows.Add(currentRow.ToArray());
                    currentRow = new List<string>();
                    i++;
                }
                else
                {
                    currentField.Append(ch);
                    i++;
                }
            }
        }

        // Handle last field and row
        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            AddField(currentRow, currentField);
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }

    /// <summary>
    /// Converts a CsvDocument back to a CSV string.
    /// </summary>
    public string Serialize(CsvDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var sb = new StringBuilder();

        if (document.Headers.Length > 0)
        {
            sb.AppendLine(string.Join(_delimiter, document.Headers.Select(QuoteFieldIfNeeded)));
        }

        foreach (var row in document.Rows)
        {
            sb.AppendLine(string.Join(_delimiter, row.Fields.Select(QuoteFieldIfNeeded)));
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Validates that all rows have the same number of fields.
    /// Returns a list of validation errors.
    /// </summary>
    public List<string> Validate(string csv)
    {
        ArgumentNullException.ThrowIfNull(csv);
        var errors = new List<string>();
        var rows = ParseRows(csv);

        if (rows.Count == 0)
        {
            errors.Add("CSV is empty.");
            return errors;
        }

        var expectedColumns = rows[0].Length;
        for (var i = 1; i < rows.Count; i++)
        {
            if (rows[i].Length != expectedColumns)
            {
                errors.Add($"Row {i + 1} has {rows[i].Length} fields, expected {expectedColumns}.");
            }
        }

        return errors;
    }

    private void AddField(List<string> row, StringBuilder field)
    {
        var value = field.ToString();
        if (_trimFields)
            value = value.Trim();
        row.Add(value);
        field.Clear();
    }

    private string QuoteFieldIfNeeded(string field)
    {
        if (field.Contains(_delimiter) || field.Contains(_quoteChar) || field.Contains('\n') || field.Contains('\r'))
        {
            var escaped = field.Replace(_quoteChar.ToString(), $"{_quoteChar}{_quoteChar}");
            return $"{_quoteChar}{escaped}{_quoteChar}";
        }
        return field;
    }
}

/// <summary>
/// Represents a parsed CSV document with headers and rows.
/// </summary>
public class CsvDocument
{
    public string[] Headers { get; }
    public IReadOnlyList<CsvRow> Rows { get; }

    public int RowCount => Rows.Count;
    public int ColumnCount => Headers.Length > 0 ? Headers.Length : (Rows.Count > 0 ? Rows[0].Fields.Length : 0);

    public CsvDocument(string[] headers, List<CsvRow> rows)
    {
        Headers = headers ?? Array.Empty<string>();
        Rows = rows?.AsReadOnly() ?? new List<CsvRow>().AsReadOnly();
    }

    /// <summary>
    /// Gets a column of values by header name.
    /// </summary>
    public string[] GetColumn(string headerName)
    {
        ArgumentNullException.ThrowIfNull(headerName);
        var index = Array.IndexOf(Headers, headerName);
        if (index < 0)
            throw new KeyNotFoundException($"Header '{headerName}' not found.");
        return Rows.Select(r => r.Fields.Length > index ? r.Fields[index] : "").ToArray();
    }

    /// <summary>
    /// Gets a column of values by index.
    /// </summary>
    public string[] GetColumn(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative.");
        return Rows.Select(r => r.Fields.Length > index ? r.Fields[index] : "").ToArray();
    }
}

/// <summary>
/// Represents a single row in a CSV document.
/// </summary>
public class CsvRow
{
    private readonly string[]? _headers;

    public int RowIndex { get; }
    public string[] Fields { get; }

    public CsvRow(int rowIndex, string[] fields, string[]? headers)
    {
        RowIndex = rowIndex;
        Fields = fields;
        _headers = headers;
    }

    /// <summary>
    /// Gets a field value by header name.
    /// </summary>
    public string this[string headerName]
    {
        get
        {
            if (_headers == null)
                throw new InvalidOperationException("No headers defined for this row.");
            var index = Array.IndexOf(_headers, headerName);
            if (index < 0)
                throw new KeyNotFoundException($"Header '{headerName}' not found.");
            return index < Fields.Length ? Fields[index] : "";
        }
    }

    /// <summary>
    /// Gets a field value by index.
    /// </summary>
    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= Fields.Length)
                throw new IndexOutOfRangeException($"Field index {index} is out of range.");
            return Fields[index];
        }
    }

    /// <summary>
    /// Tries to get a field value by header name.
    /// </summary>
    public bool TryGetField(string headerName, out string value)
    {
        value = "";
        if (_headers == null) return false;
        var index = Array.IndexOf(_headers, headerName);
        if (index < 0 || index >= Fields.Length) return false;
        value = Fields[index];
        return true;
    }
}
