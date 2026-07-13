<script setup lang="ts">
import { ActivityIcon, ArchiveIcon, CheckCircle2Icon, PauseIcon, PlayIcon, RefreshCwIcon, SaveIcon, SendIcon, SquareIcon } from '@lucide/vue'
import { storeToRefs } from 'pinia'
import { toast } from 'vue-sonner'

import type {
  HdSession,
  MedCard,
  MedCenterMachine,
  Patient,
  SessionMeasurementPoint,
  SessionMeasurementRequest,
} from '@/services/types/dialysis'

import { BasicPage } from '@/components/global-layout'
import {
  emptyToNull,
  formatDateTime,
  measurementPointOptions,
  toDateTimeLocal,
  toIsoDateTime,
} from '@/lib/dialysis'
import { machineApi, medCardApi, patientApi, sessionApi } from '@/services/api/dialysis.api'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const { activeTenant } = storeToRefs(authStore)

const loading = ref(false)
const actionLoading = ref('')
const sessions = ref<HdSession[]>([])
const medCards = ref<MedCard[]>([])
const patients = ref<Patient[]>([])
const machines = ref<MedCenterMachine[]>([])
const createMedCardId = ref('')
const selectedSessionId = ref<number | null>(null)
const startMachineId = ref('')
const pauseReason = ref('')
const measurementPoint = ref<SessionMeasurementPoint>('Hour1')

const measurement = reactive({
  sys: '',
  dia: '',
  temp: '',
  ritm: '',
  measuredAt: toDateTimeLocal(new Date().toISOString()),
  note: '',
})

const selectedSession = computed(() => sessions.value.find(item => item.id === selectedSessionId.value) ?? null)

const medCardOptions = computed(() => medCards.value.map(card => ({
  id: card.id,
  label: `${card.cardNumber} - ${patientName(card.patientId)}`,
})))

const activeMachines = computed(() => machines.value.filter(machine => machine.isActive && machine.isApproved))

function patientName(patientId: number) {
  const patient = patients.value.find(item => item.id === patientId)
  return patient ? `${patient.lastName} ${patient.firstName}` : `Patient #${patientId}`
}

function medCardLabel(medCardId: number) {
  const medCard = medCards.value.find(item => item.id === medCardId)
  return medCard ? `${medCard.cardNumber} / ${patientName(medCard.patientId)}` : `Med card #${medCardId}`
}

function machineLabel(machineId: number | null) {
  if (!machineId) {
    return '-'
  }

  const machine = machines.value.find(item => item.id === machineId)
  return machine ? `${machine.name} (${machine.serialNumber})` : `Machine #${machineId}`
}

function clearMeasurement() {
  Object.assign(measurement, {
    sys: '',
    dia: '',
    temp: '',
    ritm: '',
    measuredAt: toDateTimeLocal(new Date().toISOString()),
    note: '',
  })
}

function toMeasurement(required = false): SessionMeasurementRequest | null {
  const hasValue = Boolean(
    measurement.sys
    || measurement.dia
    || measurement.temp
    || measurement.ritm
    || measurement.note,
  )

  if (!required && !hasValue) {
    return null
  }

  return {
    sys: emptyToNull(measurement.sys),
    dia: emptyToNull(measurement.dia),
    temp: emptyToNull(measurement.temp),
    ritm: emptyToNull(measurement.ritm),
    measuredAt: toIsoDateTime(measurement.measuredAt) ?? new Date().toISOString(),
    note: emptyToNull(measurement.note),
  }
}

function requiredMeasurement(): SessionMeasurementRequest {
  return toMeasurement(true) ?? {
    sys: null,
    dia: null,
    temp: null,
    ritm: null,
    measuredAt: new Date().toISOString(),
    note: null,
  }
}

async function loadData() {
  loading.value = true
  try {
    const [nextPatients, nextMedCards, nextMachines, nextSessions] = await Promise.all([
      patientApi.list(),
      medCardApi.list(),
      machineApi.list(),
      sessionApi.list(),
    ])
    patients.value = nextPatients
    medCards.value = nextMedCards
    machines.value = nextMachines
    sessions.value = nextSessions
  }
  catch {
    toast.error('Sessions could not be loaded')
  }
  finally {
    loading.value = false
  }
}

