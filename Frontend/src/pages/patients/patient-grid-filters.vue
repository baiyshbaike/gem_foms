<script setup lang="ts">
import { FilterIcon, PlusIcon, RotateCcwIcon, Trash2Icon } from '@lucide/vue'

import type {
  PatientGridFilter,
  PatientGridFilterOperator,
  PatientGroup,
  Region,
} from '@/services/types/dialysis'

type FilterValueType = 'text' | 'date' | 'select'

interface FilterOption {
  label: string
  value: string
}

interface FilterFieldDefinition {
  field: string
  label: string
  type: FilterValueType
  options?: FilterOption[]
}

const props = defineProps<{
  modelValue: PatientGridFilter[]
  groups: PatientGroup[]
  regions: Region[]
}>()

const emit = defineEmits<{
  'update:modelValue': [filters: PatientGridFilter[]]
}>()

const open = ref(false)
const draftFilters = ref<PatientGridFilter[]>([])

const activeFilterCount = computed(() => props.modelValue.length)
const districts = computed(() => props.regions.flatMap(region => region.districts
  .filter(district => district.isActive)
  .map(district => ({
    label: `${district.name} (${region.name})`,
    value: String(district.id),
  }))))
const fieldDefinitions = computed<FilterFieldDefinition[]>(() => [
  { field: 'inn', label: 'INN', type: 'text' },
  { field: 'fullName', label: 'Full name', type: 'text' },
  { field: 'phone', label: 'Phone', type: 'text' },
  { field: 'birthDate', label: 'Birth date', type: 'date' },
  { field: 'createdAt', label: 'Registered date', type: 'date' },
  {
    field: 'regionId',
    label: 'Region',
    type: 'select',
    options: props.regions.map(region => ({ label: region.name, value: String(region.id) })),
  },
  {
    field: 'districtId',
    label: 'District',
    type: 'select',
    options: districts.value,
  },
  {
    field: 'groupId',
    label: 'Group',
    type: 'select',
    options: props.groups.map(group => ({ label: group.name, value: String(group.id) })),
  },
  {
    field: 'gender',
    label: 'Gender',
    type: 'select',
    options: [
      { label: 'Male', value: '1' },
      { label: 'Female', value: '2' },
    ],
  },
  {
    field: 'specialStatus',
    label: 'Special status',
    type: 'select',
    options: [
      { label: 'Special', value: 'true' },
      { label: 'Standard', value: 'false' },
    ],
  },
  {
    field: 'isActive',
    label: 'Active status',
    type: 'select',
    options: [
      { label: 'Active', value: 'true' },
      { label: 'Inactive', value: 'false' },
    ],
  },
])

const textOperators: FilterOption[] = [
  { label: 'Contains', value: 'contains' },
  { label: 'Does not contain', value: 'notContains' },
  { label: 'Starts with', value: 'startsWith' },
  { label: 'Ends with', value: 'endsWith' },
  { label: 'Equals', value: 'equals' },
  { label: 'Does not equal', value: 'notEquals' },
]

const dateOperators: FilterOption[] = [
  { label: 'On', value: 'equals' },
  { label: 'Before', value: 'lessThan' },
  { label: 'On or before', value: 'lessThanOrEqual' },
  { label: 'After', value: 'greaterThan' },
  { label: 'On or after', value: 'greaterThanOrEqual' },
  { label: 'Between', value: 'between' },
]

function openFilters() {
  draftFilters.value = props.modelValue.map(filter => ({
    ...filter,
  }))
  open.value = true
}

function fieldDefinition(field: string) {
  return fieldDefinitions.value.find(item => item.field === field) ?? fieldDefinitions.value[0]!
}

function operatorsFor(filter: PatientGridFilter): FilterOption[] {
  const definition = fieldDefinition(filter.field)
  if (definition.type === 'date') {
    return dateOperators
  }
  if (definition.type === 'select') {
    return textOperators.slice(4)
  }
  return textOperators
}

function addFilter() {
  draftFilters.value.push(createFilter())
}

function removeFilter(index: number) {
  draftFilters.value.splice(index, 1)
}

function changeField(filter: PatientGridFilter, field: string) {
  filter.field = field
  filter.operator = defaultOperator(fieldDefinition(field).type)
  filter.value = null
  filter.valueTo = null
}

function onFieldChange(filter: PatientGridFilter, event: Event) {
  changeField(filter, (event.target as HTMLSelectElement).value)
}

function onFilterValueChange(filter: PatientGridFilter, event: Event) {
  filter.value = (event.target as HTMLSelectElement).value
}

function defaultOperator(type: FilterValueType): PatientGridFilterOperator {
  return type === 'text' ? 'contains' : 'equals'
}

function createFilter(): PatientGridFilter {
  return {
    field: 'fullName',
    operator: 'contains',
    value: null,
    valueTo: null,
  }
}

