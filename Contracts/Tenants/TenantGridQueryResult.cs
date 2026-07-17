namespace Contracts.Tenants;

public sealed record TenantGridQueryResult(
    IReadOnlyList<TenantGridRowDto> Items,
    int TotalCount,
    IReadOnlyList<TenantGridGroupSummaryDto> Groups);

public sealed record TenantGridGroupSummaryDto(
    string Key,
    string Label,
    int Count);
