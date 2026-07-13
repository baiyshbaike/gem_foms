<script setup lang="ts">
import { EditIcon, PlusIcon, RefreshCwIcon, SaveIcon, Trash2Icon, XIcon } from '@lucide/vue'
import { storeToRefs } from 'pinia'
import { toast } from 'vue-sonner'

import type { CreateMedCardRequest, MedCard, MedCardStatus, Patient, UpdateMedCardRequest } from '@/services/types/dialysis'

import { BasicPage } from '@/components/global-layout'
import {
  emptyToNull,
  formatDateTime,
  medCardStatusOptions,
  toDateTimeLocal,
  toIsoDateTime,
} from '@/lib/dialysis'
import { medCardApi, patientApi } from '@/services/api/dialysis.api'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const { activeTenant } = storeToRefs(authStore)

const loading = ref(false)
const saving = ref(false)
const deletingId = ref<number | null>(null)
const medCards = ref<MedCard[]>([])
const patients = ref<Patient[]>([])
const editingId = ref<number | null>(null)

const form = reactive({
  patientId: '',
  cardNumber: '',
  openedAt: '',
  closedAt: '',
  status: '2',
  notes: '',
})

const patientOptions = computed(() => patients.value.map(patient => ({
  id: patient.id,
  label: `${patient.inn} - ${patient.lastName} ${patient.firstName}`,
})))

const formTitle = computed(() => editingId.value ? `Edit med card #${editingId.value}` : 'Create med card')

function patientName(patientId: number) {
  const patient = patients.value.find(item => item.id === patientId)
  return patient ? `${patient.lastName} ${patient.firstName}` : `Patient #${patientId}`
}

function resetForm() {
  editingId.value = null
  Object.assign(form, {
    patientId: '',
    cardNumber: '',
    openedAt: toDateTimeLocal(new Date().toISOString()),
    closedAt: '',
    status: '2',
    notes: '',
  })
}

function editMedCard(medCard: MedCard) {
  editingId.value = medCard.id
  Object.assign(form, {
    patientId: String(medCard.patientId),
    cardNumber: medCard.cardNumber,
    openedAt: toDateTimeLocal(medCard.openedAt),
    closedAt: toDateTimeLocal(medCard.closedAt),
    status: String(medCard.status),
    notes: medCard.notes ?? '',
  })
}

function createPayload(): CreateMedCardRequest {
  return {
    patientId: Number(form.patientId),
    cardNumber: form.cardNumber.trim(),
    openedAt: toIsoDateTime(form.openedAt),
    notes: emptyToNull(form.notes),
  }
}

function updatePayload(): UpdateMedCardRequest {
  return {
    cardNumber: form.cardNumber.trim(),
    openedAt: toIsoDateTime(form.openedAt) ?? new Date().toISOString(),
    closedAt: toIsoDateTime(form.closedAt),
    status: Number(form.status) as MedCardStatus,
    notes: emptyToNull(form.notes),
  }
}

async function loadData() {
  loading.value = true
  try {
    const [nextPatients, nextMedCards] = await Promise.all([
      patientApi.list(),
      medCardApi.list(),
    ])
    patients.value = nextPatients
    medCards.value = nextMedCards
  }
  catch {
    toast.error('Med cards could not be loaded')
  }
  finally {
    loading.value = false
  }
}

async function saveMedCard() {
  saving.value = true
  try {
    if (editingId.value) {
      await medCardApi.update(editingId.value, updatePayload())
      toast.success('Med card updated')
    }
    else {
      await medCardApi.create(createPayload())
      toast.success('Med card created')
    }

    resetForm()
    await loadData()
  }
  catch {
    toast.error('Med card could not be saved')
  }
  finally {
    saving.value = false
  }
}

async function deleteMedCard(medCard: MedCard) {
  // eslint-disable-next-line no-alert
  if (!window.confirm(`Delete med card ${medCard.cardNumber}?`)) {
    return
  }

  deletingId.value = medCard.id
  try {
    await medCardApi.delete(medCard.id)
    toast.success('Med card deleted')
    await loadData()
  }
  catch {
    toast.error('Med card could not be deleted')
  }
  finally {
    deletingId.value = null
  }
}

onMounted(() => {
  resetForm()
  loadData()
})
</script>

