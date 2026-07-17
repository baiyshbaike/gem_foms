<script setup lang="ts">
import { LoaderCircleIcon, SaveIcon } from '@lucide/vue'
import { useForm } from '@tanstack/vue-form'
import axios from 'axios'
import { toast } from 'vue-sonner'

import type {
  Region,
  TenantGridRow,
} from '@/services/types/dialysis'

import { FieldError } from '@/components/ui/field'
import { FormItem } from '@/components/ui/form'
import { tenantApi } from '@/services/api/dialysis.api'

import type { TenantEditorValues } from '../tenant-grid'

import { getActiveTenantDistricts, toCreateTenantRequest, toUpdateTenantRequest } from '../tenant-grid'
import { tenantEditorSchema } from '../tenant-schema'

interface ApiProblemDetails {
  detail?: string
  errors?: Record<string, string[]>
  title?: string
}

const props = defineProps<{
  regions: Region[]
}>()

const emit = defineEmits<{
  saved: [action: 'created' | 'updated']
}>()

const visible = ref(false)
const saving = ref(false)
const editingId = ref<string | null>(null)
const selectedRegionId = ref(0)

const isEditMode = computed(() => editingId.value !== null)
const popupTitle = computed(() => isEditMode.value ? 'Edit tenant' : 'New tenant')
const activeRegions = computed(() => props.regions.filter(region => region.isActive))
const districts = computed(() => getActiveTenantDistricts(props.regions, selectedRegionId.value))

const form = useForm({
  defaultValues: createEmptyTenant(),
  validators: {
    onSubmit: tenantEditorSchema,
  },
  onSubmit: async ({ value }) => {
    saving.value = true
    try {
      if (editingId.value === null) {
        await tenantApi.create(toCreateTenantRequest(value))
        emit('saved', 'created')
      }
      else {
        await tenantApi.update(editingId.value, toUpdateTenantRequest(value))
        emit('saved', 'updated')
      }

      visible.value = false
    }
    catch (error) {
      toast.error(toTenantEditorError(error, 'Tenant could not be saved'))
    }
    finally {
      saving.value = false
    }
  },
})

function createEmptyTenant(): TenantEditorValues {
  return {
    address: '',
    code: '',
    districtId: 0,
    regionId: 0,
    isActive: true,
    name: '',
    phone: '',
  }
}

function openCreate() {
  editingId.value = null
  selectedRegionId.value = 0
  form.reset(createEmptyTenant())
  visible.value = true
}

function openEdit(tenant: TenantGridRow) {
  editingId.value = tenant.id
  selectedRegionId.value = tenant.regionId
  form.reset({
    address: tenant.address ?? '',
    code: tenant.code,
    districtId: tenant.districtId,
    regionId: tenant.regionId,
    isActive: tenant.isActive,
    name: tenant.name,
    phone: tenant.phone,
  })
  visible.value = true
}

function onRegionChanged(change: (value: number) => void, value: unknown) {
  const regionId = Number(value)
  const normalizedId = Number.isInteger(regionId) && regionId > 0 ? regionId : 0
  selectedRegionId.value = normalizedId
  change(normalizedId)
  form.setFieldValue('districtId', 0, { dontValidate: true })
}

function onOpenChange(open: boolean) {
  if (!saving.value) {
    visible.value = open
  }
}

function toTenantEditorError(error: unknown, fallback: string): string {
  if (axios.isAxiosError<ApiProblemDetails>(error)) {
    const problem = error.response?.data
    const validationMessage = problem?.errors
      ? Object.values(problem.errors).flat()[0]
      : undefined
    return validationMessage ?? problem?.detail ?? problem?.title ?? fallback
  }

  return error instanceof Error ? error.message : fallback
}

defineExpose({ openCreate, openEdit })
</script>