function resetFilters() {
  draftFilters.value = []
}

function applyFilters() {
  const filters = draftFilters.value
    .filter(filter => filter.operator === 'isEmpty'
      || filter.operator === 'isNotEmpty'
      || (Boolean(filter.value?.trim())
        && (filter.operator !== 'between' || Boolean(filter.valueTo?.trim()))))
    .map(filter => ({
      ...filter,
      value: filter.value?.trim() || null,
      valueTo: filter.valueTo?.trim() || null,
    }))

  emit('update:modelValue', filters)
  open.value = false
}
</script>

<template>
  <UiButton variant="outline" size="sm" class="h-9 gap-2" @click="openFilters">
    <FilterIcon class="size-4" />
    Filters
    <UiBadge v-if="activeFilterCount" variant="secondary" class="h-5 min-w-5 justify-center px-1.5">
      {{ activeFilterCount }}
    </UiBadge>
  </UiButton>

  <UiSheet v-model:open="open">
    <UiSheetContent class="w-full gap-0 sm:max-w-xl">
      <UiSheetHeader class="border-b px-6 py-5">
        <UiSheetTitle>Patient filters</UiSheetTitle>
        <UiSheetDescription>
          Combine filters to narrow the registry on the server.
        </UiSheetDescription>
      </UiSheetHeader>

      <div class="min-h-0 flex-1 space-y-3 overflow-y-auto px-6 py-5">
        <div v-if="draftFilters.length === 0" class="rounded-md border border-dashed px-4 py-10 text-center">
          <FilterIcon class="mx-auto mb-3 size-6 text-muted-foreground" />
          <p class="text-sm font-medium">
            No filters applied
          </p>
          <p class="mt-1 text-sm text-muted-foreground">
            Add one or more conditions to refine the patient list.
          </p>
        </div>

        <div
          v-for="(filter, index) in draftFilters"
          :key="index"
          class="grid gap-3 rounded-md border p-3"
        >
          <div class="grid gap-2 sm:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_auto]">
            <UiNativeSelect
              :model-value="filter.field"
              class="w-full"
              aria-label="Filter field"
              @change="onFieldChange(filter, $event)"
            >
              <UiNativeSelectOption v-for="field in fieldDefinitions" :key="field.field" :value="field.field">
                {{ field.label }}
              </UiNativeSelectOption>
            </UiNativeSelect>

            <UiNativeSelect
              v-model="filter.operator"
              class="w-full"
              aria-label="Filter operator"
            >
              <UiNativeSelectOption v-for="operator in operatorsFor(filter)" :key="operator.value" :value="operator.value">
                {{ operator.label }}
              </UiNativeSelectOption>
            </UiNativeSelect>

            <UiButton
              type="button"
              size="icon"
              variant="ghost"
              title="Remove filter"
              @click="removeFilter(index)"
            >
              <Trash2Icon class="size-4" />
              <span class="sr-only">Remove filter</span>
            </UiButton>
          </div>

          <div class="grid gap-2" :class="filter.operator === 'between' ? 'sm:grid-cols-2' : ''">
            <UiNativeSelect
              v-if="fieldDefinition(filter.field).type === 'select'"
              :model-value="filter.value ?? ''"
              class="w-full"
              aria-label="Filter value"
              @change="onFilterValueChange(filter, $event)"
            >
              <UiNativeSelectOption value="" disabled>
                Select value
              </UiNativeSelectOption>
              <UiNativeSelectOption
                v-for="option in fieldDefinition(filter.field).options"
                :key="option.value"
                :value="option.value"
              >
                {{ option.label }}
              </UiNativeSelectOption>
            </UiNativeSelect>
            <UiInput
              v-else
              :model-value="filter.value ?? ''"
              :type="fieldDefinition(filter.field).type === 'date' ? 'date' : fieldDefinition(filter.field).type"
              placeholder="Filter value"
              @input="filter.value = $event.target.value"
            />
            <UiInput
              v-if="filter.operator === 'between'"
              :model-value="filter.valueTo ?? ''"
              type="date"
              aria-label="Filter end date"
              @input="filter.valueTo = $event.target.value"
            />
          </div>
        </div>

        <UiButton type="button" variant="outline" class="w-full" @click="addFilter">
          <PlusIcon class="size-4" />
          Add condition
        </UiButton>
      </div>

      <UiSheetFooter class="border-t px-6 py-4 sm:justify-between">
        <UiButton type="button" variant="ghost" @click="resetFilters">
          <RotateCcwIcon class="size-4" />
          Clear all
        </UiButton>
        <UiButton type="button" @click="applyFilters">
          Apply filters
        </UiButton>
      </UiSheetFooter>
    </UiSheetContent>
  </UiSheet>
</template>
