import type {
  ServerDataGridFilter,
  ServerDataGridFilterField,
  ServerDataGridQueryRequest,
  ServerDataGridQueryState,
} from './server-data-grid.types'

const operatorLabels: Record<string, string> = {
  contains: 'contains',
  notContains: 'does not contain',
  startsWith: 'starts with',
  endsWith: 'ends with',
  equals: 'is',
  notEquals: 'is not',
  greaterThan: 'after',
  greaterThanOrEqual: 'from',
  lessThan: 'before',
  lessThanOrEqual: 'through',
  between: 'between',
  isEmpty: 'is empty',
  isNotEmpty: 'is not empty',
}

export function createServerDataGridQueryRequest(
  state: ServerDataGridQueryState,
): ServerDataGridQueryRequest {
  return {
    page: Math.max(1, state.pageIndex + 1),
    pageSize: Math.min(100, Math.max(1, state.pageSize)),
    search: state.search.trim() || null,
    sorting: state.sorting.map(sort => ({
      field: sort.id,
      descending: sort.desc,
    })),
    filters: state.filters.map(filter => ({
      ...filter,
      value: filter.value?.trim() || null,
      valueTo: filter.valueTo?.trim() || null,
    })),
    groupBy: state.groupBy || null,
  }
}

export function formatServerDataGridFilter(
  filter: ServerDataGridFilter,
  fields: ServerDataGridFilterField[],
): string {
  const definition = fields.find(field => field.field === filter.field)
  const fieldLabel = definition?.label ?? filter.field
  const operatorLabel = operatorLabels[filter.operator] ?? filter.operator

  if (filter.operator === 'isEmpty' || filter.operator === 'isNotEmpty') {
    return `${fieldLabel} ${operatorLabel}`
  }

  return `${fieldLabel} ${operatorLabel} ${formatFilterValue(filter, definition)}`
}

function formatFilterValue(
  filter: ServerDataGridFilter,
  definition: ServerDataGridFilterField | undefined,
): string {
  if (filter.operator === 'between') {
    return `${filter.value ?? ''} - ${filter.valueTo ?? ''}`
  }

  return definition?.options?.find(option => option.value === filter.value)?.label
    ?? filter.value
    ?? ''
}
