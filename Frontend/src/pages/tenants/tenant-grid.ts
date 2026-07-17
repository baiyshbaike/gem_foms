import type { z } from 'zod'

import type {
  CreateTenantRequest,
  Region,
  UpdateTenantRequest,
} from '@/services/types/dialysis'

import type { tenantEditorSchema } from './tenant-schema'

import { createTenantSchema, updateTenantSchema } from './tenant-schema'

export type TenantEditorValues = z.input<typeof tenantEditorSchema>

export function toCreateTenantRequest(values: TenantEditorValues): CreateTenantRequest {
  return createTenantSchema.parse(normalizeTenantValues(values))
}

export function toUpdateTenantRequest(values: TenantEditorValues): UpdateTenantRequest {
  return updateTenantSchema.parse({
    ...normalizeTenantValues(values),
    isActive: Boolean(values.isActive),
  })
}

function normalizeTenantValues(values: TenantEditorValues) {
  return {
    code: String(values.code ?? ''),
    name: String(values.name ?? ''),
    address: String(values.address ?? ''),
    phone: String(values.phone ?? ''),
    regionId: Number(values.regionId),
    districtId: Number(values.districtId),
  }
}

export function getActiveTenantDistricts(regions: Region[], regionId: number) {
  return regions
    .find(region => region.id === regionId && region.isActive)
    ?.districts
    .filter(district => district.isActive) ?? []
}
