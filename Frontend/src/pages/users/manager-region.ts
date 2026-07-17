import type { AdminRole } from '@/services/types/dialysis'

const MANAGER_ROLE_CODE = 'Manager'

export function getManagerRoleId(roles: AdminRole[]): number | null {
  return roles.find(role => role.code === MANAGER_ROLE_CODE)?.id ?? null
}

export function hasSelectedManagerRole(roles: AdminRole[], roleIds: number[]): boolean {
  const managerRoleId = getManagerRoleId(roles)
  return managerRoleId !== null && roleIds.includes(managerRoleId)
}

export function getManagerRegionIdForPayload(
  roles: AdminRole[],
  roleIds: number[],
  managerRegionId: number | null,
): number | null {
  return hasSelectedManagerRole(roles, roleIds) ? managerRegionId : null
}
