using System.Net.Http.Json;
using System.Text.Json;
using Application.Audit;
using Application.Patients;
using Contracts.Patients;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Patients;

public sealed class PatientIdentityLookupService : IPatientIdentityLookupService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IActionLogService _actionLogService;
    private readonly AppDbContext _db;
    private readonly ILogger<PatientIdentityLookupService> _logger;

    public PatientIdentityLookupService(
        HttpClient httpClient,
        IActionLogService actionLogService,
        AppDbContext db,
        ILogger<PatientIdentityLookupService> logger)
    {
        _httpClient = httpClient;
        _actionLogService = actionLogService;
        _db = db;
        _logger = logger;
    }

    public async Task<PatientIdentityLookupDto> LookupAsync(
        string inn,
        CancellationToken cancellationToken)
    {
        if (_httpClient.BaseAddress is null)
        {
            return await CompleteAsync(
                new PatientIdentityLookupDto(false, null, null, null),
                "Identity provider is not configured",
                cancellationToken);
        }

        try
        {
            using var response = await _httpClient.GetAsync(
                Uri.EscapeDataString(inn),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await CompleteAsync(
                    new PatientIdentityLookupDto(false, null, null, null),
                    "Identity provider returned no patient",
                    cancellationToken);
            }

            var externalPatient = await response.Content.ReadFromJsonAsync<ExternalPatientInfo>(
                SerializerOptions,
                cancellationToken);

            var hasCompleteIdentity = externalPatient is not null
                && string.Equals(externalPatient.Pin, inn, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(externalPatient.Name)
                && !string.IsNullOrWhiteSpace(externalPatient.Surname)
                && !string.IsNullOrWhiteSpace(externalPatient.Patronymic);

            var result = hasCompleteIdentity
                ? new PatientIdentityLookupDto(
                    true,
                    externalPatient!.Name!.Trim(),
                    externalPatient.Surname!.Trim(),
                    externalPatient.Patronymic!.Trim())
                : new PatientIdentityLookupDto(false, null, null, null);

            return await CompleteAsync(
                result,
                result.Found ? null : "Identity provider returned incomplete patient data",
                cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Patient identity lookup timed out");
            return await CompleteAsync(
                new PatientIdentityLookupDto(false, null, null, null),
                "Identity provider timed out",
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Patient identity lookup request failed");
            return await CompleteAsync(
                new PatientIdentityLookupDto(false, null, null, null),
                "Identity provider is unavailable",
                cancellationToken);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Patient identity lookup returned invalid JSON");
            return await CompleteAsync(
                new PatientIdentityLookupDto(false, null, null, null),
                "Identity provider returned an invalid response",
                cancellationToken);
        }
    }

    private async Task<PatientIdentityLookupDto> CompleteAsync(
        PatientIdentityLookupDto result,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        await _actionLogService.AddAsync(new ActionLogRequest
        {
            Action = "PatientIdentityLookup",
            Module = "patient",
            EntityName = "Patient",
            StatusCode = 200,
            Succeeded = result.Found,
            FailureReason = failureReason,
            MetadataJson = $$"""{"found":{{result.Found.ToString().ToLowerInvariant()}}}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private sealed class ExternalPatientInfo
    {
        public string? Pin { get; init; }
        public string? Surname { get; init; }
        public string? Name { get; init; }
        public string? Patronymic { get; init; }
    }
}
