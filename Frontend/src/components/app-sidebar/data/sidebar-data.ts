import { Building2Icon } from '@lucide/vue'

import { useSidebar } from '@/composables/use-sidebar'

import type { SidebarData, Team, User } from '../types'

const user: User = {
  name: 'Dialysis',
  email: 'not signed in',
  avatar: '',
}

const teams: Team[] = [
  {
    name: 'No tenant',
    logo: Building2Icon,
    plan: 'Select tenant',
  },
]

const { navData } = useSidebar()

export const sidebarData: SidebarData = {
  user,
  teams,
  navMain: navData.value!,
}
