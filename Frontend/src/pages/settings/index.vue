<script setup lang="ts">
import { RefreshCwIcon, SaveIcon } from '@lucide/vue'
import { storeToRefs } from 'pinia'
import { toast } from 'vue-sonner'

import { settingsApi } from '@/services/api/dialysis.api'
import { useAuthStore } from '@/stores/auth'

import SettingsLayout from './components/settings-layout.vue'

const authStore = useAuthStore()
const { activeTenant } = storeToRefs(authStore)

const loading = ref(false)
const saving = ref(false)

const form = reactive({
  identificationStartLimitMinutes: 240,
  autoFinishActiveMinutes: 270,
  endIdentificationLimitMinutes: 120,
  sendToPayLimitMinutes: 360,
})

const settingRows = computed(() => [
  {
    key: 'identificationStartLimitMinutes',
    title: 'Identification start limit',
    description: 'Maximum minutes after Identified before the session expires if it is not started.',
  },
  {
    key: 'autoFinishActiveMinutes',
    title: 'Auto finish active minutes',
    description: 'Active treatment minutes counted without paused time before automatic Finished status.',
  },
  {
    key: 'endIdentificationLimitMinutes',
    title: 'End identification limit',
    description: 'Maximum minutes after Finished before EndIdentified becomes overdue.',
  },
  {
    key: 'sendToPayLimitMinutes',
    title: 'Send to pay limit',
    description: 'Maximum minutes after EndIdentified before payment sending becomes overdue.',
  },
] as const)

async function loadSettings() {
  loading.value = true
  try {
    const settings = await settingsApi.getSessionWorkflow()
    Object.assign(form, {
      identificationStartLimitMinutes: settings.identificationStartLimitMinutes,
      autoFinishActiveMinutes: settings.autoFinishActiveMinutes,
      endIdentificationLimitMinutes: settings.endIdentificationLimitMinutes,
      sendToPayLimitMinutes: settings.sendToPayLimitMinutes,
    })
  }
  catch {
    toast.error('Settings could not be loaded')
  }
  finally {
    loading.value = false
  }
}

async function saveSettings() {
  saving.value = true
  try {
    await settingsApi.updateSessionWorkflow({
      identificationStartLimitMinutes: Number(form.identificationStartLimitMinutes),
      autoFinishActiveMinutes: Number(form.autoFinishActiveMinutes),
      endIdentificationLimitMinutes: Number(form.endIdentificationLimitMinutes),
      sendToPayLimitMinutes: Number(form.sendToPayLimitMinutes),
    })
    toast.success('Settings saved')
    await loadSettings()
  }
  catch {
    toast.error('Settings could not be saved')
  }
  finally {
    saving.value = false
  }
}

onMounted(loadSettings)
</script>

<template>
  <SettingsLayout>
    <UiCard>
      <UiCardHeader>
        <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <UiCardTitle>Session workflow settings</UiCardTitle>
            <UiCardDescription>
              Active tenant: {{ activeTenant?.name ?? 'not selected' }}
            </UiCardDescription>
          </div>
          <div class="flex gap-2">
            <UiButton variant="outline" :disabled="loading" @click="loadSettings">
              <RefreshCwIcon class="mr-2 size-4" />
              Refresh
            </UiButton>
            <UiButton :disabled="saving || !activeTenant" @click="saveSettings">
              <SaveIcon class="mr-2 size-4" />
              Save
            </UiButton>
          </div>
        </div>
      </UiCardHeader>
      <UiCardContent>
        <form class="space-y-4" @submit.prevent="saveSettings">
          <div
            v-for="row in settingRows"
            :key="row.key"
            class="grid gap-3 rounded-md border p-4 md:grid-cols-[minmax(0,1fr)_160px]"
          >
            <div>
              <div class="font-medium">
                {{ row.title }}
              </div>
              <div class="text-sm text-muted-foreground">
                {{ row.description }}
              </div>
            </div>
            <div class="grid gap-2">
              <UiLabel :for="row.key">
                Minutes
              </UiLabel>
              <UiInput
                :id="row.key"
                v-model.number="form[row.key]"
                type="number"
                min="1"
                max="1440"
                required
              />
            </div>
          </div>

          <div class="flex justify-end">
            <UiButton type="submit" :disabled="saving || !activeTenant">
              <SaveIcon class="mr-2 size-4" />
              Save settings
            </UiButton>
          </div>
        </form>
      </UiCardContent>
    </UiCard>
  </SettingsLayout>
</template>
