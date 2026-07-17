import { describe, expect, it } from 'vitest'

import type { NavGroup } from '@/components/app-sidebar/types'

import { filterNavigationByPermissions } from './use-authorized-navigation'

const navigation: NavGroup[] = [
  {
    title: 'Administration',
    items: [
      { title: 'Tenants', url: '/tenants', permission: 'admin.tenants' },
      { title: 'Profile', url: '/profile' },
      {
        title: 'Settings',
        items: [
          { title: 'Audit', url: '/audit', permission: 'admin.audit' },
        ],
      },
    ],
  },
]

describe('permission-aware navigation', () => {
  it('removes unauthorized leaves and empty groups', () => {
    const result = filterNavigationByPermissions(navigation, [])

    expect(result[0].items.map(item => item.title)).toEqual(['Profile'])
  })

  it('keeps authorized pages and nested groups', () => {
    const result = filterNavigationByPermissions(navigation, ['admin.tenants', 'admin.audit'])

    expect(result[0].items.map(item => item.title)).toEqual(['Tenants', 'Profile', 'Settings'])
  })
})
