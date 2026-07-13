import { ActivityIcon, BellDotIcon, ClipboardListIcon, HeartPulseIcon, LayoutDashboardIcon, LogsIcon, MonitorCogIcon, PaletteIcon, PictureInPicture2Icon, SettingsIcon, StethoscopeIcon, UserIcon, UsersIcon, WrenchIcon } from '@lucide/vue'

import type { NavGroup } from '@/components/app-sidebar/types'

export function useSidebar() {
  const settingsNavItems = [
    { title: 'Profile', url: '/settings/', icon: UserIcon },
    { title: 'Account', url: '/settings/account', icon: WrenchIcon },
    { title: 'Appearance', url: '/settings/appearance', icon: PaletteIcon },
    { title: 'Notifications', url: '/settings/notifications', icon: BellDotIcon },
    { title: 'Display', url: '/settings/display', icon: PictureInPicture2Icon },
  ]

  const navData = ref<NavGroup[]> ([
    {
      title: 'Dialysis',
      items: [
        { title: 'Dashboard', url: '/dashboard', icon: LayoutDashboardIcon },
        { title: 'Patients', url: '/patients', icon: UsersIcon },
        { title: 'Med Cards', url: '/med-cards', icon: HeartPulseIcon },
        { title: 'Machines', url: '/machines', icon: MonitorCogIcon },
        { title: 'Sessions', url: '/sessions', icon: ActivityIcon },
        { title: 'Audit Logs', url: '/audit-logs', icon: LogsIcon },
      ],
    },
    {
      title: 'Administration',
      items: [
        { title: 'Users', url: '/users', icon: UserIcon },
        { title: 'Settings', items: settingsNavItems, icon: SettingsIcon },
      ],
    },
    {
      title: 'Reference',
      items: [
        { title: 'Tasks', url: '/tasks', icon: ClipboardListIcon },
        { title: 'Help Center', url: '/help-center', icon: StethoscopeIcon },
      ],
    },
  ])

  const otherPages = ref<NavGroup[]>([
    {
      title: 'Other',
      items: [
        { title: 'Settings', icon: SettingsIcon, url: '/settings' },
      ],
    },
  ])

  return {
    navData,
    otherPages,
    settingsNavItems,
  }
}
