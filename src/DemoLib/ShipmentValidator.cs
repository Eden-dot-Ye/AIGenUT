using System.Text.RegularExpressions;

namespace DemoLib;

/// <summary>
/// Validates shipment-related data fields.
/// Mimics the validation patterns found in CargoWise customs and freight modules.
/// </summary>
public static partial class ShipmentValidator
{
	/// <summary>
	/// Validates a container number according to ISO 6346 format:
	/// 4 uppercase letters + 7 digits (e.g., "ABCU1234567").
	/// </summary>
	public static bool IsValidContainerNumber(string containerNumber)
	{
		if (string.IsNullOrWhiteSpace(containerNumber))
			return false;

		return ContainerNumberRegex().IsMatch(containerNumber);
	}

	/// <summary>
	/// Validates a HS (Harmonized System) tariff code.
	/// Must be 6 to 10 digits, optionally separated by dots every 2 digits.
	/// Examples: "847130", "8471.30", "8471.30.00.00"
	/// </summary>
	public static bool IsValidHsCode(string hsCode)
	{
		if (string.IsNullOrWhiteSpace(hsCode))
			return false;

		return HsCodeRegex().IsMatch(hsCode);
	}

	/// <summary>
	/// Calculates the total weight of shipment items.
	/// Returns 0 if the list is null or empty.
	/// Each negative weight is treated as 0.
	/// </summary>
	public static decimal CalculateTotalWeight(IReadOnlyList<decimal>? weights)
	{
		if (weights is null || weights.Count == 0)
			return 0m;

		decimal total = 0m;
		foreach (var w in weights)
		{
			if (w > 0)
				total += w;
		}

		return total;
	}

	/// <summary>
	/// Formats a tracking reference by converting to uppercase and removing whitespace.
	/// </summary>
	public static string NormalizeTrackingReference(string reference)
	{
		if (reference is null)
			throw new ArgumentNullException(nameof(reference));

		return reference.Trim().ToUpperInvariant().Replace(" ", "");
	}

	/// <summary>
	/// Determines the shipment status description from a status code.
	/// </summary>
	public static string GetStatusDescription(string statusCode) => statusCode?.ToUpperInvariant() switch
	{
		"PND" => "Pending",
		"INP" => "In Progress",
		"CMP" => "Completed",
		"CAN" => "Cancelled",
		"HLD" => "On Hold",
		_ => "Unknown"
	};

	[GeneratedRegex(@"^[A-Z]{4}\d{7}$")]
	private static partial Regex ContainerNumberRegex();

	[GeneratedRegex(@"^\d{2}(\.\d{2}){0,4}$|^\d{6,10}$")]
	private static partial Regex HsCodeRegex();
}
