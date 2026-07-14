import type { LoadOptions } from 'devextreme/data'

import type {
  CreatePatientRequest,
  Patient,
  PatientGender,
  PatientGridLoadRequest,
  UpdatePatientRequest,
} from '@/services/types/dialysis'

import { createPatientSchema, updatePatientSchema } from './patient-schema'

export type PatientEditValues = Partial<Omit<Patient, 'birthDate' | 'districtId' | 'regionId'>> & {
  birthDate?: Date | string | null
  districtId?: number | null
  regionId?: number | null
}

const SERIALIZED_LOAD_FIELDS = [
  'sort',
  'group',
  'filter',
  'totalSummary',
  'groupSummary',
] as const

export function serializePatientGridLoadOptions(
  loadOptions: LoadOptions,
): PatientGridLoadRequest {
  const request: PatientGridLoadRequest = {
    skip: loadOptions.skip ?? 0,
    take: loadOptions.take ?? 25,
    requireTotalCount: loadOptions.requireTotalCount ?? true,
    requireGroupCount: loadOptions.requireGroupCount ?? false,
    isCountQuery: false,
    sort: null,
    group: null,
    filter: null,
    totalSummary: null,
    groupSummary: null,
  }

  for (const field of SERIALIZED_LOAD_FIELDS) {
    const value = loadOptions[field]
    request[field] = value === undefined || value === null
      ? null
      : JSON.stringify(value)
  }

  return request
}

export function toCreatePatientRequest(values: PatientEditValues): CreatePatientRequest {
  const normalized = {
    inn: String(values.inn ?? '').trim(),
    firstName: String(values.firstName ?? '').trim(),
    lastName: String(values.lastName ?? '').trim(),
    middleName: String(values.middleName ?? '').trim(),
    birthDate: normalizeDateOnly(values.birthDate),
    gender: Number(values.gender ?? 1) as PatientGender,
    address: String(values.address ?? '').trim(),
    address2: String(values.address2 ?? '').trim(),
    phone: String(values.phone ?? '').trim(),
    regionId: Number(values.regionId),
    districtId: Number(values.districtId),
    specialStatus: Boolean(values.specialStatus),
  }

  return createPatientSchema.parse(normalized)
}

export function toUpdatePatientRequest(values: PatientEditValues): UpdatePatientRequest {
  const normalized = {
    ...toCreatePatientRequest(values),
    groupId: Number(values.groupId),
    isActive: Boolean(values.isActive),
  }

  return updatePatientSchema.parse(normalized)
}

function normalizeDateOnly(value: Date | string | null | undefined): string {
  if (value instanceof Date) {
    if (Number.isNaN(value.getTime())) {
      return ''
    }

    const year = value.getFullYear()
    const month = String(value.getMonth() + 1).padStart(2, '0')
    const day = String(value.getDate()).padStart(2, '0')
    return `${year}-${month}-${day}`
  }

  return String(value ?? '').slice(0, 10)
}
