<script setup lang="ts">
import { EditIcon, PlusIcon, RefreshCwIcon, SaveIcon, Trash2Icon, XIcon } from '@lucide/vue'
import { toast } from 'vue-sonner'

import { BasicPage } from '@/components/global-layout'
import { formatDateTime } from '@/lib/dialysis'
import { adminUserApi } from '@/services/api/dialysis.api'

import type { AdminRole, AdminUser, CreateAdminUserRequest, UpdateAdminUserRequest } from '@/services/types/dialysis'

const loading = ref(false)
const saving = ref(false)
const deactivatingId = ref<number | null>(null)
const users = ref<AdminUser[]>([])
const roles = ref<AdminRole[]>([])
const search = ref('')
const editingId = ref<number | null>(null)
const selectedRoleIds = ref<number[]>([])

const form = reactive({
  username: '',
  password: '',
  firstName: '',
  lastName: '',
  isActive: true,
})

const filteredUsers = computed(() => {
  const query = search.value.trim().toLowerCase()
  if (!query) {
    return users.value
  }

  return users.value.filter(user =>
    user.username.toLowerCase().includes(query)
    || user.firstName.toLowerCase().includes(query)
    || user.lastName.toLowerCase().includes(query)
    || user.roles.some(role => role.name.toLowerCase().includes(query)),
  )
})

const formTitle = computed(() => editingId.value ? `Edit user #${editingId.value}` : 'Create user')

function resetForm() {
  editingId.value = null
  selectedRoleIds.value = []
  Object.assign(form, {
    username: '',
    password: '',
    firstName: '',
    lastName: '',
    isActive: true,
  })
}

function editUser(user: AdminUser) {
  editingId.value = user.id
  selectedRoleIds.value = user.roles.map(role => role.id)
  Object.assign(form, {
    username: user.username,
    password: '',
    firstName: user.firstName,
    lastName: user.lastName,
    isActive: user.isActive,
  })
}

function toggleRole(roleId: number, checked: boolean) {
  selectedRoleIds.value = checked
    ? [...new Set([...selectedRoleIds.value, roleId])]
    : selectedRoleIds.value.filter(id => id !== roleId)
}

function createPayload(): CreateAdminUserRequest {
  return {
    username: form.username.trim(),
    password: form.password,
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    isActive: form.isActive,
    roleIds: selectedRoleIds.value,
  }
}

function updatePayload(): UpdateAdminUserRequest {
  return {
    username: form.username.trim(),
    password: form.password.trim() || null,
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    isActive: form.isActive,
    roleIds: selectedRoleIds.value,
  }
}

async function loadData() {
  loading.value = true
  try {
    const [nextUsers, nextRoles] = await Promise.all([
      adminUserApi.list(),
      adminUserApi.roles(),
    ])
    users.value = nextUsers
    roles.value = nextRoles
  }
  catch {
    toast.error('Users could not be loaded')
  }
  finally {
    loading.value = false
  }
}

async function saveUser() {
  if (selectedRoleIds.value.length === 0) {
    toast.error('Select at least one role')
    return
  }

  if (!editingId.value && form.password.length < 6) {
    toast.error('Password must be at least 6 characters')
    return
  }

  saving.value = true
  try {
    if (editingId.value) {
      await adminUserApi.update(editingId.value, updatePayload())
      toast.success('User updated')
    }
    else {
      await adminUserApi.create(createPayload())
      toast.success('User created')
    }

    resetForm()
    await loadData()
  }
  catch {
    toast.error('User could not be saved')
  }
  finally {
    saving.value = false
  }
}

async function deactivateUser(user: AdminUser) {
  // eslint-disable-next-line no-alert
  if (!window.confirm(`Deactivate user ${user.username}?`)) {
    return
  }

  deactivatingId.value = user.id
  try {
    await adminUserApi.deactivate(user.id)
    toast.success('User deactivated')
    await loadData()
  }
  catch {
    toast.error('User could not be deactivated')
  }
  finally {
    deactivatingId.value = null
  }
}

onMounted(loadData)
</script>

