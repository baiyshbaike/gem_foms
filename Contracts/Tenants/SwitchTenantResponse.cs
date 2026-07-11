namespace Contracts.Tenants;

public sealed record SwitchTenantResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    TenantDto ActiveTenant);