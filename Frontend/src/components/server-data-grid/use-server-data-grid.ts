import type {
  ColumnDef,
  ColumnPinningState,
  ExpandedState,
  GroupingState,
  RowSelectionState,
  SortingState,
  VisibilityState,
} from '@tanstack/vue-table'
import type { ComputedRef } from 'vue'

import {
  getCoreRowModel,
  getExpandedRowModel,
  getGroupedRowModel,
  useVueTable,
} from '@tanstack/vue-table'
import { watchDebounced } from '@vueuse/core'

import { valueUpdater } from '@/lib/utils'

import type {
  ServerDataGridFilter,
  ServerDataGridPersistedState,
  ServerDataGridQueryRequest,
  ServerDataGridQueryResult,
} from './server-data-grid.types'

import { createServerDataGridQueryRequest } from './server-data-grid.utils'

interface UseServerDataGridOptions<TData> {
  columns: ComputedRef<ColumnDef<TData>[]>
  defaultColumnPinning: ColumnPinningState
  defaultColumnVisibility: VisibilityState
  defaultPageSize: number
  defaultSorting: SortingState
  enableSelection: boolean
  formatError: (error: unknown, fallback: string) => string
  getRowId: (row: TData) => string
  load: (request: ServerDataGridQueryRequest) => Promise<ServerDataGridQueryResult<TData>>
  loadErrorMessage: string
  pageSizes: number[]
  stateVersion: number
  storageKey: string
}

