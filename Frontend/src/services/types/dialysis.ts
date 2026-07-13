export interface AuthUser {
  id: number
  username: string
  firstName: string
  lastName: string
  permissions: string[]
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: AuthUser
}

export interface AuthSession extends LoginResponse {
  activeTenant?: Tenant
}

export interface Tenant {
  id: string
  code: string
  name: string
}

export interface SwitchTenantResponse {
  accessToken: string
  expiresAt: string
  activeTenant: Tenant
}

export interface AdminRole {
  id: number
  code: string
  name: string
  isSystem: boolean
}

export interface AdminPermission {
  id: number
  code: string
  module: string
  name: string
  description: string | null
}

export interface AdminUser {
  id: number
  username: string
  firstName: string
  lastName: string
  isActive: boolean
  failedLoginCount: number
  lockoutEndAt: string | null
  lastLoginAt: string | null
  createdAt: string
  updatedAt: string | null
  roles: AdminRole[]
}

export interface CreateAdminUserRequest {
  username: string
  password: string
  firstName: string
  lastName: string
  isActive: boolean
  roleIds: number[]
}

export interface UpdateAdminUserRequest {
  username: string
  password: string | null
  firstName: string
  lastName: string
  isActive: boolean
  roleIds: number[]
}

export interface SessionWorkflowSettings {
  id: number
  tenantId: string
  identificationStartLimitMinutes: number
  autoFinishActiveMinutes: number
  endIdentificationLimitMinutes: number
  sendToPayLimitMinutes: number
  createdAt: string
  createdBy: number
  updatedAt: string | null
  updatedBy: number | null
}

export interface UpdateSessionWorkflowSettingsRequest {
  identificationStartLimitMinutes: number
  autoFinishActiveMinutes: number
  endIdentificationLimitMinutes: number
  sendToPayLimitMinutes: number
}

export type PatientGender = 1 | 2

export interface Patient {
  id: number
  inn: string
  firstName: string
  lastName: string
  middleName: string
  birthDate: string
  gender: PatientGender
  address: string
  address2: string
  phone: string
  districtId: number
  regionId: number
  groupId: number
  groupCode: string
  groupName: string
  specialStatus: boolean
  createdAt: string
  updatedAt: string | null
  isActive: boolean
}

export interface CreatePatientRequest {
  inn: string
  firstName: string
  lastName: string
  middleName: string
  birthDate: string
  gender: PatientGender
  address: string
  address2: string
  phone: string
  districtId: number
  regionId: number
  specialStatus: boolean
}

export interface UpdatePatientRequest extends CreatePatientRequest {
  groupId: number
  isActive: boolean
}

export type MedCardStatus = 1 | 2 | 3 | 4

export interface MedCard {
  id: number
  tenantId: string
  patientId: number
  cardNumber: string
  openedAt: string
  closedAt: string | null
  status: MedCardStatus
  notes: string | null
}

export interface CreateMedCardRequest {
  patientId: number
  cardNumber: string
  openedAt: string | null
  notes: string | null
}

export interface UpdateMedCardRequest {
  cardNumber: string
  openedAt: string
  closedAt: string | null
  status: MedCardStatus
  notes: string | null
}

export type MachineAcquisitionType = 1 | 2

export interface MedCenterMachine {
  id: number
  tenantId: string
  acquisitionType: MachineAcquisitionType
  inventoryNumber: string
  name: string
  model: string
  serialNumber: string
  manufacturer: string
  manufacturingCountry: string | null
  manufactureYear: number
  certificateHolder: string | null
  certificateHolderCountry: string | null
  certificateNumber: string | null
  certificateCountry: string | null
  certificateIssuedAt: string
  permitName: string | null
  permitNumber: string | null
  permitSeries: string | null
  permitExpiresAt: string
  dailySessionLimit: number
  betweenSessionCooldownMinutes: number
  dailyLimitCooldownMinutes: number
  isApproved: boolean
  isActive: boolean
}

export interface UpsertMedCenterMachineRequest {
  acquisitionType: MachineAcquisitionType
  inventoryNumber: string
  name: string
  model: string
  serialNumber: string
  manufacturer: string
  manufacturingCountry: string | null
  manufactureYear: number
  certificateHolder: string | null
  certificateHolderCountry: string | null
  certificateNumber: string | null
  certificateCountry: string | null
  certificateIssuedAt: string
  permitName: string | null
  permitNumber: string | null
  permitSeries: string | null
  permitExpiresAt: string
  dailySessionLimit: number
  betweenSessionCooldownMinutes: number
  dailyLimitCooldownMinutes: number
  isApproved: boolean
  isActive: boolean
}

export interface SessionMeasurementRequest {
  sys: string | null
  dia: string | null
  temp: string | null
  ritm: string | null
  measuredAt: string | null
  note: string | null
}

export type SessionMeasurementPoint = 'Start' | 'Hour1' | 'Hour2' | 'Hour3' | 'Hour4' | 'End'

export interface SessionMeasurement {
  id: number
  point: SessionMeasurementPoint
  sys: string | null
  dia: string | null
  temp: string | null
  ritm: string | null
  measuredAt: string | null
  note: string | null
}

export interface HdSession {
  id: number
  tenantId: string
  patientId: number
  medCardId: number
  machineId: number | null
  status: string
  identifiedAt: string
  startedAt: string | null
  finishedAt: string | null
  endIdentifiedAt: string | null
  sentToPayAt: string | null
  paidAt: string | null
  activeMinutes: number | null
  pauseMinutes: number | null
}

export interface CreateSessionRequest {
  medCardId: number
}

export interface StartSessionRequest {
  machineId: number
  startMeasurement: SessionMeasurementRequest | null
}

export interface PauseSessionRequest {
  reason: string | null
}

export interface FinishSessionRequest {
  endMeasurement: SessionMeasurementRequest | null
}

export interface AuditLog {
  id: number
  userId: number | null
  usernameSnapshot: string | null
  action: string
  module: string
  succeeded: boolean
  failureReason: string | null
  createdAt: string
}
