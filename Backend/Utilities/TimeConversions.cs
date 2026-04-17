using Shared;
using SharedConstants = Shared.Constants;

namespace CoupleChat.Utilities;

/// <summary>
/// Centralized utility for converting between different time units used in domestic work calculations.
/// </summary>
public static class TimeConversions
{
    /// <summary>
    /// Convert minutes per day to hours per week.
    /// Formula: (minutesPerDay / 60) * 7 days
    /// </summary>
    public static decimal MinutesPerDayToHoursPerWeek(double minutesPerDay)
    {
        return (decimal)(minutesPerDay / 60.0 * 7);
    }

    /// <summary>
    /// Convert hours per week to hours per month.
    /// Formula: hoursPerWeek * (52 / 12)
    /// </summary>
    public static decimal HoursPerWeekToHoursPerMonth(decimal hoursPerWeek)
    {
        return hoursPerWeek * SharedConstants.Domestique.WeekToMonthFactor;
    }

    /// <summary>
    /// Convert minutes per day to hours per month (combined conversion).
    /// </summary>
    public static decimal MinutesPerDayToHoursPerMonth(double minutesPerDay)
    {
        var hoursPerWeek = MinutesPerDayToHoursPerWeek(minutesPerDay);
        return HoursPerWeekToHoursPerMonth(hoursPerWeek);
    }

    /// <summary>
    /// Calculate monetary value based on hours and hourly rate.
    /// </summary>
    public static decimal CalculateMonetaryValue(decimal hours, decimal hourlyRate = 0)
    {
        var rate = hourlyRate <= 0 ? SharedConstants.Domestique.HourlyRate : hourlyRate;
        return hours * rate;
    }
}
