using Infrastructure.Common;
using Microsoft.Extensions.Configuration;

namespace UnitTests;

public sealed class RegionalSettingsTests
{
    [Fact]
    public void Bishkek_day_range_uses_local_midnight_and_utc_storage()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RegionalSettings:Locale"] = "ru-RU",
                ["RegionalSettings:TimeZoneId"] = "Asia/Bishkek"
            })
            .Build();
        var settings = RegionalSettings.FromConfiguration(configuration);

        var range = settings.GetUtcDayRange(
            new DateTimeOffset(2026, 7, 16, 19, 30, 0, TimeSpan.Zero));

        Assert.Equal("ru-RU", settings.Locale);
        Assert.Equal(
            new DateTimeOffset(2026, 7, 16, 18, 0, 0, TimeSpan.Zero),
            range.Start);
        Assert.Equal(
            new DateTimeOffset(2026, 7, 17, 18, 0, 0, TimeSpan.Zero),
            range.End);
    }
}
