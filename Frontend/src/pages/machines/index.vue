<script setup lang="ts">
import { EditIcon, PlusIcon, RefreshCwIcon, SaveIcon, Trash2Icon, XIcon } from '@lucide/vue'
import { storeToRefs } from 'pinia'
import { toast } from 'vue-sonner'

import type { MachineAcquisitionType, MedCenterMachine, UpsertMedCenterMachineRequest } from '@/services/types/dialysis'

import { BasicPage } from '@/components/global-layout'
import {
  acquisitionTypeOptions,
  emptyToNull,
  formatDate,
} from '@/lib/dialysis'
import { machineApi } from '@/services/api/dialysis.api'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const { activeTenant } = storeToRefs(authStore)

const loading = ref(false)
const saving = ref(false)
const deletingId = ref<number | null>(null)
const machines = ref<MedCenterMachine[]>([])
const editingId = ref<number | null>(null)

const today = new Date()
const nextYear = new Date(today)
nextYear.setFullYear(today.getFullYear() + 1)

const form = reactive({
  acquisitionType: '1',
  inventoryNumber: '',
  name: '',
  model: '',
  serialNumber: '',
  manufacturer: '',
  manufacturingCountry: '',
  manufactureYear: String(today.getFullYear()),
  certificateHolder: '',
  certificateHolderCountry: '',
  certificateNumber: '',
  certificateCountry: '',
  certificateIssuedAt: today.toISOString().slice(0, 10),
  permitName: '',
  permitNumber: '',
  permitSeries: '',
  permitExpiresAt: nextYear.toISOString().slice(0, 10),
  dailySessionLimit: '4',
  betweenSessionCooldownMinutes: '30',
  dailyLimitCooldownMinutes: '240',
  isApproved: true,
  isActive: true,
})

const formTitle = computed(() => editingId.value ? `Edit machine #${editingId.value}` : 'Create machine')

function resetForm() {
  editingId.value = null
  Object.assign(form, {
    acquisitionType: '1',
    inventoryNumber: '',
    name: '',
    model: '',
    serialNumber: '',
    manufacturer: '',
    manufacturingCountry: '',
    manufactureYear: String(today.getFullYear()),
    certificateHolder: '',
    certificateHolderCountry: '',
    certificateNumber: '',
    certificateCountry: '',
    certificateIssuedAt: today.toISOString().slice(0, 10),
    permitName: '',
    permitNumber: '',
    permitSeries: '',
    permitExpiresAt: nextYear.toISOString().slice(0, 10),
    dailySessionLimit: '4',
    betweenSessionCooldownMinutes: '30',
    dailyLimitCooldownMinutes: '240',
    isApproved: true,
    isActive: true,
  })
}

function editMachine(machine: MedCenterMachine) {
  editingId.value = machine.id
  Object.assign(form, {
    acquisitionType: String(machine.acquisitionType),
    inventoryNumber: machine.inventoryNumber,
    name: machine.name,
    model: machine.model,
    serialNumber: machine.serialNumber,
    manufacturer: machine.manufacturer,
    manufacturingCountry: machine.manufacturingCountry ?? '',
    manufactureYear: String(machine.manufactureYear),
    certificateHolder: machine.certificateHolder ?? '',
    certificateHolderCountry: machine.certificateHolderCountry ?? '',
    certificateNumber: machine.certificateNumber ?? '',
    certificateCountry: machine.certificateCountry ?? '',
    certificateIssuedAt: machine.certificateIssuedAt.slice(0, 10),
    permitName: machine.permitName ?? '',
    permitNumber: machine.permitNumber ?? '',
    permitSeries: machine.permitSeries ?? '',
    permitExpiresAt: machine.permitExpiresAt.slice(0, 10),
    dailySessionLimit: String(machine.dailySessionLimit),
    betweenSessionCooldownMinutes: String(machine.betweenSessionCooldownMinutes),
    dailyLimitCooldownMinutes: String(machine.dailyLimitCooldownMinutes),
    isApproved: machine.isApproved,
    isActive: machine.isActive,
  })
}

