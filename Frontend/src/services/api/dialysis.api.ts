import { apiClient, unwrapData } from '@/services/http'

import type {
  AdminPermission,
  AdminRole,
  AdminUser,
  AuditLog,
  AuthUser,
  CreateAdminUserRequest,
  CreateMedCardRequest,
  CreatePatientRequest,
  CreateSessionRequest,
  CreateTenantRequest,
  FinishSessionRequest,
  HdSession,
  LoginResponse,
  MedCard,
  MedCenterMachine,
  Patient,
  PatientGridExportRequest,
  PatientGridQueryRequest,
  PatientGridQueryResult,
  PatientGroup,
  PatientIdentityLookup,
  PauseSessionRequest,
  Region,
  SessionMeasurement,
  SessionMeasurementPoint,
  SessionWorkflowSettings,
  StartSessionRequest,
  SwitchTenantResponse,
  Tenant,
  TenantDetails,
  TenantGridExportRequest,
  TenantGridQueryRequest,
  TenantGridQueryResult,
  UpdateAdminUserRequest,
  UpdateMedCardRequest,
  UpdatePatientRequest,
  UpdateSessionWorkflowSettingsRequest,
  UpdateTenantRequest,
  UpsertMedCenterMachineRequest,
} from '../types/dialysis'

export const authApi = {
  login: (username: string, password: string) =>
    unwrapData(apiClient.post<LoginResponse>('/auth/login', { username, password })),

  refresh: (refreshToken: string) =>
    unwrapData(apiClient.post<LoginResponse>('/auth/refresh', { refreshToken })),

  me: () =>
    unwrapData(apiClient.get<AuthUser>('/auth/me')),

  logout: (refreshToken: string) =>
    apiClient.post('/auth/logout', { refreshToken }),
}

export const tenantApi = {
  my: () =>
    unwrapData(apiClient.get<Tenant[]>('/tenants/my')),

  switch: (tenantId: string) =>
    unwrapData(apiClient.post<SwitchTenantResponse>(`/tenants/${tenantId}/switch`)),

  get: (tenantId: string) =>
    unwrapData(apiClient.get<TenantDetails>(`/tenants/${tenantId}`)),

  create: (payload: CreateTenantRequest) =>
    unwrapData(apiClient.post<TenantDetails>('/tenants', payload)),

  update: (tenantId: string, payload: UpdateTenantRequest) =>
    unwrapData(apiClient.put<TenantDetails>(`/tenants/${tenantId}`, payload)),

  deactivate: (tenantId: string) =>
    apiClient.delete(`/tenants/${tenantId}`),

  gridQuery: (payload: TenantGridQueryRequest) =>
    unwrapData(apiClient.post<TenantGridQueryResult>('/tenants/grid/query', payload)),

  gridExport: (payload: TenantGridExportRequest) =>
    unwrapData(apiClient.post<TenantGridQueryResult>('/tenants/grid/export', payload)),
}

export const adminUserApi = {
  list: () =>
    unwrapData(apiClient.get<AdminUser[]>('/admin/users')),

  roles: () =>
    unwrapData(apiClient.get<AdminRole[]>('/admin/users/roles')),

  permissions: () =>
    unwrapData(apiClient.get<AdminPermission[]>('/admin/users/permissions')),

  create: (payload: CreateAdminUserRequest) =>
    unwrapData(apiClient.post<AdminUser>('/admin/users', payload)),

  update: (id: number, payload: UpdateAdminUserRequest) =>
    unwrapData(apiClient.put<AdminUser>(`/admin/users/${id}`, payload)),

  deactivate: (id: number) =>
    apiClient.delete(`/admin/users/${id}`),
}

export const settingsApi = {
  getSessionWorkflow: () =>
    unwrapData(apiClient.get<SessionWorkflowSettings>('/settings/session-workflow')),

  updateSessionWorkflow: (payload: UpdateSessionWorkflowSettingsRequest) =>
    unwrapData(apiClient.put<SessionWorkflowSettings>('/settings/session-workflow', payload)),
}