<template>
  <BasicPage
    title="Med Cards"
    description="Tenant scoped treatment cards"
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

    <div class="mb-4 rounded-md border bg-muted/30 px-4 py-3 text-sm">
      Active tenant:
      <span class="font-medium">{{ activeTenant?.name ?? 'not selected' }}</span>
    </div>

    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_380px]">
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>Med card list</UiCardTitle>
          <UiCardDescription>
            Cards belong to active tenant and point to a global patient.
          </UiCardDescription>
        </UiCardHeader>
        <UiCardContent>
          <div class="overflow-x-auto rounded-md border">
            <table class="w-full min-w-[850px] text-sm">
              <thead class="bg-muted/60 text-left">
                <tr>
                  <th class="px-3 py-2 font-medium">
                    Card
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Patient
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Opened
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Closed
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
                  <td colspan="6" class="px-3 py-8 text-center text-muted-foreground">
                    Loading med cards...
                  </td>
                </tr>
                <tr v-else-if="medCards.length === 0">
                  <td colspan="6" class="px-3 py-8 text-center text-muted-foreground">
                    No med cards found.
                  </td>
                </tr>
                <template v-else>
                  <tr
                    v-for="medCard in medCards"
                    :key="medCard.id"
                    class="border-t"
                  >
                    <td class="px-3 py-2">
                      <div class="font-medium">
                        {{ medCard.cardNumber }}
                      </div>
                      <div class="text-xs text-muted-foreground">
                        #{{ medCard.id }}
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      {{ patientName(medCard.patientId) }}
                    </td>
                    <td class="px-3 py-2">
                      {{ formatDateTime(medCard.openedAt) }}
                    </td>
                    <td class="px-3 py-2">
                      {{ formatDateTime(medCard.closedAt) }}
                    </td>
                    <td class="px-3 py-2">
                      <UiBadge variant="secondary">
                        {{ medCardStatusOptions.find(item => item.value === medCard.status)?.label ?? medCard.status }}
                      </UiBadge>
                    </td>
                    <td class="px-3 py-2">
                      <div class="flex justify-end gap-2">
                        <UiButton size="sm" variant="outline" @click="editMedCard(medCard)">
                          <EditIcon class="size-4" />
                        </UiButton>
                        <UiButton
                          size="sm"
                          variant="destructive"
                          :disabled="deletingId === medCard.id"
                          @click="deleteMedCard(medCard)"
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
            Patient must exist before opening a card.
          </UiCardDescription>
        </UiCardHeader>
        <UiCardContent>
          <form class="space-y-4" @submit.prevent="saveMedCard">
            <div class="grid gap-2">
              <UiLabel for="patientId">
                Patient
              </UiLabel>
              <select
                id="patientId"
                v-model="form.patientId"
                class="h-9 rounded-md border bg-background px-3 text-sm"
                :disabled="!!editingId"
                required
              >
                <option value="" disabled>
                  Select patient
                </option>
                <option
                  v-for="patient in patientOptions"
                  :key="patient.id"
                  :value="patient.id"
                >
                  {{ patient.label }}
                </option>
              </select>
            </div>
            <div class="grid gap-2">
              <UiLabel for="cardNumber">
                Card number
              </UiLabel>
              <UiInput id="cardNumber" v-model="form.cardNumber" required />
            </div>
            <div class="grid gap-2">
              <UiLabel for="openedAt">
                Opened at
              </UiLabel>
              <UiInput id="openedAt" v-model="form.openedAt" type="datetime-local" required />
            </div>
            <div v-if="editingId" class="grid gap-2">
              <UiLabel for="closedAt">
                Closed at
              </UiLabel>
              <UiInput id="closedAt" v-model="form.closedAt" type="datetime-local" />
            </div>
            <div v-if="editingId" class="grid gap-2">
              <UiLabel for="status">
                Status
              </UiLabel>
              <select id="status" v-model="form.status" class="h-9 rounded-md border bg-background px-3 text-sm">
                <option
                  v-for="status in medCardStatusOptions"
                  :key="status.value"
                  :value="status.value"
                >
                  {{ status.label }}
                </option>
              </select>
            </div>
            <div class="grid gap-2">
              <UiLabel for="notes">
                Notes
              </UiLabel>
              <UiTextarea id="notes" v-model="form.notes" rows="4" />
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