function payload(): UpsertMedCenterMachineRequest {
  return {
    acquisitionType: Number(form.acquisitionType) as MachineAcquisitionType,
    inventoryNumber: form.inventoryNumber.trim(),
    name: form.name.trim(),
    model: form.model.trim(),
    serialNumber: form.serialNumber.trim(),
    manufacturer: form.manufacturer.trim(),
    manufacturingCountry: emptyToNull(form.manufacturingCountry),
    manufactureYear: Number(form.manufactureYear),
    certificateHolder: emptyToNull(form.certificateHolder),
    certificateHolderCountry: emptyToNull(form.certificateHolderCountry),
    certificateNumber: emptyToNull(form.certificateNumber),
    certificateCountry: emptyToNull(form.certificateCountry),
    certificateIssuedAt: form.certificateIssuedAt,
    permitName: emptyToNull(form.permitName),
    permitNumber: emptyToNull(form.permitNumber),
    permitSeries: emptyToNull(form.permitSeries),
    permitExpiresAt: form.permitExpiresAt,
    dailySessionLimit: Number(form.dailySessionLimit),
    betweenSessionCooldownMinutes: Number(form.betweenSessionCooldownMinutes),
    dailyLimitCooldownMinutes: Number(form.dailyLimitCooldownMinutes),
    isApproved: form.isApproved,
    isActive: form.isActive,
  }
}

async function loadMachines() {
  loading.value = true
  try {
    machines.value = await machineApi.list()
  }
  catch {
    toast.error('Machines could not be loaded')
  }
  finally {
    loading.value = false
  }
}

async function saveMachine() {
  saving.value = true
  try {
    if (editingId.value) {
      await machineApi.update(editingId.value, payload())
      toast.success('Machine updated')
    }
    else {
      await machineApi.create(payload())
      toast.success('Machine created')
    }

    resetForm()
    await loadMachines()
  }
  catch {
    toast.error('Machine could not be saved')
  }
  finally {
    saving.value = false
  }
}

async function deleteMachine(machine: MedCenterMachine) {
  // eslint-disable-next-line no-alert
  if (!window.confirm(`Delete machine ${machine.name}?`)) {
    return
  }

  deletingId.value = machine.id
  try {
    await machineApi.delete(machine.id)
    toast.success('Machine deleted')
    await loadMachines()
  }
  catch {
    toast.error('Machine could not be deleted')
  }
  finally {
    deletingId.value = null
  }
}

onMounted(loadMachines)
</script>