export function useServerDataGrid<TData>(options: UseServerDataGridOptions<TData>) {
  const persistedState = readPersistedState(options.storageKey, options.stateVersion)
  const rows = shallowRef<TData[]>([])
  const totalCount = ref(0)
  const groupSummaries = ref<ServerDataGridQueryResult<TData>['groups']>([])
  const loading = ref(false)
  const errorMessage = ref('')
  const refreshVersion = ref(0)
  const requestVersion = ref(0)
  const sorting = ref<SortingState>(persistedState?.sorting ?? [...options.defaultSorting])
  const columnVisibility = ref<VisibilityState>({
    ...options.defaultColumnVisibility,
    ...persistedState?.columnVisibility,
  })
  const columnOrder = ref(persistedState?.columnOrder ?? [])
  const columnPinning = ref<ColumnPinningState>(persistedState?.columnPinning ?? clonePinning(options.defaultColumnPinning))
  const columnSizing = ref(persistedState?.columnSizing ?? {})
  const grouping = ref<GroupingState>((persistedState?.grouping ?? []).slice(0, 1))
  const expanded = ref<ExpandedState>(true)
  const rowSelection = ref<RowSelectionState>({})
  const pagination = ref({
    pageIndex: 0,
    pageSize: persistedState?.pageSize && options.pageSizes.includes(persistedState.pageSize)
      ? persistedState.pageSize
      : options.defaultPageSize,
  })
  const filters = ref<ServerDataGridFilter[]>(persistedState?.filters ?? [])
  const searchInput = ref(persistedState?.search ?? '')
  const search = ref(persistedState?.search ?? '')

  const pageCount = computed(() => Math.max(1, Math.ceil(totalCount.value / pagination.value.pageSize)))
  const selectedIds = computed(() => Object.entries(rowSelection.value)
    .filter(([, selected]) => selected)
    .map(([id]) => id))
  const groupBy = computed(() => grouping.value[0] ?? null)
  const query = computed(() => createServerDataGridQueryRequest({
    pageIndex: pagination.value.pageIndex,
    pageSize: pagination.value.pageSize,
    search: search.value,
    sorting: sorting.value,
    filters: filters.value,
    groupBy: groupBy.value,
  }))

  const table = useVueTable<TData>({
    get data() { return rows.value },
    get columns() { return options.columns.value },
    get pageCount() { return pageCount.value },
    state: {
      get sorting() { return sorting.value },
      get columnVisibility() { return columnVisibility.value },
      get columnOrder() { return columnOrder.value },
      get columnPinning() { return columnPinning.value },
      get columnSizing() { return columnSizing.value },
      get rowSelection() { return rowSelection.value },
      get pagination() { return pagination.value },
      get grouping() { return grouping.value },
      get expanded() { return expanded.value },
    },
    enableColumnPinning: true,
    enableColumnResizing: true,
    enableMultiSort: true,
    enableRowSelection: options.enableSelection ? row => !row.getIsGrouped() : false,
    columnResizeMode: 'onChange',
    groupedColumnMode: false,
    manualFiltering: true,
    manualPagination: true,
    manualSorting: true,
    autoResetPageIndex: false,
    getRowId: row => options.getRowId(row),
    getCoreRowModel: getCoreRowModel(),
    getGroupedRowModel: getGroupedRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    onSortingChange: (updater) => {
      valueUpdater(updater, sorting)
      pagination.value.pageIndex = 0
    },
    onColumnVisibilityChange: updater => valueUpdater(updater, columnVisibility),
    onColumnOrderChange: updater => valueUpdater(updater, columnOrder),
    onColumnPinningChange: updater => valueUpdater(updater, columnPinning),
    onColumnSizingChange: updater => valueUpdater(updater, columnSizing),
    onRowSelectionChange: updater => valueUpdater(updater, rowSelection),
    onPaginationChange: updater => valueUpdater(updater, pagination),
    onGroupingChange: (updater) => {
      valueUpdater(updater, grouping)
      grouping.value = grouping.value.slice(-1)
      expanded.value = true
      pagination.value.pageIndex = 0
    },
    onExpandedChange: updater => valueUpdater(updater, expanded),
  })

  watchDebounced(searchInput, (value) => {
    search.value = value
    clearSelection()
    pagination.value.pageIndex = 0
  }, { debounce: 350, maxWait: 900 })

  watch([query, refreshVersion], loadRows, { deep: true, immediate: true })
  watch(
    [sorting, columnVisibility, columnOrder, columnPinning, columnSizing, grouping, filters, search, () => pagination.value.pageSize],
    persistState,
    { deep: true },
  )

  async function loadRows() {
    const currentRequest = ++requestVersion.value
    loading.value = true
    errorMessage.value = ''

    try {
      const result = await options.load(query.value)
      if (currentRequest !== requestVersion.value) {
        return
      }

      rows.value = result.items
      totalCount.value = result.totalCount
      groupSummaries.value = result.groups

      const lastPageIndex = Math.max(0, Math.ceil(result.totalCount / pagination.value.pageSize) - 1)
      if (pagination.value.pageIndex > lastPageIndex) {
        pagination.value.pageIndex = lastPageIndex
      }
    }
    catch (error) {
      if (currentRequest === requestVersion.value) {
        errorMessage.value = options.formatError(error, options.loadErrorMessage)
      }
    }
    finally {
      if (currentRequest === requestVersion.value) {
        loading.value = false
      }
    }
  }

  function refresh() {
    refreshVersion.value += 1
  }

  function clearSelection() {
    rowSelection.value = {}
  }

  function removeSelection(id: string | number) {
    const nextSelection = { ...rowSelection.value }
    delete nextSelection[String(id)]
    rowSelection.value = nextSelection
  }

  function setFilters(nextFilters: ServerDataGridFilter[]) {
    filters.value = nextFilters
    clearSelection()
    pagination.value.pageIndex = 0
  }

  function removeFilter(index: number) {
    setFilters(filters.value.filter((_, filterIndex) => filterIndex !== index))
  }

  function setGrouping(value: unknown) {
    grouping.value = value && value !== 'none' ? [String(value)] : []
    expanded.value = true
    pagination.value.pageIndex = 0
  }

  function reset() {
    sorting.value = [...options.defaultSorting]
    columnVisibility.value = { ...options.defaultColumnVisibility }
    columnOrder.value = []
    columnPinning.value = clonePinning(options.defaultColumnPinning)
    columnSizing.value = {}
    grouping.value = []
    expanded.value = true
    filters.value = []
    clearSelection()
    search.value = ''
    searchInput.value = ''
    pagination.value = { pageIndex: 0, pageSize: options.defaultPageSize }
    getStorage()?.removeItem(options.storageKey)
  }

  function persistState() {
    const state: ServerDataGridPersistedState = {
      version: options.stateVersion,
      sorting: sorting.value,
      columnVisibility: columnVisibility.value,
      columnOrder: columnOrder.value,
      columnPinning: columnPinning.value,
      columnSizing: columnSizing.value,
      grouping: grouping.value,
      filters: filters.value,
      search: search.value,
      pageSize: pagination.value.pageSize,
    }
    getStorage()?.setItem(options.storageKey, JSON.stringify(state))
  }

  return {
    clearSelection,
    columnOrder,
    columnPinning,
    columnSizing,
    columnVisibility,
    errorMessage,
    expanded,
    filters,
    groupBy,
    grouping,
    groupSummaries,
    loading,
    pageCount,
    pagination,
    query,
    refresh,
    removeFilter,
    removeSelection,
    reset,
    rowSelection,
    rows,
    searchInput,
    selectedIds,
    setFilters,
    setGrouping,
    sorting,
    table,
    totalCount,
  }
}

function readPersistedState(storageKey: string, version: number): ServerDataGridPersistedState | null {
  const storage = getStorage()
  const value = storage?.getItem(storageKey)
  if (!value) {
    return null
  }

  try {
    const state = JSON.parse(value) as Partial<ServerDataGridPersistedState>
    if (state.version !== version) {
      storage?.removeItem(storageKey)
      return null
    }
    return state as ServerDataGridPersistedState
  }
  catch {
    storage?.removeItem(storageKey)
    return null
  }
}

function clonePinning(value: ColumnPinningState): ColumnPinningState {
  return {
    left: value.left ? [...value.left] : [],
    right: value.right ? [...value.right] : [],
  }
}

function getStorage(): Storage | null {
  return typeof window === 'undefined' ? null : window.localStorage
}