<template>
  <UiDialog :open="visible" @update:open="onOpenChange">
    <UiDialogScrollContent class="max-w-[780px] gap-0 p-0">
      <UiDialogHeader class="border-b px-6 py-5 text-left">
        <UiDialogTitle>{{ popupTitle }}</UiDialogTitle>
        <UiDialogDescription>
          Configure the dialysis center and its physical location.
        </UiDialogDescription>
      </UiDialogHeader>

      <form class="flex min-h-0 flex-col" @submit.prevent="form.handleSubmit">
        <div class="grid max-h-[calc(100dvh-220px)] gap-5 overflow-y-auto px-6 py-5 md:grid-cols-2">
          <form.Field name="code">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel for="tenant-code" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Code
                </UiLabel>
                <UiInput
                  id="tenant-code"
                  :model-value="field.state.value"
                  maxlength="100"
                  autocomplete="off"
                  :disabled="saving"
                  :aria-invalid="!!state.meta.errors?.length"
                  placeholder="CENTER-01"
                  class="font-mono uppercase"
                  @input="field.handleChange($event.target.value.toUpperCase())"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="name">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel for="tenant-name" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Name
                </UiLabel>
                <UiInput
                  id="tenant-name"
                  :model-value="field.state.value"
                  maxlength="200"
                  :disabled="saving"
                  :aria-invalid="!!state.meta.errors?.length"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="regionId">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Region
                </UiLabel>
                <UiSelect
                  :model-value="field.state.value || undefined"
                  :disabled="saving"
                  @update:model-value="value => onRegionChanged(field.handleChange, value)"
                >
                  <UiSelectTrigger class="w-full" :aria-invalid="!!state.meta.errors?.length">
                    <UiSelectValue placeholder="Select region" />
                  </UiSelectTrigger>
                  <UiSelectContent>
                    <UiSelectItem v-for="region in activeRegions" :key="region.id" :value="region.id">
                      {{ region.name }}
                    </UiSelectItem>
                  </UiSelectContent>
                </UiSelect>
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="districtId">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  District
                </UiLabel>
                <UiSelect
                  :model-value="field.state.value || undefined"
                  :disabled="saving || !selectedRegionId"
                  @update:model-value="value => field.handleChange(Number(value))"
                >
                  <UiSelectTrigger class="w-full" :aria-invalid="!!state.meta.errors?.length">
                    <UiSelectValue placeholder="Select district" />
                  </UiSelectTrigger>
                  <UiSelectContent>
                    <UiSelectItem v-for="district in districts" :key="district.id" :value="district.id">
                      {{ district.name }}
                    </UiSelectItem>
                  </UiSelectContent>
                </UiSelect>
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="phone">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel for="tenant-phone" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Phone
                </UiLabel>
                <UiInput
                  id="tenant-phone"
                  type="tel"
                  maxlength="50"
                  :model-value="field.state.value ?? ''"
                  :disabled="saving"
                  :aria-invalid="!!state.meta.errors?.length"
                  placeholder="+996 312 00 00 00"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="address">
            <template #default="{ field, state }">
              <FormItem class="md:col-span-2">
                <UiLabel for="tenant-address" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Address
                </UiLabel>
                <UiTextarea
                  id="tenant-address"
                  :model-value="field.state.value ?? ''"
                  maxlength="500"
                  :disabled="saving"
                  :aria-invalid="!!state.meta.errors?.length"
                  class="min-h-20 resize-y"
                  placeholder="Optional"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field v-if="isEditMode" name="isActive">
            <template #default="{ field }">
              <div class="flex items-center justify-between gap-4 rounded-md border p-4 md:col-span-2">
                <div class="grid gap-1">
                  <UiLabel for="tenant-active">
                    Active tenant
                  </UiLabel>
                  <p class="text-xs text-muted-foreground">
                    Inactive tenants cannot be selected for operational work.
                  </p>
                </div>
                <UiSwitch
                  id="tenant-active"
                  :model-value="field.state.value"
                  :disabled="saving"
                  @update:model-value="value => field.handleChange(Boolean(value))"
                />
              </div>
            </template>
          </form.Field>
        </div>

        <UiDialogFooter class="border-t bg-background px-6 py-4">
          <UiButton type="button" variant="outline" :disabled="saving" @click="onOpenChange(false)">
            Cancel
          </UiButton>
          <UiButton type="submit" :disabled="saving">
            <LoaderCircleIcon v-if="saving" class="size-4 animate-spin" />
            <SaveIcon v-else class="size-4" />
            Save tenant
          </UiButton>
        </UiDialogFooter>
      </form>
    </UiDialogScrollContent>
  </UiDialog>
</template>
