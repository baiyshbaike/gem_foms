<script setup lang="ts">
import type { z } from 'zod'

import { LoaderCircleIcon, SaveIcon, SearchIcon } from '@lucide/vue'
import { useForm } from '@tanstack/vue-form'
import axios from 'axios'
import { toast } from 'vue-sonner'

import type {
  PatientGridRow,
  PatientGroup,
  Region,
} from '@/services/types/dialysis'

import { FieldError } from '@/components/ui/field'
import { FormItem } from '@/components/ui/form'
import { patientApi } from '@/services/api/dialysis.api'

import {
  toCreatePatientRequest,
  toUpdatePatientRequest,
} from './patient-grid'
import { derivePatientIdentityFromInn } from './patient-identity'
import { updatePatientSchema } from './patient-schema'

type LookupState = 'idle' | 'searching' | 'found' | 'manual'
type PatientEditorValues = z.input<typeof updatePatientSchema>

interface ApiProblemDetails {
  detail?: string
  errors?: Record<string, string[]>
  title?: string
}

const props = defineProps<{
  groups: PatientGroup[]
  regions: Region[]
}>()

const emit = defineEmits<{
  saved: [action: 'created' | 'updated']
}>()

const visible = ref(false)
const saving = ref(false)
const editingId = ref<number | null>(null)
const lookupState = ref<LookupState>('idle')
const searchedInn = ref('')
const lookupError = ref('')
const selectedRegionId = ref(0)

const isEditMode = computed(() => editingId.value !== null)
const lookupCompleted = computed(() => lookupState.value === 'found' || lookupState.value === 'manual')
const identityFieldsLocked = computed(() => !isEditMode.value && lookupState.value === 'found')
const detailsLocked = computed(() => saving.value || (!isEditMode.value && !lookupCompleted.value))
const saveDisabled = computed(() => saving.value || (!isEditMode.value && !lookupCompleted.value))
const popupTitle = computed(() => isEditMode.value ? 'Edit patient' : 'New patient')
const districts = computed(() => props.regions
  .find(region => region.id === selectedRegionId.value)
  ?.districts
  .filter(district => district.isActive) ?? [])

const form = useForm({
  defaultValues: createEmptyPatient(),
  validators: {
    onSubmit: updatePatientSchema,
  },
  onSubmit: async ({ value }) => {
    saving.value = true
    try {
      if (editingId.value === null) {
        await patientApi.create(toCreatePatientRequest(value))
        emit('saved', 'created')
      }
      else {
        await patientApi.update(editingId.value, toUpdatePatientRequest(value))
        emit('saved', 'updated')
      }

      visible.value = false
    }
    catch (error) {
      toast.error(toPatientEditorError(error, 'Patient could not be saved').message)
    }
    finally {
      saving.value = false
    }
  },
})

function createEmptyPatient(): PatientEditorValues {
  const newGroupId = props?.groups?.find(group => group.code.toLowerCase() === 'new')?.id
    ?? props?.groups?.[0]?.id
    ?? 1

  return {
    address: '',
    address2: '',
    birthDate: '',
    districtId: 0,
    firstName: '',
    gender: 1,
    groupId: newGroupId,
    inn: '',
    isActive: true,
    lastName: '',
    middleName: '',
    phone: '',
    regionId: 0,
    specialStatus: false,
  }
}

function openCreate() {
  editingId.value = null
  lookupState.value = 'idle'
  searchedInn.value = ''
  lookupError.value = ''
  selectedRegionId.value = 0
  form.reset(createEmptyPatient())
  visible.value = true
}

function openEdit(patient: PatientGridRow) {
  editingId.value = patient.id
  lookupState.value = 'idle'
  searchedInn.value = ''
  lookupError.value = ''
  selectedRegionId.value = patient.regionId
  form.reset({
    address: patient.address,
    address2: patient.address2,
    birthDate: patient.birthDate.slice(0, 10),
    districtId: patient.districtId,
    firstName: patient.firstName,
    gender: patient.gender,
    groupId: patient.groupId,
    inn: patient.inn,
    isActive: patient.isActive,
    lastName: patient.lastName,
    middleName: patient.middleName,
    phone: patient.phone,
    regionId: patient.regionId,
    specialStatus: patient.specialStatus,
  })
  visible.value = true
}

