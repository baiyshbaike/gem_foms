using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Data;

internal static class GeoRegionSeedData
{
    public static object[] Regions(DateTimeOffset seedDate)
    {
        return Load().Regions
            .Select((region, index) => new
            {
                Id = (long)index + 1,
                region.Name,
                CreatedAt = seedDate,
                CreatedBy = 0L,
                UpdatedAt = (DateTimeOffset?)null,
                UpdatedBy = (long?)null,
                IsDeleted = false,
                DeletedAt = (DateTimeOffset?)null,
                IsActive = true
            })
            .Cast<object>()
            .ToArray();
    }

    public static object[] Districts(DateTimeOffset seedDate)
    {
        var districtId = 1L;
        var districts = new List<object>();

        foreach (var region in Load().Regions.Select((value, index) => new { value, index }))
        {
            foreach (var districtName in region.value.Districts)
            {
                districts.Add(new
                {
                    Id = districtId++,
                    RegionId = (long)region.index + 1,
                    Name = districtName,
                    CreatedAt = seedDate,
                    CreatedBy = 0L,
                    UpdatedAt = (DateTimeOffset?)null,
                    UpdatedBy = (long?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTimeOffset?)null,
                    IsActive = true
                });
            }
        }

        return districts.ToArray();
    }

    private static RegionSeedFile Load()
    {
        var path = ResolveSeedPath();
        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize<RegionSeedFile>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web))
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

    private sealed class RegionSeedFile
    {
        [JsonPropertyName("regions")]
        public List<RegionSeedItem> Regions { get; set; } = [];
    }

    private sealed class RegionSeedItem
    {
        [JsonPropertyName("name_ru")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("raions")]
        public List<string> Districts { get; set; } = [];
    }
}
