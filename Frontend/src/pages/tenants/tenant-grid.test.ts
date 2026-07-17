import { describe, expect, it } from 'vitest'

import type { Region } from '@/services/types/dialysis'

import {
  getActiveTenantDistricts,
  toCreateTenantRequest,
  toUpdateTenantRequest,
} from './tenant-grid'

const values = {
  address: '  Main street  ',
  code: ' center-01 ',
  districtId: 2,
  regionId: 1,
  isActive: false,
  name: '  Central Dialysis  ',
  phone: ' +996 312 00 00 00 ',
}

describe('tenant editor helpers', () => {
  it('normalizes create and update payloads', () => {
    expect(toCreateTenantRequest(values)).toEqual({
      address: 'Main street',
      code: 'CENTER-01',
      districtId: 2,
      regionId: 1,
      name: 'Central Dialysis',
      phone: '+996 312 00 00 00',
    })
    expect(toUpdateTenantRequest(values).isActive).toBe(false)
  })

  it('rejects an empty tenant phone', () => {
    expect(() => toCreateTenantRequest({ ...values, phone: '   ' })).toThrow()
  })

  it('rejects an invalid tenant code', () => {
    expect(() => toCreateTenantRequest({ ...values, code: 'bad code' })).toThrow()
  })

  it('returns districts only from the active selected region', () => {
    const regions: Region[] = [
      {
        id: 1,
        name: 'North',
        isActive: true,
        districts: [
          { id: 2, regionId: 1, regionName: 'North', name: 'Open', isActive: true },
          { id: 3, regionId: 1, regionName: 'North', name: 'Closed', isActive: false },
        ],
      },
    ]

    expect(getActiveTenantDistricts(regions, 1).map(district => district.id)).toEqual([2])
    expect(getActiveTenantDistricts(regions, 99)).toEqual([])
  })
})