function onOpenChange(open: boolean) {
  if (!saving.value) {
    visible.value = open
  }
}

function onInnInput(change: (value: string) => void, event: Event) {
  const normalizedInn = (event.target as HTMLInputElement).value.replace(/\D/g, '').slice(0, 14)

  change(normalizedInn)
  lookupError.value = ''

  if (!isEditMode.value && searchedInn.value && normalizedInn !== searchedInn.value) {
    lookupState.value = 'idle'
    searchedInn.value = ''
    clearIdentityFields()
  }
}

function onRegionChanged(change: (value: number) => void, value: unknown) {
  const regionId = Number(value)
  const normalizedRegionId = Number.isInteger(regionId) && regionId > 0 ? regionId : 0

  selectedRegionId.value = normalizedRegionId
  change(normalizedRegionId)
  form.setFieldValue('districtId', 0)
}

async function lookupIdentity() {
  const inn = form.getFieldValue('inn')
  const derivedIdentity = derivePatientIdentityFromInn(inn)
  if (!derivedIdentity) {
    lookupError.value = 'INN must contain a valid 14-digit identity number'
    return
  }

  lookupState.value = 'searching'
  searchedInn.value = inn
  lookupError.value = ''

  let externalIdentity = null
  try {
    externalIdentity = await patientApi.lookupIdentity(inn)
  }
  catch {
    // The local INN derivation remains usable when the external provider is unavailable.
  }

  if (form.getFieldValue('inn') !== inn) {
    return
  }

  setIdentityFields({
    birthDate: derivedIdentity.birthDate,
    gender: derivedIdentity.gender,
    firstName: externalIdentity?.found ? externalIdentity.firstName ?? '' : '',
    lastName: externalIdentity?.found ? externalIdentity.lastName ?? '' : '',
    middleName: externalIdentity?.found ? externalIdentity.middleName ?? '' : '',
  })
  lookupState.value = externalIdentity?.found ? 'found' : 'manual'
}

function setIdentityFields(values: Pick<PatientEditorValues, 'birthDate' | 'gender' | 'firstName' | 'lastName' | 'middleName'>) {
  const options = { dontUpdateMeta: true, dontValidate: true }
  form.setFieldValue('birthDate', values.birthDate, options)
  form.setFieldValue('gender', values.gender, options)
  form.setFieldValue('firstName', values.firstName, options)
  form.setFieldValue('lastName', values.lastName, options)
  form.setFieldValue('middleName', values.middleName, options)
}

function clearIdentityFields() {
  setIdentityFields({
    birthDate: '',
    firstName: '',
    gender: 1,
    lastName: '',
    middleName: '',
  })
}

function toPatientEditorError(error: unknown, fallback: string): Error {
  if (axios.isAxiosError<ApiProblemDetails>(error)) {
    const problem = error.response?.data
    const validationMessage = problem?.errors
      ? Object.values(problem.errors).flat()[0]
      : undefined

    return new Error(validationMessage ?? problem?.detail ?? problem?.title ?? fallback)
  }

  return error instanceof Error ? error : new Error(fallback)
}

defineExpose({ openCreate, openEdit })
</script>