<template>
  <BasicPage
    title="Users"
    description="Manage application users and roles"
    sticky
  >
    <template #actions>
      <UiButton variant="outline" @click="loadData">
        <RefreshCwIcon class="mr-2 size-4" />
        Refresh
      </UiButton>
      <UiButton @click="resetForm">
        <PlusIcon class="mr-2 size-4" />
        New
      </UiButton>
    </template>

    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_380px]">
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>User list</UiCardTitle>
          <UiCardDescription>
            Users are protected by role-based permissions.
          </UiCardDescription>
        </UiCardHeader>
        <UiCardContent class="space-y-4">
          <UiInput v-model="search" placeholder="Search users" />

          <div class="overflow-x-auto rounded-md border">
            <table class="w-full min-w-[900px] text-sm">
              <thead class="bg-muted/60 text-left">
                <tr>
                  <th class="px-3 py-2 font-medium">
                    User
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Roles
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Last login
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Status
                  </th>
                  <th class="px-3 py-2 text-right font-medium">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="loading">
                  <td colspan="5" class="px-3 py-8 text-center text-muted-foreground">
                    Loading users...
                  </td>
                </tr>
                <tr v-else-if="filteredUsers.length === 0">
                  <td colspan="5" class="px-3 py-8 text-center text-muted-foreground">
                    No users found.
                  </td>
                </tr>
                <template v-else>
                  <tr
                    v-for="user in filteredUsers"
                    :key="user.id"
                    class="border-t"
                  >
                    <td class="px-3 py-2">
                      <div class="font-medium">
                        {{ user.username }}
                      </div>
                      <div class="text-xs text-muted-foreground">
                        {{ user.firstName }} {{ user.lastName }}
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      <div class="flex flex-wrap gap-1">
                        <UiBadge
                          v-for="role in user.roles"
                          :key="role.id"
                          variant="secondary"
                        >
                          {{ role.name }}
                        </UiBadge>
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      {{ formatDateTime(user.lastLoginAt) }}
                    </td>
                    <td class="px-3 py-2">
                      <UiBadge :variant="user.isActive ? 'default' : 'secondary'">
                        {{ user.isActive ? 'Active' : 'Inactive' }}
                      </UiBadge>
                    </td>
                    <td class="px-3 py-2">
                      <div class="flex justify-end gap-2">
                        <UiButton size="sm" variant="outline" @click="editUser(user)">
                          <EditIcon class="size-4" />
                        </UiButton>
                        <UiButton
                          size="sm"
                          variant="destructive"
                          :disabled="deactivatingId === user.id || !user.isActive"
                          @click="deactivateUser(user)"
                        >
                          <Trash2Icon class="size-4" />
                        </UiButton>
                      </div>
                    </td>
                  </tr>
                </template>
              </tbody>
            </table>
          </div>
        </UiCardContent>
      </UiCard>

      <UiCard>
        <UiCardHeader>
          <UiCardTitle>{{ formTitle }}</UiCardTitle>
          <UiCardDescription>
            Password is required only for new users.
          </UiCardDescription>
        </UiCardHeader>
        <UiCardContent>
          <form class="space-y-4" @submit.prevent="saveUser">
            <div class="grid gap-2">
              <UiLabel for="username">
                Username
              </UiLabel>
              <UiInput id="username" v-model="form.username" required />
            </div>
            <div class="grid gap-2">
              <UiLabel for="password">
                Password
              </UiLabel>
              <UiInput
                id="password"
                v-model="form.password"
                type="password"
                :required="!editingId"
                autocomplete="new-password"
              />
            </div>
            <div class="grid gap-3 md:grid-cols-2">
              <div class="grid gap-2">
                <UiLabel for="firstName">
                  First name
                </UiLabel>
                <UiInput id="firstName" v-model="form.firstName" required />
              </div>
              <div class="grid gap-2">
                <UiLabel for="lastName">
                  Last name
                </UiLabel>
                <UiInput id="lastName" v-model="form.lastName" required />
              </div>
            </div>

            <div class="space-y-2">
              <UiLabel>Roles</UiLabel>
              <div class="grid gap-2 rounded-md border p-3">
                <label
                  v-for="role in roles"
                  :key="role.id"
                  class="flex items-center gap-2 text-sm"
                >
                  <input
                    class="size-4"
                    type="checkbox"
                    :checked="selectedRoleIds.includes(role.id)"
                    @change="toggleRole(role.id, ($event.target as HTMLInputElement).checked)"
                  >
                  <span>{{ role.name }}</span>
                  <span class="text-xs text-muted-foreground">{{ role.code }}</span>
                </label>
              </div>
            </div>

            <label class="flex items-center gap-2 rounded-md border p-3 text-sm">
              <input v-model="form.isActive" class="size-4" type="checkbox">
              Active
            </label>

            <div class="flex justify-end gap-2">
              <UiButton type="button" variant="outline" @click="resetForm">
                <XIcon class="mr-2 size-4" />
                Clear
              </UiButton>
              <UiButton type="submit" :disabled="saving">
                <SaveIcon class="mr-2 size-4" />
                Save
              </UiButton>
            </div>
          </form>
        </UiCardContent>
      </UiCard>
    </div>
  </BasicPage>
</template>