<template>
  <BasicPage
    title="Machines"
    description="Tenant scoped dialysis machines"
    sticky
  >
    <template #actions>
      <UiButton variant="outline" @click="loadMachines">
        <RefreshCwIcon class="mr-2 size-4" />
        Refresh
      </UiButton>
      <UiButton @click="resetForm">
        <PlusIcon class="mr-2 size-4" />
        New
      </UiButton>
    </template>

    <div class="mb-4 rounded-md border bg-muted/30 px-4 py-3 text-sm">
      Active tenant:
      <span class="font-medium">{{ activeTenant?.name ?? 'not selected' }}</span>
    </div>

    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_460px]">
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>Machine list</UiCardTitle>
          <UiCardDescription>
            Session start validates machine active status, permit expiration, daily load and cooldown windows.
          </UiCardDescription>
        </UiCardHeader>
        <UiCardContent>
          <div class="overflow-x-auto rounded-md border">
            <table class="w-full min-w-[980px] text-sm">
              <thead class="bg-muted/60 text-left">
                <tr>
                  <th class="px-3 py-2 font-medium">
                    Machine
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Serial
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Permit expires
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Daily load
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Cooldowns
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
                  <td colspan="7" class="px-3 py-8 text-center text-muted-foreground">
                    Loading machines...
                  </td>
                </tr>
                <tr v-else-if="machines.length === 0">
                  <td colspan="7" class="px-3 py-8 text-center text-muted-foreground">
                    No machines found.
                  </td>
                </tr>
                <template v-else>
                  <tr
                    v-for="machine in machines"
                    :key="machine.id"
                    class="border-t"
                  >
                    <td class="px-3 py-2">
                      <div class="font-medium">
                        {{ machine.name }}
                      </div>
                      <div class="text-xs text-muted-foreground">
                        {{ machine.manufacturer }} / {{ machine.model }}
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      <div>{{ machine.serialNumber }}</div>
                      <div class="text-xs text-muted-foreground">
                        Inv: {{ machine.inventoryNumber }}
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      {{ formatDate(machine.permitExpiresAt) }}
                    </td>
                    <td class="px-3 py-2">
                      {{ machine.dailySessionLimit }} sessions/day
                    </td>
                    <td class="px-3 py-2">
                      <div>{{ machine.betweenSessionCooldownMinutes }} min between</div>
                      <div class="text-xs text-muted-foreground">
                        {{ machine.dailyLimitCooldownMinutes }} min after limit
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      <UiBadge :variant="machine.isActive && machine.isApproved ? 'default' : 'secondary'">
                        {{ machine.isActive && machine.isApproved ? 'Available' : 'Blocked' }}
                      </UiBadge>
                    </td>
                    <td class="px-3 py-2">
                      <div class="flex justify-end gap-2">
                        <UiButton size="sm" variant="outline" @click="editMachine(machine)">
                          <EditIcon class="size-4" />
                        </UiButton>
                        <UiButton
                          size="sm"
                          variant="destructive"
                          :disabled="deletingId === machine.id"
                          @click="deleteMachine(machine)"
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
            Required legal and load fields are used by session validation.
          </UiCardDescription>
        </UiCardHeader>
        <UiCardContent>
          <form class="space-y-4" @submit.prevent="saveMachine">
            <div class="grid gap-3 md:grid-cols-2">
              <div class="grid gap-2">
                <UiLabel for="acquisitionType">
                  Acquisition
                </UiLabel>
                <select id="acquisitionType" v-model="form.acquisitionType" class="h-9 rounded-md border bg-background px-3 text-sm">
                  <option
                    v-for="option in acquisitionTypeOptions"
                    :key="option.value"
                    :value="option.value"
                  >
                    {{ option.label }}
                  </option>
                </select>
              </div>
              <div class="grid gap-2">
                <UiLabel for="inventoryNumber">
                  Inventory number
                </UiLabel>
                <UiInput id="inventoryNumber" v-model="form.inventoryNumber" required />
              </div>
            </div>
            <div class="grid gap-3 md:grid-cols-2">
              <div class="grid gap-2">
                <UiLabel for="name">
                  Name
                </UiLabel>
                <UiInput id="name" v-model="form.name" required />
              </div>
              <div class="grid gap-2">
                <UiLabel for="model">
                  Model
                </UiLabel>
                <UiInput id="model" v-model="form.model" required />
              </div>
            </div>
            <div class="grid gap-3 md:grid-cols-2">
              <div class="grid gap-2">
                <UiLabel for="serialNumber">
                  Serial number
                </UiLabel>
                <UiInput id="serialNumber" v-model="form.serialNumber" required />
              </div>
              <div class="grid gap-2">
                <UiLabel for="manufacturer">
                  Manufacturer
                </UiLabel>
                <UiInput id="manufacturer" v-model="form.manufacturer" required />
              </div>
            </div>
            <div class="grid gap-3 md:grid-cols-2">
              <div class="grid gap-2">
                <UiLabel for="manufacturingCountry">
                  Manufacturing country
                </UiLabel>
                <UiInput id="manufacturingCountry" v-model="form.manufacturingCountry" />
              </div>
              <div class="grid gap-2">
                <UiLabel for="manufactureYear">
                  Manufacture year
                </UiLabel>
                <UiInput id="manufactureYear" v-model="form.manufactureYear" type="number" min="1980" max="2100" required />
              </div>
            </div>
            <UiSeparator />
            <div class="grid gap-3 md:grid-cols-2">
              <div class="grid gap-2">
                <UiLabel for="certificateIssuedAt">
                  Certificate issued
                </UiLabel>
                <UiInput id="certificateIssuedAt" v-model="form.certificateIssuedAt" type="date" required />
              </div>
              <div class="grid gap-2">
                <UiLabel for="permitExpiresAt">
                  Permit expires
                </UiLabel>
                <UiInput id="permitExpiresAt" v-model="form.permitExpiresAt" type="date" required />
              </div>
            </div>
            <div class="grid gap-3 md:grid-cols-2">
              <div class="grid gap-2">
                <UiLabel for="certificateNumber">
                  Certificate number
                </UiLabel>
                <UiInput id="certificateNumber" v-model="form.certificateNumber" />
              </div>
              <div class="grid gap-2">
                <UiLabel for="certificateCountry">
                  Certificate country
                </UiLabel>
                <UiInput id="certificateCountry" v-model="form.certificateCountry" />
              </div>
            </div>
            <div class="grid gap-3 md:grid-cols-2">
              <div class="grid gap-2">
                <UiLabel for="certificateHolder">
                  Certificate holder
                </UiLabel>
                <UiInput id="certificateHolder" v-model="form.certificateHolder" />
              </div>
              <div class="grid gap-2">
                <UiLabel for="certificateHolderCountry">
                  Holder country
                </UiLabel>
                <UiInput id="certificateHolderCountry" v-model="form.certificateHolderCountry" />
              </div>
            </div>
            <div class="grid gap-3 md:grid-cols-3">
              <div class="grid gap-2">
                <UiLabel for="permitName">
                  Permit name
                </UiLabel>
                <UiInput id="permitName" v-model="form.permitName" />
              </div>
              <div class="grid gap-2">
                <UiLabel for="permitSeries">
                  Permit series
                </UiLabel>
                <UiInput id="permitSeries" v-model="form.permitSeries" />
              </div>
              <div class="grid gap-2">
                <UiLabel for="permitNumber">
                  Permit number
                </UiLabel>
                <UiInput id="permitNumber" v-model="form.permitNumber" />
              </div>
            </div>
            <UiSeparator />
            <div class="grid gap-3 md:grid-cols-3">
              <div class="grid gap-2">
                <UiLabel for="dailySessionLimit">
                  Daily limit
                </UiLabel>
                <UiInput id="dailySessionLimit" v-model="form.dailySessionLimit" type="number" min="1" max="30" required />
              </div>
              <div class="grid gap-2">
                <UiLabel for="betweenSessionCooldownMinutes">
                  Between sessions
                </UiLabel>
                <UiInput id="betweenSessionCooldownMinutes" v-model="form.betweenSessionCooldownMinutes" type="number" min="1" max="1440" required />
              </div>
              <div class="grid gap-2">
                <UiLabel for="dailyLimitCooldownMinutes">
                  After limit
                </UiLabel>
                <UiInput id="dailyLimitCooldownMinutes" v-model="form.dailyLimitCooldownMinutes" type="number" min="1" max="1440" required />
              </div>
            </div>
            <div class="flex flex-wrap gap-4 rounded-md border p-3">
              <label class="flex items-center gap-2 text-sm">
                <input v-model="form.isApproved" type="checkbox" class="size-4">
                Approved
              </label>
              <label class="flex items-center gap-2 text-sm">
                <input v-model="form.isActive" type="checkbox" class="size-4">
                Active
              </label>
            </div>
            <div class="flex justify-end gap-2">
              <UiButton type="button" variant="outline" @click="resetForm">
                <XIcon class="mr-2 size-4" />
                Clear
              </UiButton>
              <UiButton type="submit" :disabled="saving || !activeTenant">
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
