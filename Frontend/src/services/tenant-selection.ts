import type { Tenant } from './types/dialysis'

export function resolveTenantSelection(
  activeTenant: Tenant | null,
  tenants: Tenant[],
): string | null | undefined {
  if (activeTenant && tenants.some(tenant => tenant.id === activeTenant.id)) {
    return undefined
  }

  return tenants[0]?.id ?? null
}
