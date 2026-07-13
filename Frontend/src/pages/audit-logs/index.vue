<script setup lang="ts">
import { RefreshCwIcon } from '@lucide/vue'
import { toast } from 'vue-sonner'

import type { AuditLog } from '@/services/types/dialysis'

import { BasicPage } from '@/components/global-layout'
import { formatDateTime } from '@/lib/dialysis'
import { auditApi } from '@/services/api/dialysis.api'

const loading = ref(false)
const logs = ref<AuditLog[]>([])

async function loadLogs() {
  loading.value = true
  try {
    logs.value = await auditApi.latest()
  }
  catch {
    toast.error('Audit logs could not be loaded')
  }
  finally {
    loading.value = false
  }
}

onMounted(loadLogs)
</script>

<template>
  <BasicPage
    title="Audit Logs"
    description="User action history"
    sticky
  >
    <template #actions>
      <UiButton variant="outline" @click="loadLogs">
        <RefreshCwIcon class="mr-2 size-4" />
        Refresh
      </UiButton>
    </template>

    <UiCard>
      <UiCardHeader>
        <UiCardTitle>Latest actions</UiCardTitle>
        <UiCardDescription>
          Backend stores action, module, status and failure reason without sensitive payloads.
        </UiCardDescription>
      </UiCardHeader>
      <UiCardContent>
        <div class="overflow-x-auto rounded-md border">
          <table class="w-full min-w-[900px] text-sm">
            <thead class="bg-muted/60 text-left">
              <tr>
                <th class="px-3 py-2 font-medium">
                  Time
                </th>
                <th class="px-3 py-2 font-medium">
                  User
                </th>
                <th class="px-3 py-2 font-medium">
                  Module
                </th>
                <th class="px-3 py-2 font-medium">
                  Action
                </th>
                <th class="px-3 py-2 font-medium">
                  Result
                </th>
                <th class="px-3 py-2 font-medium">
                  Failure
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="6" class="px-3 py-8 text-center text-muted-foreground">
                  Loading audit logs...
                </td>
              </tr>
              <tr v-else-if="logs.length === 0">
                <td colspan="6" class="px-3 py-8 text-center text-muted-foreground">
                  No audit logs found.
                </td>
              </tr>
              <template v-else>
                <tr
                  v-for="log in logs"
                  :key="log.id"
                  class="border-t"
                >
                  <td class="px-3 py-2">
                    {{ formatDateTime(log.createdAt) }}
                  </td>
                  <td class="px-3 py-2">
                    {{ log.usernameSnapshot ?? `User #${log.userId ?? '-'}` }}
                  </td>
                  <td class="px-3 py-2">
                    {{ log.module }}
                  </td>
                  <td class="px-3 py-2">
                    {{ log.action }}
                  </td>
                  <td class="px-3 py-2">
                    <UiBadge :variant="log.succeeded ? 'default' : 'destructive'">
                      {{ log.succeeded ? 'Succeeded' : 'Failed' }}
                    </UiBadge>
                  </td>
                  <td class="px-3 py-2 text-muted-foreground">
                    {{ log.failureReason ?? '-' }}
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>
      </UiCardContent>
    </UiCard>
  </BasicPage>
</template>
