using System.Text.RegularExpressions;

namespace Diginsight.Components;

/// <summary>
/// Provides parsing and calculation functionality for extended TimeSpan expressions that include calendar-based units.
/// </summary>
/// <remarks>
/// <para>
/// This parser extends the standard <see cref="TimeSpan"/> functionality by supporting calendar-based units
/// that cannot be precisely represented as fixed durations (years and months).
/// </para>
/// <para>
/// Supported units:
/// <list type="bullet">
/// <item><description>Y - Years (e.g., "1Y", "1.5Y")</description></item>
/// <item><description>M - Months (e.g., "6M", "2.5M")</description></item>
/// <item><description>W - Weeks (e.g., "2W", "1.5W")</description></item>
/// <item><description>D - Days (e.g., "30D", "1.5D")</description></item>
/// <item><description>H - Hours (e.g., "12H", "0.5H")</description></item>
/// <item><description>m - Minutes (e.g., "30m", "1.5m")</description></item>
/// <item><description>S - Seconds (e.g., "45S", "1.5S")</description></item>
/// </list>
/// </para>
/// <para>
/// Example expressions: "6M", "1.5Y", "2W3D", "1Y6M2W1D12H30m45S"
/// </para>
/// <para>
/// Units can be combined and must appear in the order shown above. Fractional values are supported for all units.
/// The parser is case-insensitive and falls back to standard <see cref="TimeSpan.Parse"/> for compatibility.
/// </para>
/// </remarks>
public static class TimeSpanParser
{
    private static readonly Regex DurationRegex = new(
        @"^(?:(\d+(?:\.\d+)?)Y)?(?:(\d+(?:\.\d+)?)M)?(?:(\d+(?:\.\d+)?)W)?(?:(\d+(?:\.\d+)?)D)?(?:(\d+(?:\.\d+)?)H)?(?:(\d+(?:\.\d+)?)m)?(?:(\d+(?:\.\d+)?)S)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Default duration period used when no expression is provided (1 month).
    /// </summary>
    public const string DefaultPeriod = "1M";

    /// <summary>
    /// Parses an extended TimeSpan expression into a <see cref="TimeSpan"/> using approximate conversions.
    /// </summary>
    /// <param name="expression">
    /// Duration expression to parse. Supports extended format (e.g., "6M", "1.5Y", "2W3D") 
    /// or standard <see cref="TimeSpan"/> format (e.g., "01:30:00"). 
    /// If null or whitespace, returns <see cref="DefaultPeriod"/>.
    /// </param>
    /// <returns>
    /// A <see cref="TimeSpan"/> representing the approximate duration. Calendar-based units
    /// are converted using average values (1 year ≈ 365.25 days, 1 month ≈ 30.44 days).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression format is invalid and cannot be parsed by either the extended
    /// parser or the standard <see cref="TimeSpan.Parse"/> method.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method attempts to parse using standard <see cref="TimeSpan.Parse"/> first for backward compatibility.
    /// If that fails, it uses the extended format parser.
    /// </para>
    /// <para>
    /// Uses approximate conversions for calendar-based units:
    /// <list type="bullet">
    /// <item><description>1 Year = 365.25 days (accounts for leap years)</description></item>
    /// <item><description>1 Month = 30.44 days (average month length: 365.25 / 12)</description></item>
    /// <item><description>1 Week = 7 days</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// For calendar-accurate date calculations, use <see cref="GetExpressionOccurrence"/> instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var sixMonths = ExtendedTimeSpanParser.Parse("6M");
    /// var oneAndHalfYears = ExtendedTimeSpanParser.Parse("1.5Y");
    /// var combined = ExtendedTimeSpanParser.Parse("1Y6M2W");
    /// var standard = ExtendedTimeSpanParser.Parse("01:30:00"); // Also supported
    /// </code>
    /// </example>
    public static TimeSpan Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return ParseToTimeSpan(DefaultPeriod);

        // Try standard TimeSpan.Parse first for backward compatibility
        if (TimeSpan.TryParse(expression, out TimeSpan standardTimeSpan))
            return standardTimeSpan;

        return ParseToTimeSpan(expression);
    }

