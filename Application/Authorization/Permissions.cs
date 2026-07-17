namespace Application.Authorization;

public static class Permissions
{
    public const string AdminUsers = "admin.users";
    public const string AdminRoles = "admin.roles";
    public const string AdminPermissions = "admin.permissions";
    public const string AdminTenants = "admin.tenants";
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

    public const string RegionRead = "region.read";
    public const string DistrictRead = "district.read";
    
    public const string PatientLookup = "patient.lookup";
    public const string PatientRead = "patient.read";
    public const string PatientCreate = "patient.create";
    public const string PatientUpdate = "patient.update";
    public const string PatientDelete = "patient.delete";
    public const string PatientExport = "patient.export";
    
    public const string MedCardRead = "medcard.read";
    public const string MedCardCreate = "medcard.create";
    public const string MedCardUpdate = "medcard.update";
    public const string MedCardDelete = "medcard.delete";
    
    public const string SessionRead = "session.read";
    public const string SessionCreate = "session.create";
    public const string SessionStart = "session.start";
    public const string SessionPause = "session.pause";
    public const string SessionResume = "session.resume";
    public const string SessionFinish = "session.finish";
    public const string SessionEndIdentify = "session.end_identify";
    public const string SessionSendToPay = "session.send_to_pay";
    public const string SessionMarkPaid = "session.mark_paid";
    public const string SessionArchive = "session.archive";
    public const string SessionMeasurementUpdate = "session.measurement.update";
    public const string SessionOverrideTimeLimits = "session.override_time_limits";
    public const string SessionSettingsManage = "session.settings.manage";
    
    public const string MedCenterMachineRead = "medcenter.machine.read";
    public const string MedCenterMachineCreate = "medcenter.machine.create";
    public const string MedCenterMachineUpdate = "medcenter.machine.update";
    public const string MedCenterMachineDelete = "medcenter.machine.delete";
    
    public static readonly string[] All =
    [
        AdminUsers,
        AdminRoles,
        AdminPermissions,
        AdminTenants,
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
        RegionRead,
        DistrictRead,
        PatientLookup,
        PatientRead,
        PatientCreate,
        PatientUpdate,
        PatientDelete,
        PatientExport,
        MedCardRead,
        MedCardCreate,
        MedCardUpdate,
        MedCardDelete,
        SessionRead ,
        SessionCreate ,
        SessionStart,
        SessionPause,
        SessionResume,
        SessionFinish,
        SessionEndIdentify,
        SessionSendToPay,
        SessionMarkPaid,
        SessionArchive,
        SessionMeasurementUpdate,
        SessionOverrideTimeLimits,
        SessionSettingsManage,
        MedCenterMachineRead,
        MedCenterMachineCreate,
        MedCenterMachineUpdate,
        MedCenterMachineDelete,
    ];
}
