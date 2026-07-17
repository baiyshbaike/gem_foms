import { describe, expect, it } from 'vitest'

import type { Tenant } from './types/dialysis'

import { resolveTenantSelection } from './tenant-selection'

const tenantA: Tenant = { id: 'a', code: 'A', name: 'Tenant A' }
const tenantB: Tenant = { id: 'b', code: 'B', name: 'Tenant B' }

describe('tenant selection reconciliation', () => {
  it('keeps an accessible active tenant', () => {
    expect(resolveTenantSelection(tenantA, [tenantA, tenantB])).toBeUndefined()
  })

  it('selects the first fallback after deactivation', () => {
    expect(resolveTenantSelection(tenantA, [tenantB])).toBe('b')
  })

  it('clears the selection when no tenant remains', () => {
    expect(resolveTenantSelection(tenantA, [])).toBeNull()
  })
})