    /// <summary>
    /// Attempts to parse an extended TimeSpan expression without throwing exceptions.
    /// </summary>
    /// <param name="expression">Duration expression to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="TimeSpan"/> if parsing succeeded,
    /// or <see cref="TimeSpan.Zero"/> if parsing failed.
    /// </param>
    /// <returns>
    /// <c>true</c> if the expression was successfully parsed; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method wraps <see cref="Parse"/> and catches all exceptions, making it safe
    /// for scenarios where invalid input is expected.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (ExtendedTimeSpanParser.TryParse("6M", out var duration))
    /// {
    ///     Console.WriteLine($"Duration: {duration.TotalDays} days");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Invalid expression");
    /// }
    /// </code>
    /// </example>
    public static bool TryParse(string expression, out TimeSpan result)
    {
        result = TimeSpan.Zero;

        try
        {
            result = Parse(expression);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Calculates a date/time by applying an extended duration expression to a reference date,
    /// using calendar-accurate operations for months and years.
    /// </summary>
    /// <param name="referenceDate">The starting date/time from which to calculate.</param>
    /// <param name="expression">
    /// Duration expression (e.g., "6M", "1Y", "2W3D"). If null or whitespace, uses <see cref="DefaultPeriod"/>.
    /// </param>
    /// <param name="occurrence">
    /// Multiplier for the expression (typically used for recurring calculations).
    /// Currently not implemented in the method body - the expression is applied once.
    /// </param>
    /// <returns>
    /// A <see cref="DateTimeOffset"/> representing the calculated date/time by subtracting
    /// the duration from the reference date.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression format is invalid.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method provides calendar-accurate calculations by using <see cref="DateTimeOffset.AddYears"/>
    /// and <see cref="DateTimeOffset.AddMonths"/> for year and month components, which properly handle
    /// varying month lengths and leap years.
    /// </para>
    /// <para>
    /// Smaller units (weeks, days, hours, minutes, seconds) are applied using <see cref="TimeSpan"/> arithmetic.
    /// Fractional years are converted to months, and fractional months are converted to days using
    /// an average of 30.44 days per month.
    /// </para>
    /// <para>
    /// The duration is subtracted from the reference date (moving backward in time).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var today = DateTimeOffset.Now;
    /// var sixMonthsAgo = ExtendedTimeSpanParser.CalculateExpressionOccurrence(today, "6M", 1);
    /// var oneYearAgo = ExtendedTimeSpanParser.CalculateExpressionOccurrence(today, "1Y", 1);
    /// </code>
    /// </example>
    public static DateTimeOffset GetExpressionOccurrence(DateTimeOffset referenceDate, string expression, int occurrence)
    {
        if (string.IsNullOrWhiteSpace(expression))
            expression = DefaultPeriod;

        // Try standard TimeSpan.Parse first
        if (TimeSpan.TryParse(expression, out TimeSpan standardTimeSpan))
            return referenceDate - standardTimeSpan;

        var match = DurationRegex.Match(expression.Trim().ToUpperInvariant());
        if (!match.Success)
            throw new ArgumentException($"Invalid duration expression: {expression}. Use format like '6M', '1.5Y', '2W3D'");

        var result = referenceDate;

        // Apply calendar-accurate operations for years and months
        if (match.Groups[1].Success) // Years
        {
            double years = double.Parse(match.Groups[1].Value);
            int wholeYears = (int)years;
            double fractionalYears = years - wholeYears;

            result = result.AddYears(-wholeYears);
            if (fractionalYears > 0)
            {
                // Convert fractional years to months for better accuracy
                int additionalMonths = (int)(fractionalYears * 12);
                result = result.AddMonths(-additionalMonths);
            }
        }

        if (match.Groups[2].Success) // Months
        {
            double months = double.Parse(match.Groups[2].Value);
            int wholeMonths = (int)months;
            double fractionalMonths = months - wholeMonths;

            result = result.AddMonths(-wholeMonths);
            if (fractionalMonths > 0)
            {
                // Convert fractional months to days
                int additionalDays = (int)(fractionalMonths * 30.44);
                result = result.AddDays(-additionalDays);
            }
        }

        // Apply TimeSpan operations for smaller units
        TimeSpan timeSpanOffset = TimeSpan.Zero;

        if (match.Groups[3].Success) // Weeks
            timeSpanOffset = timeSpanOffset.Add(TimeSpan.FromDays(double.Parse(match.Groups[3].Value) * 7));

        if (match.Groups[4].Success) // Days
            timeSpanOffset = timeSpanOffset.Add(TimeSpan.FromDays(double.Parse(match.Groups[4].Value)));

        if (match.Groups[5].Success) // Hours
            timeSpanOffset = timeSpanOffset.Add(TimeSpan.FromHours(double.Parse(match.Groups[5].Value)));

        if (match.Groups[6].Success) // Minutes
            timeSpanOffset = timeSpanOffset.Add(TimeSpan.FromMinutes(double.Parse(match.Groups[6].Value)));

        if (match.Groups[7].Success) // Seconds
            timeSpanOffset = timeSpanOffset.Add(TimeSpan.FromSeconds(double.Parse(match.Groups[7].Value)));

        return result - timeSpanOffset;
    }

    /// <summary>
    /// Parses an extended duration expression to a <see cref="TimeSpan"/> using approximate conversions.
    /// </summary>
    /// <param name="expression">Duration expression in extended format (e.g., "6M", "1.5Y", "2W3D").</param>
    /// <returns>A <see cref="TimeSpan"/> representing the approximate duration.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression format is invalid.</exception>
    /// <remarks>
    /// This is an internal implementation method used by <see cref="Parse"/>.
    /// Calendar-based units are converted using averages: 1 year = 365.25 days, 1 month = 30.44 days.
    /// </remarks>
    private static TimeSpan ParseToTimeSpan(string expression)
    {
        var match = DurationRegex.Match(expression.Trim().ToUpperInvariant());
        if (!match.Success)
            throw new ArgumentException($"Invalid duration expression: {expression}. Use format like '6M', '1.5Y', '2W3D'");

        TimeSpan result = TimeSpan.Zero;

        if (match.Groups[1].Success) // Years
            result = result.Add(TimeSpan.FromDays(double.Parse(match.Groups[1].Value) * 365.25));

        if (match.Groups[2].Success) // Months
            result = result.Add(TimeSpan.FromDays(double.Parse(match.Groups[2].Value) * 30.44));

        if (match.Groups[3].Success) // Weeks
            result = result.Add(TimeSpan.FromDays(double.Parse(match.Groups[3].Value) * 7));

        if (match.Groups[4].Success) // Days
            result = result.Add(TimeSpan.FromDays(double.Parse(match.Groups[4].Value)));

        if (match.Groups[5].Success) // Hours
            result = result.Add(TimeSpan.FromHours(double.Parse(match.Groups[5].Value)));

        if (match.Groups[6].Success) // Minutes
            result = result.Add(TimeSpan.FromMinutes(double.Parse(match.Groups[6].Value)));

        if (match.Groups[7].Success) // Seconds
            result = result.Add(TimeSpan.FromSeconds(double.Parse(match.Groups[7].Value)));

        return result;
    }

    /// <summary>
    /// Provides human-readable examples of supported duration expression formats.
    /// </summary>
    /// <remarks>
    /// These constants can be used as reference examples or in unit tests to demonstrate
    /// various combinations of duration units supported by <see cref="TimeSpanParser"/>.
    /// </remarks>
    public static class Examples
    {
        /// <summary>Six months duration: "6M"</summary>
        public const string SixMonths = "6M";
        
        /// <summary>One and a half years duration: "1.5Y"</summary>
        public const string OneAndHalfYears = "1.5Y";
        
        /// <summary>Six weeks duration: "6W"</summary>
        public const string SixWeeks = "6W";
        
        /// <summary>One year and six months duration: "1Y6M"</summary>
        public const string OneYearSixMonths = "1Y6M";
        
        /// <summary>Two weeks and three days duration: "2W3D"</summary>
        public const string TwoWeeksThreeDays = "2W3D";
        
        /// <summary>Three months and two weeks duration: "3M2W"</summary>
        public const string ThreeMonthsTwoWeeks = "3M2W";
        
        /// <summary>One year, two months, and five days duration: "1Y2M5D"</summary>
        public const string OneYearTwoMonthsFiveDays = "1Y2M5D";
    }
}