async function createSession() {
  if (!createMedCardId.value) {
    toast.error('Select a med card first')
    return
  }

  actionLoading.value = 'create'
  try {
    const created = await sessionApi.create({ medCardId: Number(createMedCardId.value) })
    selectedSessionId.value = created.id
    createMedCardId.value = ''
    toast.success('Session identified')
    await loadData()
  }
  catch {
    toast.error('Session could not be created')
  }
  finally {
    actionLoading.value = ''
  }
}

async function runAction(action: string, session: HdSession | null = selectedSession.value) {
  if (!session) {
    toast.error('Select a session first')
    return
  }

  actionLoading.value = `${action}:${session.id}`
  try {
    if (action === 'start') {
      if (!startMachineId.value) {
        toast.error('Select a machine first')
        return
      }

      await sessionApi.start(session.id, {
        machineId: Number(startMachineId.value),
        startMeasurement: toMeasurement(false),
      })
    }
    else if (action === 'pause') {
      await sessionApi.pause(session.id, { reason: emptyToNull(pauseReason.value) })
    }
    else if (action === 'resume') {
      await sessionApi.resume(session.id)
    }
    else if (action === 'finish') {
      await sessionApi.finish(session.id, { endMeasurement: toMeasurement(false) })
    }
    else if (action === 'endIdentify') {
      await sessionApi.endIdentify(session.id)
    }
    else if (action === 'sendToPay') {
      await sessionApi.sendToPay(session.id)
    }
    else if (action === 'markPaid') {
      await sessionApi.markPaid(session.id)
    }
    else if (action === 'archive') {
      await sessionApi.archive(session.id)
    }

    toast.success('Session updated')
    clearMeasurement()
    await loadData()
  }
  catch {
    toast.error('Session action failed')
  }
  finally {
    actionLoading.value = ''
  }
}

async function saveMeasurement() {
  if (!selectedSession.value) {
    toast.error('Select a session first')
    return
  }

  actionLoading.value = `measurement:${selectedSession.value.id}`
  try {
    await sessionApi.measurement(selectedSession.value.id, measurementPoint.value, requiredMeasurement())
    toast.success('Measurement saved')
    clearMeasurement()
  }
  catch {
    toast.error('Measurement could not be saved')
  }
  finally {
    actionLoading.value = ''
  }
}

onMounted(loadData)
</script>

