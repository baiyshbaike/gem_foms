import type {
  CreatePatientRequest,
  Patient,
  PatientGender,
  PatientGridFilter,
  PatientGridQueryRequest,
  UpdatePatientRequest,
} from '@/services/types/dialysis'

import { createPatientSchema, updatePatientSchema } from './patient-schema'

export type PatientEditValues = Partial<Omit<Patient, 'birthDate' | 'districtId' | 'regionId'>> & {
  birthDate?: Date | string | null
  districtId?: number | null
  regionId?: number | null
}

export interface PatientGridQueryState {
  pageIndex: number
  pageSize: number
  search: string
  sorting: Array<{ id: string, desc: boolean }>
  filters: PatientGridFilter[]
  groupBy: string | null
}

export function createPatientGridQueryRequest(
  state: PatientGridQueryState,
): PatientGridQueryRequest {
  return {
    page: Math.max(1, state.pageIndex + 1),
    pageSize: Math.min(100, Math.max(1, state.pageSize)),
    search: state.search.trim() || null,
    sorting: state.sorting.map(sort => ({
      field: sort.id,
      descending: sort.desc,
    })),
    filters: state.filters.map(filter => ({
      ...filter,
      value: filter.value?.trim() || null,
      valueTo: filter.valueTo?.trim() || null,
    })),
    groupBy: state.groupBy || null,
  }
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
  }

  return createPatientSchema.parse(normalized)
}

export function toUpdatePatientRequest(values: PatientEditValues): UpdatePatientRequest {
  const normalized = {
    ...toCreatePatientRequest(values),
    groupId: Number(values.groupId),
    specialStatus: Boolean(values.specialStatus),
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
