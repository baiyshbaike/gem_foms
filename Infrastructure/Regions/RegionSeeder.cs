using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Regions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Regions;

public static class RegionSeeder
{
    private const long SystemUserId = 0;

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var seedFile = await LoadSeedFileAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var existingRegions = await db.Regions
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

        var regionsByName = existingRegions.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var regionSeed in seedFile.Regions)
        {
            if (!regionsByName.TryGetValue(regionSeed.NameRu, out var region))
            {
                region = new Region
                {
                    Name = regionSeed.NameRu,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = SystemUserId
                };

                db.Regions.Add(region);
                regionsByName[region.Name] = region;
                continue;
            }

            region.IsActive = true;
            region.IsDeleted = false;
            region.DeletedAt = null;
        }

        await db.SaveChangesAsync(cancellationToken);

        var existingDistricts = await db.Districts
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

        foreach (var regionSeed in seedFile.Regions)
        {
            var region = regionsByName[regionSeed.NameRu];
            var existingDistrictNames = existingDistricts
                .Where(x => x.RegionId == region.Id)
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var districtName in regionSeed.Raions)
            {
                if (!existingDistrictNames.TryGetValue(districtName, out var district))
                {
                    district = new District
                    {
                        RegionId = region.Id,
                        Name = districtName,
                        IsActive = true,
                        CreatedAt = now,
                        CreatedBy = SystemUserId
                    };

                    db.Districts.Add(district);
                    existingDistrictNames[district.Name] = district;
                    continue;
                }

                district.IsActive = true;
                district.IsDeleted = false;
                district.DeletedAt = null;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await ResetIdentitySequencesAsync(db, cancellationToken);
    }

    private static async Task<RegionSeedFile> LoadSeedFileAsync(CancellationToken cancellationToken)
    {
        var path = ResolveSeedPath();
        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<RegionSeedFile>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            cancellationToken)
            ?? throw new InvalidOperationException("regions.json is empty or invalid.");
    }

    private static string ResolveSeedPath()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var outputPath = Path.Combine(baseDirectory, "regions.json");
        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        var directory = new DirectoryInfo(baseDirectory);
        while (directory is not null)
        {
            var sourcePath = Path.Combine(directory.FullName, "Infrastructure", "regions.json");
            if (File.Exists(sourcePath))
            {
                return sourcePath;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("regions.json was not found.", outputPath);
    }

    private static async Task ResetIdentitySequencesAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            SELECT setval(pg_get_serial_sequence('"GeoRegions"', 'Id'), COALESCE((SELECT MAX("Id") FROM "GeoRegions"), 1));
            SELECT setval(pg_get_serial_sequence('"Districts"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Districts"), 1));
            """,
            cancellationToken);
    }

    private sealed class RegionSeedFile
    {
        [JsonPropertyName("regions")]
        public List<RegionSeedItem> Regions { get; set; } = [];
    }

    private sealed class RegionSeedItem
    {
        [JsonPropertyName("name_ru")]
        public string NameRu { get; set; } = string.Empty;

        [JsonPropertyName("raions")]
        public List<string> Raions { get; set; } = [];
    }
}
