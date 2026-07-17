using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Common;

public sealed class RegionalSettings
{
    public const string SectionName = "RegionalSettings";

    private RegionalSettings(CultureInfo culture, TimeZoneInfo timeZone)
    {
        Culture = culture;
        TimeZone = timeZone;
    }

    public CultureInfo Culture { get; }
    public TimeZoneInfo TimeZone { get; }
    public string Locale => Culture.Name;
    public string TimeZoneId => TimeZone.Id;

    public static RegionalSettings FromConfiguration(IConfiguration configuration)
    {
        var locale = configuration[$"{SectionName}:Locale"] ?? "ru-RU";
        var timeZoneId = configuration[$"{SectionName}:TimeZoneId"] ?? "Asia/Bishkek";

        try
        {
            return new RegionalSettings(
                CultureInfo.GetCultureInfo(locale),
                TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }
        catch (CultureNotFoundException exception)
        {
            throw new InvalidOperationException($"Regional locale '{locale}' is invalid.", exception);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new InvalidOperationException($"Regional time zone '{timeZoneId}' was not found.", exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new InvalidOperationException($"Regional time zone '{timeZoneId}' is invalid.", exception);
        }
    }

    public UtcDayRange GetUtcDayRange(DateTimeOffset instant)
    {
        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, TimeZone).Date);
        var nextDate = localDate.AddDays(1);

        return new UtcDayRange(
            ToUtc(localDate),
            ToUtc(nextDate));
    }

    private DateTimeOffset ToUtc(DateOnly localDate)
    {
        var localMidnight = DateTime.SpecifyKind(
            localDate.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Unspecified);
        var offset = TimeZone.GetUtcOffset(localMidnight);
        return new DateTimeOffset(localMidnight, offset).ToUniversalTime();
    }
}

public readonly record struct UtcDayRange(DateTimeOffset Start, DateTimeOffset End);
