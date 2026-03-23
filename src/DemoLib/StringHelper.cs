namespace DemoLib;

/// <summary>
/// A collection of string utility methods.
/// Inspired by CargoWise string manipulation patterns.
/// </summary>
public static class StringHelper
{
	/// <summary>
	/// Truncates a string to the specified maximum length.
	/// If the string is shorter than maxLength, returns it unchanged.
	/// </summary>
	public static string Truncate(string input, int maxLength)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));
		if (maxLength < 0)
			throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be non-negative.");

		return input.Length <= maxLength ? input : input[..maxLength];
	}

	/// <summary>
	/// Returns true if the string contains only uppercase letters, digits, and spaces.
	/// </summary>
	public static bool IsAlphanumericAndSpaces(string input)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		foreach (var c in input)
		{
			if (!char.IsAsciiLetterOrDigit(c) && c != ' ')
				return false;
		}

		return true;
	}

	/// <summary>
	/// Converts a standard Base64 string to a URL-safe Base64 string (RFC 4648).
	/// Replaces '+' with '-', '/' with '_', and removes trailing '=' padding.
	/// </summary>
	public static string ToUrlSafeBase64(string regularBase64)
	{
		if (regularBase64 is null)
			throw new ArgumentNullException(nameof(regularBase64));

		return regularBase64
			.TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_');
	}

	/// <summary>
	/// Converts a URL-safe Base64 string back to standard Base64.
	/// Restores '+', '/', and re-adds '=' padding as needed.
	/// </summary>
	public static string FromUrlSafeBase64(string urlSafeBase64)
	{
		if (urlSafeBase64 is null)
			throw new ArgumentNullException(nameof(urlSafeBase64));

		var base64 = urlSafeBase64
			.Replace('-', '+')
			.Replace('_', '/');

		switch (base64.Length % 4)
		{
			case 2:
				base64 += "==";
				break;
			case 3:
				base64 += "=";
				break;
		}

		return base64;
	}
}
