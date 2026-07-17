import { describe, expect, it } from 'vitest'

import type { AdminRole } from '@/services/types/dialysis'

import {
  getManagerRegionIdForPayload,
  getManagerRoleId,
  hasSelectedManagerRole,
} from './manager-region'

const roles: AdminRole[] = [
  { id: 1, code: 'Admin', name: 'Administrator', isSystem: true },
  { id: 2, code: 'Manager', name: 'Manager', isSystem: true },
]

describe('manager region helpers', () => {
  it('detects the Manager role by stable role code', () => {
    expect(getManagerRoleId(roles)).toBe(2)
    expect(hasSelectedManagerRole(roles, [2])).toBe(true)
    expect(hasSelectedManagerRole(roles, [1])).toBe(false)
  })

  it('removes region from non-manager payloads', () => {
    expect(getManagerRegionIdForPayload(roles, [2], 10)).toBe(10)
    expect(getManagerRegionIdForPayload(roles, [1], 10)).toBeNull()
  })
})
