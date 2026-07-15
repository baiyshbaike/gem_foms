import { describe, expect, it } from 'vitest'

import {
  createPatientGridQueryRequest,
  toCreatePatientRequest,
  toUpdatePatientRequest,
} from './patient-grid'

describe('patient grid request helpers', () => {
  it('creates a normalized producer-independent server query', () => {
    const result = createPatientGridQueryRequest({
      pageIndex: 1,
      pageSize: 25,
      search: '  Ada  ',
      sorting: [{ id: 'createdAt', desc: true }],
      groupBy: 'regionName',
      filters: [{
        field: 'lastName',
        operator: 'contains',
        value: '  Love ',
        valueTo: null,
      }],
    })

    expect(result).toEqual({
      page: 2,
      pageSize: 25,
      search: 'Ada',
      sorting: [{ field: 'createdAt', descending: true }],
      groupBy: 'regionName',
      filters: [{
        field: 'lastName',
        operator: 'contains',
        value: 'Love',
        valueTo: null,
      }],
    })
  })

  it('normalizes create and update payloads', () => {
    const patient = {
      id: 7,
      inn: ' 12345678901234 ',
      firstName: ' Ada ',
      lastName: ' Lovelace ',
      middleName: ' Byron ',
      birthDate: '1815-12-10T00:00:00Z',
      gender: 2 as const,
      address: ' London ',
      address2: ' Westminster ',
      phone: ' +996000000000 ',
      regionId: 1,
      districtId: 2,
      groupId: 3,
      groupCode: 'archived',
      groupName: 'Archived',
      specialStatus: true,
      createdAt: '2026-07-13T00:00:00Z',
      updatedAt: null,
      isActive: false,
    }

    const createRequest = toCreatePatientRequest(patient)
    expect(createRequest).toMatchObject({
      inn: '12345678901234',
      firstName: 'Ada',
      birthDate: '1815-12-10',
      regionId: 1,
      districtId: 2,
    })
    expect(createRequest).not.toHaveProperty('specialStatus')
    expect(toUpdatePatientRequest(patient)).toMatchObject({
      groupId: 3,
      isActive: false,
      specialStatus: true,
    })
  })

  it('normalizes Date values without a timezone day shift', () => {
    const result = toCreatePatientRequest({
      address: 'Address',
      address2: 'Address 2',
      birthDate: new Date(2000, 0, 2),
      districtId: 2,
      firstName: 'Ada',
      gender: 2,
      inn: '12345678901234',
      lastName: 'Lovelace',
      middleName: 'Byron',
      phone: '+996000000000',
      regionId: 1,
      specialStatus: false,
    })

    expect(result.birthDate).toBe('2000-01-02')
  })

  it('rejects values that violate the patient contract', () => {
    expect(() => toCreatePatientRequest({
      address: 'Address',
      address2: 'Address 2',
      birthDate: '2999-01-01',
      districtId: 2,
      firstName: 'Ada',
      gender: 2,
      inn: 'invalid',
      lastName: 'Lovelace',
      middleName: 'Byron',
      phone: '+996000000000',
      regionId: 1,
      specialStatus: false,
    })).toThrow()
  })
})
