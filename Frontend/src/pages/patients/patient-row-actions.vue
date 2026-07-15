<script setup lang="ts">
import { Edit3Icon, Trash2Icon } from '@lucide/vue'

import type { PatientGridRow } from '@/services/types/dialysis'

const props = defineProps<{
  patient: PatientGridRow
  canUpdate: boolean
  canDelete: boolean
  disabled?: boolean
}>()

const emit = defineEmits<{
  edit: [patient: PatientGridRow]
  delete: [patient: PatientGridRow]
}>()
</script>

<template>
  <div class="flex items-center justify-end gap-1">
    <UiTooltipProvider v-if="canUpdate">
      <UiTooltip>
        <UiTooltipTrigger as-child>
          <UiButton
            type="button"
            variant="ghost"
            size="icon-sm"
            :disabled="disabled"
            @click="emit('edit', props.patient)"
          >
            <Edit3Icon class="size-4" />
            <span class="sr-only">Edit patient</span>
          </UiButton>
        </UiTooltipTrigger>
        <UiTooltipContent>Edit patient</UiTooltipContent>
      </UiTooltip>
    </UiTooltipProvider>

    <UiTooltipProvider v-if="canDelete">
      <UiTooltip>
        <UiTooltipTrigger as-child>
          <UiButton
            type="button"
            variant="ghost"
            size="icon-sm"
            class="text-destructive hover:bg-destructive/10 hover:text-destructive"
            :disabled="disabled"
            @click="emit('delete', props.patient)"
          >
            <Trash2Icon class="size-4" />
            <span class="sr-only">Delete patient</span>
          </UiButton>
        </UiTooltipTrigger>
        <UiTooltipContent>Delete patient</UiTooltipContent>
      </UiTooltip>
    </UiTooltipProvider>
  </div>
</template>
