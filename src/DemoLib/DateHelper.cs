namespace DemoLib;

/// <summary>
/// Date formatting utilities for shipment documents.
/// </summary>
public static class DateHelper
{
    /// <summary>
    /// Formats a date as ISO 8601 (yyyy-MM-dd).
    /// </summary>
    public static string ToIsoDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Returns the number of business days between two dates (excluding weekends).
    /// </summary>
    public static int BusinessDaysBetween(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End date must be after start date.");

        int businessDays = 0;
        var current = start;
        while (current < end)
        {
            current = current.AddDays(1);
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                businessDays++;
        }
        return businessDays;
    }
}