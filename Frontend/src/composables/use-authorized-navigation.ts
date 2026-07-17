import type { BaseNavItem, NavGroup, NavItem } from '@/components/app-sidebar/types'

export function filterNavigationByPermissions(
  groups: Readonly<NavGroup[]>,
  permissions: ReadonlyArray<string>,
): NavGroup[] {
  const permissionSet = new Set(permissions)

  return groups
    .map(group => ({
      ...group,
      items: group.items
        .map(item => filterItem(item, permissionSet))
        .filter((item): item is NavItem => item !== null),
    }))
    .filter(group => group.items.length > 0)
}

function filterItem(item: NavItem, permissions: ReadonlySet<string>): NavItem | null {
  if (!isAllowed(item, permissions)) {
    return null
  }

  if (!item.items) {
    return item
  }

  const items = item.items.filter(child => isAllowed(child, permissions))
  return items.length > 0 ? { ...item, items } : null
}

function isAllowed(item: BaseNavItem, permissions: ReadonlySet<string>): boolean {
  return !item.permission || permissions.has(item.permission)
}
