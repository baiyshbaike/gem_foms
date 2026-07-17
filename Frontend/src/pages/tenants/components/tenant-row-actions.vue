<script setup lang="ts">
import { Edit3Icon, PowerIcon } from '@lucide/vue'

import type { TenantGridRow } from '@/services/types/dialysis'

const props = defineProps<{
  tenant: TenantGridRow
  disabled?: boolean
}>()

const emit = defineEmits<{
  edit: [tenant: TenantGridRow]
  deactivate: [tenant: TenantGridRow]
}>()
</script>

<template>
  <div class="flex items-center justify-end gap-1">
    <UiTooltipProvider>
      <UiTooltip>
        <UiTooltipTrigger as-child>
          <UiButton
            type="button"
            variant="ghost"
            size="icon-sm"
            :disabled="disabled"
            @click="emit('edit', props.tenant)"
          >
            <Edit3Icon class="size-4" />
            <span class="sr-only">Edit tenant</span>
          </UiButton>
        </UiTooltipTrigger>
        <UiTooltipContent>Edit tenant</UiTooltipContent>
      </UiTooltip>
    </UiTooltipProvider>

    <UiTooltipProvider v-if="tenant.isActive">
      <UiTooltip>
        <UiTooltipTrigger as-child>
          <UiButton
            type="button"
            variant="ghost"
            size="icon-sm"
            class="text-destructive hover:bg-destructive/10 hover:text-destructive"
            :disabled="disabled"
            @click="emit('deactivate', props.tenant)"
          >
            <PowerIcon class="size-4" />
            <span class="sr-only">Deactivate tenant</span>
          </UiButton>
        </UiTooltipTrigger>
        <UiTooltipContent>Deactivate tenant</UiTooltipContent>
      </UiTooltip>
    </UiTooltipProvider>
  </div>
</template>
