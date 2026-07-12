namespace Application.Authorization;

public static class Permissions
{
    public const string AdminUsers = "admin.users";
    public const string AdminRoles = "admin.roles";
    public const string AdminPermissions = "admin.permissions";
    public const string AdminAudit = "admin.audit";

    public const string AuthLogin = "auth.login";
    public const string AuthRefresh = "auth.refresh";
    public const string AuthLogout = "auth.logout";
    public const string AuthMe = "auth.me";
    
    public const string TenantRead = "tenant.read";
    public const string TenantSwitch = "tenant.switch";
    public const string TenantAccessAll = "tenant.access_all";
    public const string TenantAccessAssigned = "tenant.access_assigned";
    public const string TenantAccessOwn = "tenant.access_own";
    
    public static readonly string[] All =
    [
        AdminUsers,
        AdminRoles,
        AdminPermissions,
        AdminAudit,
        AuthLogin,
        AuthRefresh,
        AuthLogout,
        AuthMe,
        TenantRead,
        TenantSwitch,
        TenantAccessAll,
        TenantAccessAssigned,
        TenantAccessOwn,
    ];
}