<template>
  <UiDialog :open="visible" @update:open="onOpenChange">
    <UiDialogScrollContent class="max-w-[760px] gap-0 p-0">
      <UiDialogHeader class="border-b px-6 py-5 text-left">
        <UiDialogTitle>{{ popupTitle }}</UiDialogTitle>
        <UiDialogDescription>
          {{ isEditMode ? 'Update identity, contact and registry details.' : 'Find the identity first, then complete contact and address details.' }}
        </UiDialogDescription>
      </UiDialogHeader>

      <form class="flex min-h-0 flex-col" @submit.prevent="form.handleSubmit">
        <div class="grid max-h-[calc(100dvh-220px)] gap-5 overflow-y-auto px-6 py-5 md:grid-cols-2">
          <form.Field name="inn">
            <template #default="{ field, state }">
              <FormItem class="md:col-span-2">
                <UiLabel
                  for="patient-inn"
                  :data-error="!!state.meta.errors?.length || !!lookupError"
                  class="data-[error=true]:text-destructive"
                >
                  INN
                </UiLabel>
                <div class="flex items-center gap-2">
                  <UiInput
                    id="patient-inn"
                    :model-value="field.state.value"
                    inputmode="numeric"
                    autocomplete="off"
                    maxlength="14"
                    :disabled="saving || lookupState === 'searching'"
                    :aria-invalid="!!state.meta.errors?.length || !!lookupError"
                    class="min-w-0 flex-1"
                    placeholder="14-digit identity number"
                    @input="onInnInput(field.handleChange, $event)"
                    @blur="field.handleBlur"
                  />
                  <UiButton
                    v-if="!isEditMode"
                    type="button"
                    variant="default"
                    class="shrink-0"
                    :disabled="saving || lookupState === 'searching'"
                    @click="lookupIdentity"
                  >
                    <LoaderCircleIcon v-if="lookupState === 'searching'" class="size-4 animate-spin" />
                    <SearchIcon v-else class="size-4" />
                    {{ lookupState === 'searching' ? 'Searching' : 'Search' }}
                  </UiButton>
                </div>
                <FieldError :errors="lookupError ? [...state.meta.errors, lookupError] : state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="lastName">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel for="patient-last-name" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Last name
                </UiLabel>
                <UiInput
                  id="patient-last-name"
                  :model-value="field.state.value"
                  maxlength="100"
                  :disabled="detailsLocked || identityFieldsLocked"
                  :aria-invalid="!!state.meta.errors?.length"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="firstName">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel for="patient-first-name" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  First name
                </UiLabel>
                <UiInput
                  id="patient-first-name"
                  :model-value="field.state.value"
                  maxlength="100"
                  :disabled="detailsLocked || identityFieldsLocked"
                  :aria-invalid="!!state.meta.errors?.length"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="middleName">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel for="patient-middle-name" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Middle name
                </UiLabel>
                <UiInput
                  id="patient-middle-name"
                  :model-value="field.state.value"
                  maxlength="100"
                  :disabled="detailsLocked || identityFieldsLocked"
                  :aria-invalid="!!state.meta.errors?.length"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="phone">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel for="patient-phone" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Phone
                </UiLabel>
                <UiInput
                  id="patient-phone"
                  :model-value="field.state.value"
                  type="tel"
                  maxlength="50"
                  :disabled="detailsLocked"
                  :aria-invalid="!!state.meta.errors?.length"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="birthDate">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel for="patient-birth-date" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Birth date
                </UiLabel>
                <UiInput
                  id="patient-birth-date"
                  :model-value="field.state.value"
                  type="date"
                  :disabled="saving || !isEditMode"
                  :aria-invalid="!!state.meta.errors?.length"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="gender">
            <template #default="{ field, state }">
              <FormItem>
                <UiLabel :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Gender
                </UiLabel>
                <UiSelect
                  :model-value="field.state.value"
                  :disabled="saving || !isEditMode"
                  @update:model-value="value => field.handleChange(Number(value) as 1 | 2)"
                >
                  <UiSelectTrigger class="w-full" :aria-invalid="!!state.meta.errors?.length">
                    <UiSelectValue placeholder="Select gender" />
                  </UiSelectTrigger>
                  <UiSelectContent>
                    <UiSelectItem :value="1">
                      Male
                    </UiSelectItem>
                    <UiSelectItem :value="2">
                      Female
                    </UiSelectItem>
                  </UiSelectContent>
                </UiSelect>
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
                  :disabled="detailsLocked"
                  @update:model-value="value => onRegionChanged(field.handleChange, value)"
                >
                  <UiSelectTrigger class="w-full" :aria-invalid="!!state.meta.errors?.length">
                    <UiSelectValue placeholder="Select region" />
                  </UiSelectTrigger>
                  <UiSelectContent>
                    <UiSelectItem v-for="region in regions" :key="region.id" :value="region.id">
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
                  :disabled="detailsLocked || !selectedRegionId"
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

          <form.Field name="address">
            <template #default="{ field, state }">
              <FormItem class="md:col-span-2">
                <UiLabel for="patient-address" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Address
                </UiLabel>
                <UiTextarea
                  id="patient-address"
                  :model-value="field.state.value"
                  maxlength="500"
                  :disabled="detailsLocked"
                  :aria-invalid="!!state.meta.errors?.length"
                  class="min-h-20 resize-y"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <form.Field name="address2">
            <template #default="{ field, state }">
              <FormItem class="md:col-span-2">
                <UiLabel for="patient-address-2" :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                  Address 2
                </UiLabel>
                <UiTextarea
                  id="patient-address-2"
                  :model-value="field.state.value"
                  maxlength="500"
                  :disabled="detailsLocked"
                  :aria-invalid="!!state.meta.errors?.length"
                  class="min-h-20 resize-y"
                  @input="field.handleChange($event.target.value)"
                  @blur="field.handleBlur"
                />
                <FieldError :errors="state.meta.errors" />
              </FormItem>
            </template>
          </form.Field>

          <template v-if="isEditMode">
            <form.Field name="groupId">
              <template #default="{ field, state }">
                <FormItem>
                  <UiLabel :data-error="!!state.meta.errors?.length" class="data-[error=true]:text-destructive">
                    Group
                  </UiLabel>
                  <UiSelect
                    :model-value="field.state.value"
                    :disabled="saving"
                    @update:model-value="value => field.handleChange(Number(value))"
                  >
                    <UiSelectTrigger class="w-full" :aria-invalid="!!state.meta.errors?.length">
                      <UiSelectValue placeholder="Select group" />
                    </UiSelectTrigger>
                    <UiSelectContent>
                      <UiSelectItem v-for="group in groups" :key="group.id" :value="group.id">
                        {{ group.name }}
                      </UiSelectItem>
                    </UiSelectContent>
                  </UiSelect>
                  <FieldError :errors="state.meta.errors" />
                </FormItem>
              </template>
            </form.Field>

            <div class="grid gap-3 rounded-md border p-4">
              <form.Field name="specialStatus">
                <template #default="{ field }">
                  <div class="flex items-center justify-between gap-4">
                    <UiLabel for="patient-special-status">
                      Special status
                    </UiLabel>
                    <UiSwitch
                      id="patient-special-status"
                      :model-value="field.state.value"
                      :disabled="saving"
                      @update:model-value="value => field.handleChange(Boolean(value))"
                    />
                  </div>
                </template>
              </form.Field>
              <form.Field name="isActive">
                <template #default="{ field }">
                  <div class="flex items-center justify-between gap-4">
                    <UiLabel for="patient-active">
                      Active
                    </UiLabel>
                    <UiSwitch
                      id="patient-active"
                      :model-value="field.state.value"
                      :disabled="saving"
                      @update:model-value="value => field.handleChange(Boolean(value))"
                    />
                  </div>
                </template>
              </form.Field>
            </div>
          </template>
        </div>

        <UiDialogFooter class="border-t bg-background px-6 py-4">
          <UiButton type="button" variant="outline" :disabled="saving" @click="onOpenChange(false)">
            Cancel
          </UiButton>
          <UiButton type="submit" :disabled="saveDisabled">
            <LoaderCircleIcon v-if="saving" class="size-4 animate-spin" />
            <SaveIcon v-else class="size-4" />
            Save patient
          </UiButton>
        </UiDialogFooter>
      </form>
    </UiDialogScrollContent>
  </UiDialog>
</template>
