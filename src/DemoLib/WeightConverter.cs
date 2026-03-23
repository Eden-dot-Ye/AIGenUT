namespace DemoLib;

/// <summary>
/// Converts between different weight units commonly used in freight logistics.
/// </summary>
public static class WeightConverter
{
	private const decimal KgToLbsFactor = 2.20462m;
	private const decimal KgToOzFactor = 35.274m;
	private const decimal LbsToKgFactor = 0.453592m;
	private const decimal TonneToKgFactor = 1000m;

	/// <summary>
	/// Converts kilograms to pounds.
	/// </summary>
	public static decimal KgToLbs(decimal kg)
	{
		if (kg < 0)
			throw new ArgumentOutOfRangeException(nameof(kg), "Weight cannot be negative.");

		return Math.Round(kg * KgToLbsFactor, 4);
	}

	/// <summary>
	/// Converts pounds to kilograms.
	/// </summary>
	public static decimal LbsToKg(decimal lbs)
	{
		if (lbs < 0)
			throw new ArgumentOutOfRangeException(nameof(lbs), "Weight cannot be negative.");

		return Math.Round(lbs * LbsToKgFactor, 4);
	}

	/// <summary>
	/// Converts kilograms to ounces.
	/// </summary>
	public static decimal KgToOz(decimal kg)
	{
		if (kg < 0)
			throw new ArgumentOutOfRangeException(nameof(kg), "Weight cannot be negative.");

		return Math.Round(kg * KgToOzFactor, 4);
	}

	/// <summary>
	/// Converts metric tonnes to kilograms.
	/// </summary>
	public static decimal TonneToKg(decimal tonnes)
	{
		if (tonnes < 0)
			throw new ArgumentOutOfRangeException(nameof(tonnes), "Weight cannot be negative.");

		return tonnes * TonneToKgFactor;
	}

	/// <summary>
	/// Calculates the volumetric (dimensional) weight in kilograms.
	/// Formula: (length × width × height) / divisor
	/// Common divisors: 5000 for air freight, 6000 for sea freight.
	/// Dimensions are in centimeters.
	/// </summary>
	public static decimal CalculateVolumetricWeight(decimal lengthCm, decimal widthCm, decimal heightCm, int divisor = 5000)
	{
		if (lengthCm < 0 || widthCm < 0 || heightCm < 0)
			throw new ArgumentOutOfRangeException("Dimensions cannot be negative.");
		if (divisor <= 0)
			throw new ArgumentOutOfRangeException(nameof(divisor), "Divisor must be positive.");

		return Math.Round(lengthCm * widthCm * heightCm / divisor, 2);
	}
}