<template>
  <BasicPage
    title="Sessions"
    description="HdSession workflow and measurements"
    sticky
  >
    <template #actions>
      <UiButton variant="outline" @click="loadData">
        <RefreshCwIcon class="mr-2 size-4" />
        Refresh
      </UiButton>
    </template>

    <div class="mb-4 rounded-md border bg-muted/30 px-4 py-3 text-sm">
      Active tenant:
      <span class="font-medium">{{ activeTenant?.name ?? 'not selected' }}</span>
    </div>

    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_420px]">
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>Session list</UiCardTitle>
          <UiCardDescription>
            Use row actions for state transitions. Invalid transitions are rejected by backend.
          </UiCardDescription>
        </UiCardHeader>
        <UiCardContent>
          <div class="overflow-x-auto rounded-md border">
            <table class="w-full min-w-[1100px] text-sm">
              <thead class="bg-muted/60 text-left">
                <tr>
                  <th class="px-3 py-2 font-medium">
                    Session
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Med card / patient
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Machine
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Status
                  </th>
                  <th class="px-3 py-2 font-medium">
                    Timing
                  </th>
                  <th class="px-3 py-2 text-right font-medium">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="loading">
                  <td colspan="6" class="px-3 py-8 text-center text-muted-foreground">
                    Loading sessions...
                  </td>
                </tr>
                <tr v-else-if="sessions.length === 0">
                  <td colspan="6" class="px-3 py-8 text-center text-muted-foreground">
                    No sessions found.
                  </td>
                </tr>
                <template v-else>
                  <tr
                    v-for="session in sessions"
                    :key="session.id"
                    class="border-t"
                    :class="selectedSessionId === session.id ? 'bg-muted/40' : ''"
                  >
                    <td class="px-3 py-2">
                      <div class="font-medium">
                        #{{ session.id }}
                      </div>
                      <div class="text-xs text-muted-foreground">
                        Patient #{{ session.patientId }}
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      {{ medCardLabel(session.medCardId) }}
                    </td>
                    <td class="px-3 py-2">
                      {{ machineLabel(session.machineId) }}
                    </td>
                    <td class="px-3 py-2">
                      <UiBadge variant="secondary">
                        {{ session.status }}
                      </UiBadge>
                    </td>
                    <td class="px-3 py-2">
                      <div>Identified: {{ formatDateTime(session.identifiedAt) }}</div>
                      <div class="text-xs text-muted-foreground">
                        Active {{ session.activeMinutes ?? 0 }}m / Pause {{ session.pauseMinutes ?? 0 }}m
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      <div class="flex flex-wrap justify-end gap-2">
                        <UiButton size="sm" variant="outline" @click="selectedSessionId = session.id">
                          Select
                        </UiButton>
                        <UiButton size="sm" variant="outline" @click="runAction('start', session)">
                          <PlayIcon class="size-4" />
                        </UiButton>
                        <UiButton size="sm" variant="outline" @click="runAction('pause', session)">
                          <PauseIcon class="size-4" />
                        </UiButton>
                        <UiButton size="sm" variant="outline" @click="runAction('resume', session)">
                          <ActivityIcon class="size-4" />
                        </UiButton>
                        <UiButton size="sm" variant="outline" @click="runAction('finish', session)">
                          <SquareIcon class="size-4" />
                        </UiButton>
                        <UiButton size="sm" variant="outline" @click="runAction('endIdentify', session)">
                          <CheckCircle2Icon class="size-4" />
                        </UiButton>
                        <UiButton size="sm" variant="outline" @click="runAction('sendToPay', session)">
                          <SendIcon class="size-4" />
                        </UiButton>
                        <UiButton size="sm" variant="outline" @click="runAction('markPaid', session)">
                          Paid
                        </UiButton>
                        <UiButton size="sm" variant="outline" @click="runAction('archive', session)">
                          <ArchiveIcon class="size-4" />
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

      <div class="space-y-4">
        <UiCard>
          <UiCardHeader>
            <UiCardTitle>Create identified session</UiCardTitle>
            <UiCardDescription>
              A session starts as Identified after selecting an open med card.
            </UiCardDescription>
          </UiCardHeader>
          <UiCardContent>
            <form class="space-y-4" @submit.prevent="createSession">
              <div class="grid gap-2">
                <UiLabel for="createMedCardId">
                  Med card
                </UiLabel>
                <select id="createMedCardId" v-model="createMedCardId" class="h-9 rounded-md border bg-background px-3 text-sm" required>
                  <option value="" disabled>
                    Select med card
                  </option>
                  <option
                    v-for="card in medCardOptions"
                    :key="card.id"
                    :value="card.id"
                  >
                    {{ card.label }}
                  </option>
                </select>
              </div>
              <UiButton type="submit" class="w-full" :disabled="actionLoading === 'create' || !activeTenant">
                Create session
              </UiButton>
            </form>
          </UiCardContent>
        </UiCard>

        <UiCard>
          <UiCardHeader>
            <UiCardTitle>Selected session actions</UiCardTitle>
            <UiCardDescription>
              Selected: {{ selectedSession ? `#${selectedSession.id} ${selectedSession.status}` : 'none' }}
            </UiCardDescription>
          </UiCardHeader>
          <UiCardContent class="space-y-4">
            <div class="grid gap-2">
              <UiLabel for="startMachineId">
                Start machine
              </UiLabel>
              <select id="startMachineId" v-model="startMachineId" class="h-9 rounded-md border bg-background px-3 text-sm">
                <option value="" disabled>
                  Select machine
                </option>
                <option
                  v-for="machine in activeMachines"
                  :key="machine.id"
                  :value="machine.id"
                >
                  {{ machine.name }} / {{ machine.serialNumber }}
                </option>
              </select>
            </div>
            <div class="grid gap-2">
              <UiLabel for="pauseReason">
                Pause reason
              </UiLabel>
              <UiTextarea id="pauseReason" v-model="pauseReason" rows="2" />
            </div>
            <div class="grid grid-cols-2 gap-2">
              <UiButton variant="outline" :disabled="!selectedSession" @click="runAction('start')">
                Start
              </UiButton>
              <UiButton variant="outline" :disabled="!selectedSession" @click="runAction('pause')">
                Pause
              </UiButton>
              <UiButton variant="outline" :disabled="!selectedSession" @click="runAction('resume')">
                Resume
              </UiButton>
              <UiButton variant="outline" :disabled="!selectedSession" @click="runAction('finish')">
                Finish
              </UiButton>
              <UiButton variant="outline" :disabled="!selectedSession" @click="runAction('endIdentify')">
                End identify
              </UiButton>
              <UiButton variant="outline" :disabled="!selectedSession" @click="runAction('sendToPay')">
                Send to pay
              </UiButton>
              <UiButton variant="outline" :disabled="!selectedSession" @click="runAction('markPaid')">
                Mark paid
              </UiButton>
              <UiButton variant="outline" :disabled="!selectedSession" @click="runAction('archive')">
                Archive
              </UiButton>
            </div>
          </UiCardContent>
        </UiCard>

        <UiCard>
          <UiCardHeader>
            <UiCardTitle>Measurements</UiCardTitle>
            <UiCardDescription>
              Save start, hourly and end vitals for the selected session.
            </UiCardDescription>
          </UiCardHeader>
          <UiCardContent>
            <form class="space-y-4" @submit.prevent="saveMeasurement">
              <div class="grid gap-2">
                <UiLabel for="measurementPoint">
                  Point
                </UiLabel>
                <select id="measurementPoint" v-model="measurementPoint" class="h-9 rounded-md border bg-background px-3 text-sm">
                  <option
                    v-for="point in measurementPointOptions"
                    :key="point.value"
                    :value="point.value"
                  >
                    {{ point.label }}
                  </option>
                </select>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div class="grid gap-2">
                  <UiLabel for="sys">
                    Sys
                  </UiLabel>
                  <UiInput id="sys" v-model="measurement.sys" />
                </div>
                <div class="grid gap-2">
                  <UiLabel for="dia">
                    Dia
                  </UiLabel>
                  <UiInput id="dia" v-model="measurement.dia" />
                </div>
                <div class="grid gap-2">
                  <UiLabel for="temp">
                    Temp
                  </UiLabel>
                  <UiInput id="temp" v-model="measurement.temp" />
                </div>
                <div class="grid gap-2">
                  <UiLabel for="ritm">
                    Ritm
                  </UiLabel>
                  <UiInput id="ritm" v-model="measurement.ritm" />
                </div>
              </div>
              <div class="grid gap-2">
                <UiLabel for="measuredAt">
                  Measured at
                </UiLabel>
                <UiInput id="measuredAt" v-model="measurement.measuredAt" type="datetime-local" />
              </div>
              <div class="grid gap-2">
                <UiLabel for="measurementNote">
                  Note
                </UiLabel>
                <UiTextarea id="measurementNote" v-model="measurement.note" rows="2" />
              </div>
              <div class="flex justify-end gap-2">
                <UiButton type="button" variant="outline" @click="clearMeasurement">
                  Clear
                </UiButton>
                <UiButton type="submit" :disabled="!selectedSession">
                  <SaveIcon class="mr-2 size-4" />
                  Save
                </UiButton>
              </div>
            </form>
          </UiCardContent>
        </UiCard>
      </div>
    </div>
  </BasicPage>
</template>