export const patientApi = {
  list: (params?: { search?: string, groupId?: number | null }) =>
    unwrapData(apiClient.get<Patient[]>('/patients', { params })),

  get: (id: number) =>
    unwrapData(apiClient.get<Patient>(`/patients/${id}`)),

  getByInn: (inn: string) =>
    unwrapData(apiClient.get<Patient>(`/patients/by-inn/${inn}`)),

  lookupIdentity: (inn: string) =>
    unwrapData(apiClient.get<PatientIdentityLookup>(`/patients/identity-lookup/${encodeURIComponent(inn)}`)),

  create: (payload: CreatePatientRequest) =>
    unwrapData(apiClient.post<Patient>('/patients', payload)),

  update: (id: number, payload: UpdatePatientRequest) =>
    unwrapData(apiClient.put<Patient>(`/patients/${id}`, payload)),

  delete: (id: number) =>
    apiClient.delete(`/patients/${id}`),

  gridQuery: (payload: PatientGridQueryRequest) =>
    unwrapData(apiClient.post<PatientGridQueryResult>('/patients/grid/query', payload)),

  gridExport: (payload: PatientGridExportRequest) =>
    unwrapData(apiClient.post<PatientGridQueryResult>('/patients/grid/export', payload)),

  groups: () =>
    unwrapData(apiClient.get<PatientGroup[]>('/patients/groups')),
}

export const regionApi = {
  list: (includeInactive = false) =>
    unwrapData(apiClient.get<Region[]>('/regions', { params: { includeInactive } })),
}

export const medCardApi = {
  list: (tenantIds?: string[]) =>
    unwrapData(apiClient.get<MedCard[]>('/med-cards', { params: { tenantIds } })),

  get: (id: number) =>
    unwrapData(apiClient.get<MedCard>(`/med-cards/${id}`)),

  create: (payload: CreateMedCardRequest) =>
    unwrapData(apiClient.post<MedCard>('/med-cards', payload)),

  update: (id: number, payload: UpdateMedCardRequest) =>
    unwrapData(apiClient.put<MedCard>(`/med-cards/${id}`, payload)),

  delete: (id: number) =>
    apiClient.delete(`/med-cards/${id}`),
}

export const machineApi = {
  list: (tenantIds?: string[]) =>
    unwrapData(apiClient.get<MedCenterMachine[]>('/med-center-machines', { params: { tenantIds } })),

  get: (id: number) =>
    unwrapData(apiClient.get<MedCenterMachine>(`/med-center-machines/${id}`)),

  create: (payload: UpsertMedCenterMachineRequest) =>
    unwrapData(apiClient.post<MedCenterMachine>('/med-center-machines', payload)),

  update: (id: number, payload: UpsertMedCenterMachineRequest) =>
    unwrapData(apiClient.put<MedCenterMachine>(`/med-center-machines/${id}`, payload)),

  delete: (id: number) =>
    apiClient.delete(`/med-center-machines/${id}`),
}

export const sessionApi = {
  list: (tenantIds?: string[]) =>
    unwrapData(apiClient.get<HdSession[]>('/sessions', { params: { tenantIds } })),

  get: (id: number) =>
    unwrapData(apiClient.get<HdSession>(`/sessions/${id}`)),

  create: (payload: CreateSessionRequest) =>
    unwrapData(apiClient.post<HdSession>('/sessions', payload)),

  start: (id: number, payload: StartSessionRequest) =>
    unwrapData(apiClient.put<HdSession>(`/sessions/${id}/start`, payload)),

  pause: (id: number, payload: PauseSessionRequest) =>
    unwrapData(apiClient.put<HdSession>(`/sessions/${id}/pause`, payload)),

  resume: (id: number) =>
    unwrapData(apiClient.put<HdSession>(`/sessions/${id}/resume`)),

  finish: (id: number, payload: FinishSessionRequest) =>
    unwrapData(apiClient.put<HdSession>(`/sessions/${id}/finish`, payload)),

  endIdentify: (id: number) =>
    unwrapData(apiClient.put<HdSession>(`/sessions/${id}/end-identify`)),

  sendToPay: (id: number) =>
    unwrapData(apiClient.put<HdSession>(`/sessions/${id}/send-to-pay`)),

  markPaid: (id: number) =>
    unwrapData(apiClient.put<HdSession>(`/sessions/${id}/mark-paid`)),

  archive: (id: number) =>
    unwrapData(apiClient.put<HdSession>(`/sessions/${id}/archive`)),

  measurement: (id: number, point: SessionMeasurementPoint, payload: FinishSessionRequest['endMeasurement']) =>
    unwrapData(apiClient.put<SessionMeasurement>(`/sessions/${id}/measurements/${point}`, payload)),
}

export const auditApi = {
  latest: () =>
    unwrapData(apiClient.get<AuditLog[]>('/admin/audit-logs')),
}
