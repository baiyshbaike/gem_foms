<script setup lang="ts">
import { FilterIcon, PlusIcon, RotateCcwIcon, Trash2Icon } from '@lucide/vue'

import type {
  ServerDataGridFilter,
  ServerDataGridFilterField,
  ServerDataGridFilterOperator,
  ServerDataGridOption,
} from './server-data-grid.types'

const props = withDefaults(defineProps<{
  defaultField?: string
  description?: string
  fields: ServerDataGridFilterField[]
  modelValue: ServerDataGridFilter[]
  title?: string
}>(), {
  defaultField: '',
  description: 'Combine filters to narrow the result set on the server.',
  title: 'Filters',
})

const emit = defineEmits<{
  'update:modelValue': [filters: ServerDataGridFilter[]]
}>()

const open = ref(false)
const draftFilters = ref<ServerDataGridFilter[]>([])
const activeFilterCount = computed(() => props.modelValue.length)

const textOperators: ServerDataGridOption[] = [
  { label: 'Contains', value: 'contains' },
  { label: 'Does not contain', value: 'notContains' },
  { label: 'Starts with', value: 'startsWith' },
  { label: 'Ends with', value: 'endsWith' },
  { label: 'Equals', value: 'equals' },
  { label: 'Does not equal', value: 'notEquals' },
]

const comparisonOperators: ServerDataGridOption[] = [
  { label: 'Equals', value: 'equals' },
  { label: 'Less than', value: 'lessThan' },
  { label: 'Less than or equal', value: 'lessThanOrEqual' },
  { label: 'Greater than', value: 'greaterThan' },
  { label: 'Greater than or equal', value: 'greaterThanOrEqual' },
  { label: 'Between', value: 'between' },
]

const dateOperators: ServerDataGridOption[] = [
  { label: 'On', value: 'equals' },
  { label: 'Before', value: 'lessThan' },
  { label: 'On or before', value: 'lessThanOrEqual' },
  { label: 'After', value: 'greaterThan' },
  { label: 'On or after', value: 'greaterThanOrEqual' },
  { label: 'Between', value: 'between' },
]

function openFilters() {
  draftFilters.value = props.modelValue.map(filter => ({ ...filter }))
  open.value = true
}

function fieldDefinition(field: string) {
  return props.fields.find(item => item.field === field) ?? props.fields[0]!
}

function operatorsFor(filter: ServerDataGridFilter): ServerDataGridOption[] {
  const definition = fieldDefinition(filter.field)
  if (definition.type === 'date') {
    return dateOperators
  }
  if (definition.type === 'number') {
    return comparisonOperators
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

function changeField(filter: ServerDataGridFilter, field: string) {
  filter.field = field
  filter.operator = defaultOperator(fieldDefinition(field).type)
  filter.value = null
  filter.valueTo = null
}

function onFieldChange(filter: ServerDataGridFilter, event: Event) {
  changeField(filter, (event.target as HTMLSelectElement).value)
}

function onFilterValueChange(filter: ServerDataGridFilter, event: Event) {
  filter.value = (event.target as HTMLSelectElement).value
}

function onInput(filter: ServerDataGridFilter, event: Event, target: 'value' | 'valueTo') {
  filter[target] = (event.target as HTMLInputElement).value
}

function defaultOperator(type: ServerDataGridFilterField['type']): ServerDataGridFilterOperator {
  return type === 'text' ? 'contains' : 'equals'
}

function createFilter(): ServerDataGridFilter {
  const definition = fieldDefinition(props.defaultField || props.fields[0]?.field || '')
  return {
    field: definition.field,
    operator: defaultOperator(definition.type),
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
        <UiSheetTitle>{{ title }}</UiSheetTitle>
        <UiSheetDescription>{{ description }}</UiSheetDescription>
      </UiSheetHeader>

      <div class="min-h-0 flex-1 space-y-3 overflow-y-auto px-6 py-5">
        <div v-if="draftFilters.length === 0" class="rounded-md border border-dashed px-4 py-10 text-center">
          <FilterIcon class="mx-auto mb-3 size-6 text-muted-foreground" />
          <p class="text-sm font-medium">
            No filters applied
          </p>
          <p class="mt-1 text-sm text-muted-foreground">
            Add one or more conditions to refine the list.
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
              <UiNativeSelectOption v-for="field in fields" :key="field.field" :value="field.field">
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

            <UiButton type="button" size="icon" variant="ghost" title="Remove filter" @click="removeFilter(index)">
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
              @input="onInput(filter, $event, 'value')"
            />
            <UiInput
              v-if="filter.operator === 'between'"
              :model-value="filter.valueTo ?? ''"
              :type="fieldDefinition(filter.field).type === 'number' ? 'number' : 'date'"
              aria-label="Filter end value"
              @input="onInput(filter, $event, 'valueTo')"
            />
          </div>
        </div>

        <UiButton type="button" variant="outline" class="w-full" :disabled="fields.length === 0" @click="addFilter">
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
