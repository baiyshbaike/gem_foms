<script setup lang="ts">
import type {
  Column,
  ColumnDef,
  ColumnOrderState,
  ColumnPinningState,
  ColumnSizingState,
  ExpandedState,
  GroupingState,
  PaginationState,
  Row,
  RowSelectionState,
  SortingState,
  VisibilityState,
} from '@tanstack/vue-table'
import type { CSSProperties } from 'vue'

import {
  ArrowDownToLineIcon,
  ChevronDownIcon,
  ChevronFirstIcon,
  ChevronLastIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ChevronsUpDownIcon,
  Columns3Icon,
  FileSpreadsheetIcon,
  PlusIcon,
  RefreshCwIcon,
  RotateCcwIcon,
  SearchIcon,
  Trash2Icon,
  XIcon,
} from '@lucide/vue'
import {
  FlexRender,
  getCoreRowModel,
  getExpandedRowModel,
  getGroupedRowModel,
  useVueTable,
} from '@tanstack/vue-table'
import { watchDebounced } from '@vueuse/core'
import axios from 'axios'
import { h } from 'vue'
import { toast } from 'vue-sonner'

import type {
  PatientGridExportRequest,
  PatientGridFilter,
  PatientGridGroupSummary,
  PatientGridQueryRequest,
  PatientGridRow,
  PatientGroup,
  Region,
} from '@/services/types/dialysis'

import { DataTableColumnHeader, SelectColumn } from '@/components/data-table'
import { BasicPage } from '@/components/global-layout'
import { Badge } from '@/components/ui/badge'
import { formatDate, formatDateTime } from '@/lib/dialysis'
import { valueUpdater } from '@/lib/utils'
import { patientApi, regionApi } from '@/services/api/dialysis.api'
import { useAuthStore } from '@/stores/auth'

import PatientEditorPopup from './patient-editor-popup.vue'
import { createPatientGridQueryRequest } from './patient-grid'
import PatientGridFilters from './patient-grid-filters.vue'
import PatientRowActions from './patient-row-actions.vue'

interface PatientEditorComponent {
  openCreate: () => void
  openEdit: (patient: PatientGridRow) => void
}

interface ApiProblemDetails {
  detail?: string
  errors?: Record<string, string[]>
  title?: string
}

interface PersistedPatientGridState {
  version: number
  columnOrder?: ColumnOrderState
  columnPinning?: ColumnPinningState
  columnSizing?: ColumnSizingState
  columnVisibility?: VisibilityState
  filters?: PatientGridFilter[]
  grouping?: GroupingState
  pageSize?: number
  search?: string
  sorting?: SortingState
}

interface ExportColumn {
  id: string
  label: string
  width: number
  value: (patient: PatientGridRow) => string | number | boolean | null
}

const patientGridStateVersion = 2
const pageSizes = [10, 25, 50, 100]
const authStore = useAuthStore()
const patientEditorRef = ref<PatientEditorComponent | null>(null)
const patients = ref<PatientGridRow[]>([])
const totalCount = ref(0)
const groupSummaries = ref<PatientGridGroupSummary[]>([])
const regions = ref<Region[]>([])
const groups = ref<PatientGroup[]>([])
const loading = ref(false)
const loadingLookups = ref(false)
const exporting = ref(false)
const deleting = ref(false)
const errorMessage = ref('')
const refreshVersion = ref(0)
const requestVersion = ref(0)
const deleteTarget = ref<PatientGridRow | null>(null)
const draggedColumnId = ref<string | null>(null)

const canCreate = computed(() => authStore.hasPermission('patient.create'))
const canUpdate = computed(() => authStore.hasPermission('patient.update'))
const canDelete = computed(() => authStore.hasPermission('patient.delete'))
const canExport = computed(() => authStore.hasPermission('patient.export'))
const canManageRows = computed(() => canUpdate.value || canDelete.value)
const lookupsReady = computed(() => regions.value.length > 0 && groups.value.length > 0)
const gridStorageKey = computed(() => `dialysis:patient-grid:${authStore.user?.id ?? 'anonymous'}`)
const persistedState = readPersistedGridState()

const defaultVisibility: VisibilityState = {
  address: false,
  address2: false,
  firstName: false,
  id: false,
  lastName: false,
  middleName: false,
  updatedAt: false,
}
const sorting = ref<SortingState>(persistedState?.sorting ?? [{ id: 'createdAt', desc: true }])
const columnVisibility = ref<VisibilityState>({ ...defaultVisibility, ...persistedState?.columnVisibility })
const columnOrder = ref<ColumnOrderState>(persistedState?.columnOrder ?? [])
const columnPinning = ref<ColumnPinningState>(persistedState?.columnPinning ?? {
  left: ['select', 'inn'],
  right: ['actions'],
})
const columnSizing = ref<ColumnSizingState>(persistedState?.columnSizing ?? {})
const grouping = ref<GroupingState>((persistedState?.grouping ?? []).slice(0, 1))
const expanded = ref<ExpandedState>(true)
const rowSelection = ref<RowSelectionState>({})
const pagination = ref<PaginationState>({
  pageIndex: 0,
  pageSize: persistedState?.pageSize && pageSizes.includes(persistedState.pageSize)
    ? persistedState.pageSize
    : 25,
})
const filters = ref<PatientGridFilter[]>(persistedState?.filters ?? [])
const searchInput = ref(persistedState?.search ?? '')
const search = ref(persistedState?.search ?? '')

const pageCount = computed(() => Math.max(1, Math.ceil(totalCount.value / pagination.value.pageSize)))
const selectedIds = computed(() => Object.entries(rowSelection.value)
  .filter(([, selected]) => selected)
  .map(([id]) => Number(id))
  .filter(Number.isFinite))
const selectedCount = computed(() => selectedIds.value.length)
const groupBy = computed(() => grouping.value[0] ?? null)
const gridRequest = computed<PatientGridQueryRequest>(() => createPatientGridQueryRequest({
  pageIndex: pagination.value.pageIndex,
  pageSize: pagination.value.pageSize,
  search: search.value,
  sorting: sorting.value,
  filters: filters.value,
  groupBy: groupBy.value,
}))

const columnLabels: Record<string, string> = {
  id: 'ID',
  inn: 'INN',
  firstName: 'First name',
  lastName: 'Last name',
  middleName: 'Middle name',
  fullName: 'Full name',
  gender: 'Gender',
  birthDate: 'Birth date',
  phone: 'Phone',
  regionId: 'Region',
  regionName: 'Region',
  districtId: 'District',
  districtName: 'District',
  groupId: 'Group',
  groupName: 'Group',
  specialStatus: 'Special',
  isActive: 'Active',
  createdAt: 'Registered',
  updatedAt: 'Updated',
  address: 'Address',
  address2: 'Address 2',
  actions: 'Actions',
}

const columns = computed<ColumnDef<PatientGridRow>[]>(() => {
  const definitions: ColumnDef<PatientGridRow>[] = [
    SelectColumn as ColumnDef<PatientGridRow>,
    {
      accessorKey: 'id',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'ID', multiSort: true }),
      size: 82,
      minSize: 72,
      maxSize: 120,
    },
    {
      accessorKey: 'inn',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'INN', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'font-mono text-xs tabular-nums' }, row.original.inn),
      size: 150,
      minSize: 140,
    },
    {
      accessorKey: 'fullName',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Full name', multiSort: true }),
      cell: ({ row }) => h('div', { class: 'grid min-w-0 py-0.5' }, [
        h('span', { class: 'truncate font-medium text-foreground' }, row.original.fullName),
        h('span', { class: 'truncate text-xs text-muted-foreground' }, row.original.address),
      ]),
      size: 260,
      minSize: 210,
      maxSize: 420,
    },
    {
      accessorKey: 'lastName',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Last name', multiSort: true }),
      size: 150,
    },
    {
      accessorKey: 'firstName',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'First name', multiSort: true }),
      size: 150,
    },
    {
      accessorKey: 'middleName',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Middle name', multiSort: true }),
      size: 150,
    },
    {
      accessorKey: 'birthDate',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Birth date', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'tabular-nums' }, formatDate(row.original.birthDate)),
      size: 128,
      minSize: 118,
    },
    {
      accessorKey: 'gender',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Gender', multiSort: true }),
      cell: ({ row }) => h(Badge, { variant: 'outline' }, () => row.original.gender === 1 ? 'Male' : 'Female'),
      size: 105,
      minSize: 96,
    },
    {
      accessorKey: 'phone',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Phone', multiSort: true }),
      size: 155,
      minSize: 135,
    },
    {
      accessorKey: 'regionName',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Region', multiSort: true }),
      size: 170,
      minSize: 130,
    },
    {
      accessorKey: 'districtName',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'District', multiSort: true }),
      size: 170,
      minSize: 130,
    },
    {
      accessorKey: 'groupName',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Group', multiSort: true }),
      cell: ({ row }) => h(Badge, { variant: 'secondary' }, () => row.original.groupName),
      size: 125,
      minSize: 110,
    },
    {
      accessorKey: 'specialStatus',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Special', multiSort: true }),
      cell: ({ row }) => row.original.specialStatus
        ? h(Badge, { variant: 'default' }, () => 'Special')
        : h('span', { class: 'text-muted-foreground' }, 'Standard'),
      size: 105,
      minSize: 96,
    },
    {
      accessorKey: 'isActive',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Active', multiSort: true }),
      cell: ({ row }) => h(Badge, {
        variant: row.original.isActive ? 'outline' : 'secondary',
        class: row.original.isActive ? 'border-emerald-500/40 text-emerald-700 dark:text-emerald-400' : '',
      }, () => row.original.isActive ? 'Active' : 'Inactive'),
      size: 105,
      minSize: 96,
    },
    {
      accessorKey: 'createdAt',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Registered', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'tabular-nums' }, formatDateTime(row.original.createdAt)),
      size: 175,
      minSize: 155,
    },
    {
      accessorKey: 'updatedAt',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Updated', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'tabular-nums' }, formatDateTime(row.original.updatedAt)),
      size: 175,
      minSize: 155,
    },
    {
      accessorKey: 'address',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Address', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'block truncate' }, row.original.address),
      size: 280,
      minSize: 180,
    },
    {
      accessorKey: 'address2',
      header: ({ column }) => h(DataTableColumnHeader<PatientGridRow>, { column, title: 'Address 2', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'block truncate' }, row.original.address2),
      size: 280,
      minSize: 180,
    },
  ]

  if (canManageRows.value) {
    definitions.push({
      id: 'actions',
      header: () => h('span', { class: 'block text-right' }, 'Actions'),
      cell: ({ row }) => h(PatientRowActions, {
        patient: row.original,
        canUpdate: canUpdate.value,
        canDelete: canDelete.value,
        disabled: !lookupsReady.value || loadingLookups.value,
        onEdit: openEditPatient,
        onDelete: requestDeletePatient,
      }),
      enableHiding: false,
      enablePinning: true,
      enableResizing: false,
      enableSorting: false,
      size: 88,
      minSize: 88,
      maxSize: 88,
    })
  }

  return definitions
})

const table = useVueTable<PatientGridRow>({
  get data() { return patients.value },
  get columns() { return columns.value },
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
  enableRowSelection: row => !row.getIsGrouped(),
  columnResizeMode: 'onChange',
  groupedColumnMode: false,
  manualFiltering: true,
  manualPagination: true,
  manualSorting: true,
  autoResetPageIndex: false,
  getRowId: row => String(row.id),
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
  rowSelection.value = {}
  pagination.value.pageIndex = 0
}, { debounce: 350, maxWait: 900 })

watch([gridRequest, refreshVersion], loadPatients, { deep: true, immediate: true })

watch(
  [sorting, columnVisibility, columnOrder, columnPinning, columnSizing, grouping, filters, search, () => pagination.value.pageSize],
  persistGridState,
  { deep: true },
)

onMounted(loadLookups)

async function loadPatients() {
  const currentRequest = ++requestVersion.value
  loading.value = true
  errorMessage.value = ''

  try {
    const result = await patientApi.gridQuery(gridRequest.value)
    if (currentRequest !== requestVersion.value) {
      return
    }

    patients.value = result.items
    totalCount.value = result.totalCount
    groupSummaries.value = result.groups

    const lastPageIndex = Math.max(0, Math.ceil(result.totalCount / pagination.value.pageSize) - 1)
    if (pagination.value.pageIndex > lastPageIndex) {
      pagination.value.pageIndex = lastPageIndex
    }
  }
  catch (error) {
    if (currentRequest === requestVersion.value) {
      errorMessage.value = toPatientError(error, 'Patients could not be loaded').message
    }
  }
  finally {
    if (currentRequest === requestVersion.value) {
      loading.value = false
    }
  }
}

async function loadLookups() {
  loadingLookups.value = true
  try {
    const [regionResult, groupResult] = await Promise.all([
      regionApi.list(),
      patientApi.groups(),
    ])
    regions.value = regionResult.filter(region => region.isActive)
    groups.value = groupResult
  }
  catch (error) {
    toast.error(toPatientError(error, 'Patient lookups could not be loaded').message)
  }
  finally {
    loadingLookups.value = false
  }
}

function refreshGrid() {
  refreshVersion.value += 1
}

function openCreatePatient() {
  patientEditorRef.value?.openCreate()
}

function openEditPatient(patient: PatientGridRow) {
  patientEditorRef.value?.openEdit(patient)
}

function requestDeletePatient(patient: PatientGridRow) {
  deleteTarget.value = patient
}

async function deletePatient() {
  if (!deleteTarget.value) {
    return
  }

  const patient = deleteTarget.value
  deleting.value = true
  try {
    await patientApi.delete(patient.id)
    const nextSelection = { ...rowSelection.value }
    delete nextSelection[String(patient.id)]
    rowSelection.value = nextSelection
    deleteTarget.value = null
    toast.success('Patient deleted')
    refreshGrid()
  }
  catch (error) {
    toast.error(toPatientError(error, 'Patient could not be deleted').message)
  }
  finally {
    deleting.value = false
  }
}

function onPatientSaved(action: 'created' | 'updated') {
  toast.success(action === 'created' ? 'Patient created' : 'Patient updated')
  refreshGrid()
}

function updateFilters(nextFilters: PatientGridFilter[]) {
  filters.value = nextFilters
  rowSelection.value = {}
  pagination.value.pageIndex = 0
}

function removeFilter(index: number) {
  filters.value = filters.value.filter((_, filterIndex) => filterIndex !== index)
  rowSelection.value = {}
  pagination.value.pageIndex = 0
}

function setGrouping(value: unknown) {
  grouping.value = value && value !== 'none' ? [String(value)] : []
  expanded.value = true
  pagination.value.pageIndex = 0
}

function onPageSizeChange(event: Event) {
  table.setPageSize(Number((event.target as HTMLSelectElement).value))
}

function resetGridLayout() {
  sorting.value = [{ id: 'createdAt', desc: true }]
  columnVisibility.value = { ...defaultVisibility }
  columnOrder.value = []
  columnPinning.value = { left: ['select', 'inn'], right: ['actions'] }
  columnSizing.value = {}
  grouping.value = []
  expanded.value = true
  filters.value = []
  rowSelection.value = {}
  search.value = ''
  searchInput.value = ''
  pagination.value = { pageIndex: 0, pageSize: 25 }
  localStorage.removeItem(gridStorageKey.value)
  toast.success('Grid layout reset')
}

function readPersistedGridState(): PersistedPatientGridState | null {
  const value = localStorage.getItem(gridStorageKey.value)
  if (!value) {
    return null
  }

  try {
    const state = JSON.parse(value) as Partial<PersistedPatientGridState>
    if (state.version !== patientGridStateVersion) {
      localStorage.removeItem(gridStorageKey.value)
      return null
    }

    return state as PersistedPatientGridState
  }
  catch {
    localStorage.removeItem(gridStorageKey.value)
    return null
  }
}

function persistGridState() {
  const state: PersistedPatientGridState = {
    version: patientGridStateVersion,
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
  localStorage.setItem(gridStorageKey.value, JSON.stringify(state))
}

function startColumnDrag(columnId: string) {
  if (columnId !== 'select' && columnId !== 'actions') {
    draggedColumnId.value = columnId
  }
}

function dropColumn(targetColumnId: string) {
  const sourceColumnId = draggedColumnId.value
  draggedColumnId.value = null
  if (!sourceColumnId || sourceColumnId === targetColumnId || targetColumnId === 'select' || targetColumnId === 'actions') {
    return
  }

  const allIds = table.getAllLeafColumns().map(column => column.id)
  const currentOrder = columnOrder.value.length
    ? [...columnOrder.value, ...allIds.filter(id => !columnOrder.value.includes(id))]
    : allIds
  const sourceIndex = currentOrder.indexOf(sourceColumnId)
  const targetIndex = currentOrder.indexOf(targetColumnId)
  if (sourceIndex < 0 || targetIndex < 0) {
    return
  }

  currentOrder.splice(sourceIndex, 1)
  currentOrder.splice(targetIndex, 0, sourceColumnId)
  columnOrder.value = currentOrder
}

function getPinningStyles(column: Column<PatientGridRow>, header = false): CSSProperties {
  const pinned = column.getIsPinned()
  const isLastLeftPinnedColumn = pinned === 'left' && column.getIsLastColumn('left')
  const isFirstRightPinnedColumn = pinned === 'right' && column.getIsFirstColumn('right')

  return {
    boxShadow: isLastLeftPinnedColumn
      ? '4px 0 6px -4px var(--border)'
      : isFirstRightPinnedColumn
        ? '-4px 0 6px -4px var(--border)'
        : undefined,
    left: pinned === 'left' ? `${column.getStart('left')}px` : undefined,
    right: pinned === 'right' ? `${column.getAfter('right')}px` : undefined,
    position: pinned ? 'sticky' : 'relative',
    width: `${column.getSize()}px`,
    minWidth: `${column.getSize()}px`,
    maxWidth: `${column.getSize()}px`,
    zIndex: pinned ? (header ? 30 : 10) : (header ? 20 : 0),
  }
}

function groupDetails(row: Row<PatientGridRow>) {
  const field = row.groupingColumnId
  const rawValue = field ? row.getValue(field) : ''
  const key = typeof rawValue === 'boolean'
    ? String(rawValue).toLowerCase()
    : String(rawValue)
  const summary = groupSummaries.value.find(item => item.key === key)
  return {
    label: summary?.label ?? String(rawValue),
    count: summary?.count ?? row.subRows.length,
  }
}

function filterLabel(filter: PatientGridFilter): string {
  const field = columnLabels[filter.field] ?? filter.field
  const operators: Record<string, string> = {
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
  }
  return `${field} ${operators[filter.operator] ?? filter.operator} ${filterValueLabel(filter)}`
}

function filterValueLabel(filter: PatientGridFilter): string {
  if (filter.field === 'gender') {
    return filter.value === '1' ? 'Male' : 'Female'
  }
  if (filter.field === 'isActive') {
    return filter.value === 'true' ? 'Active' : 'Inactive'
  }
  if (filter.field === 'specialStatus') {
    return filter.value === 'true' ? 'Special' : 'Standard'
  }
  if (filter.field === 'regionId') {
    return regions.value.find(region => String(region.id) === filter.value)?.name ?? filter.value ?? ''
  }
  if (filter.field === 'districtId') {
    return regions.value.flatMap(region => region.districts).find(district => String(district.id) === filter.value)?.name ?? filter.value ?? ''
  }
  if (filter.field === 'groupId') {
    return groups.value.find(group => String(group.id) === filter.value)?.name ?? filter.value ?? ''
  }
  return filter.operator === 'between'
    ? `${filter.value ?? ''} - ${filter.valueTo ?? ''}`
    : filter.value ?? ''
}

async function exportPatients(selectedOnly: boolean) {
  if (!canExport.value) {
    toast.error('You do not have permission to export patients')
    return
  }
  if (selectedOnly && selectedIds.value.length === 0) {
    toast.error('Select at least one patient to export')
    return
  }

  exporting.value = true
  try {
    const request: PatientGridExportRequest = {
      ...gridRequest.value,
      selectedIds: selectedOnly ? selectedIds.value : [],
    }
    const result = await patientApi.gridExport(request)
    await createPatientWorkbook(result.items, result.groups)

    if (result.totalCount > result.items.length) {
      toast.warning(`Export was limited to ${result.items.length.toLocaleString()} patients`)
    }
    else {
      toast.success(`Exported ${result.items.length.toLocaleString()} patients`)
    }
  }
  catch (error) {
    toast.error(toPatientError(error, 'Patient export could not be created').message)
  }
  finally {
    exporting.value = false
  }
}

async function createPatientWorkbook(rows: PatientGridRow[], summaries: PatientGridGroupSummary[]) {
  const { Workbook } = await import('exceljs')
  const workbook = new Workbook()
  const worksheet = workbook.addWorksheet('Patients', {
    views: [{ state: 'frozen', ySplit: 1 }],
  })
  const visibleColumnIds = new Set(table.getVisibleLeafColumns().map(column => column.id))
  const visibleExportColumns = patientExportColumns().filter(column => visibleColumnIds.has(column.id))
  const exportColumns = visibleExportColumns.length > 0
    ? visibleExportColumns
    : patientExportColumns().slice(0, 2)

  worksheet.columns = exportColumns.map(column => ({
    key: column.id,
    width: column.width,
  }))

  const header = worksheet.addRow(exportColumns.map(column => column.label))
  header.font = { bold: true, color: { argb: 'FFFFFFFF' } }
  header.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF334155' } }
  header.alignment = { vertical: 'middle' }
  header.height = 24
  worksheet.autoFilter = {
    from: { row: 1, column: 1 },
    to: { row: 1, column: exportColumns.length },
  }

  let previousGroupKey: string | null = null
  for (const patient of rows) {
    if (groupBy.value) {
      const rawGroupValue = patient[groupBy.value as keyof PatientGridRow]
      const groupKey = typeof rawGroupValue === 'boolean'
        ? String(rawGroupValue).toLowerCase()
        : String(rawGroupValue)
      if (groupKey !== previousGroupKey) {
        const summary = summaries.find(item => item.key === groupKey)
        const groupRow = worksheet.addRow([`${summary?.label ?? rawGroupValue} (${summary?.count ?? 0})`])
        if (exportColumns.length > 1) {
          worksheet.mergeCells(groupRow.number, 1, groupRow.number, exportColumns.length)
        }
        groupRow.font = { bold: true }
        groupRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE2E8F0' } }
        previousGroupKey = groupKey
      }
    }

    worksheet.addRow(exportColumns.map(column => column.value(patient)))
  }

  worksheet.eachRow((row, rowNumber) => {
    if (rowNumber > 1) {
      row.alignment = { vertical: 'middle' }
    }
  })

  const buffer = await workbook.xlsx.writeBuffer()
  downloadBlob(
    new Blob([buffer as unknown as BlobPart], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    }),
    `patients-${new Date().toISOString().slice(0, 10)}.xlsx`,
  )
}

function patientExportColumns(): ExportColumn[] {
  return [
    { id: 'id', label: 'ID', width: 10, value: patient => patient.id },
    { id: 'inn', label: 'INN', width: 18, value: patient => patient.inn },
    { id: 'fullName', label: 'Full name', width: 34, value: patient => patient.fullName },
    { id: 'lastName', label: 'Last name', width: 22, value: patient => patient.lastName },
    { id: 'firstName', label: 'First name', width: 22, value: patient => patient.firstName },
    { id: 'middleName', label: 'Middle name', width: 22, value: patient => patient.middleName },
    { id: 'gender', label: 'Gender', width: 12, value: patient => patient.gender === 1 ? 'Male' : 'Female' },
    { id: 'birthDate', label: 'Birth date', width: 14, value: patient => patient.birthDate },
    { id: 'phone', label: 'Phone', width: 20, value: patient => patient.phone },
    { id: 'regionName', label: 'Region', width: 24, value: patient => patient.regionName },
    { id: 'districtName', label: 'District', width: 24, value: patient => patient.districtName },
    { id: 'groupName', label: 'Group', width: 16, value: patient => patient.groupName },
    { id: 'specialStatus', label: 'Special', width: 12, value: patient => patient.specialStatus ? 'Yes' : 'No' },
    { id: 'isActive', label: 'Active', width: 12, value: patient => patient.isActive ? 'Yes' : 'No' },
    { id: 'createdAt', label: 'Registered', width: 22, value: patient => patient.createdAt },
    { id: 'updatedAt', label: 'Updated', width: 22, value: patient => patient.updatedAt },
    { id: 'address', label: 'Address', width: 40, value: patient => patient.address },
    { id: 'address2', label: 'Address 2', width: 40, value: patient => patient.address2 },
  ]
}

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = filename
  anchor.click()
  URL.revokeObjectURL(url)
}

function toPatientError(error: unknown, fallback: string): Error {
  if (axios.isAxiosError<ApiProblemDetails>(error)) {
    const problem = error.response?.data
    const validationMessage = problem?.errors
      ? Object.values(problem.errors).flat()[0]
      : undefined
    return new Error(validationMessage ?? problem?.detail ?? problem?.title ?? fallback)
  }
  return error instanceof Error ? error : new Error(fallback)
}
</script>

<template>
  <BasicPage title="Patients" description="Global patient registry" sticky>
    <div class="flex h-[calc(100dvh-154px)] min-h-[560px] flex-col overflow-hidden rounded-md border bg-background">
      <div class="flex flex-wrap items-center gap-2 border-b p-3">
        <UiInputGroup class="w-full sm:w-[280px]">
          <UiInputGroupAddon align="inline-start">
            <SearchIcon class="size-4 text-muted-foreground" />
          </UiInputGroupAddon>
          <UiInputGroupInput v-model="searchInput" placeholder="Search patients..." />
          <UiInputGroupAddon v-if="searchInput" align="inline-end">
            <UiInputGroupButton type="button" size="icon-xs" title="Clear search" @click="searchInput = ''">
              <XIcon class="size-3.5" />
              <span class="sr-only">Clear search</span>
            </UiInputGroupButton>
          </UiInputGroupAddon>
        </UiInputGroup>

        <PatientGridFilters
          :model-value="filters"
          :groups="groups"
          :regions="regions"
          @update:model-value="updateFilters"
        />

        <UiSelect :model-value="groupBy ?? 'none'" @update:model-value="setGrouping">
          <UiSelectTrigger class="h-9 w-[170px]">
            <ChevronsUpDownIcon class="size-4 text-muted-foreground" />
            <UiSelectValue placeholder="Group by" />
          </UiSelectTrigger>
          <UiSelectContent>
            <UiSelectItem value="none">
              No grouping
            </UiSelectItem>
            <UiSelectItem value="regionName">
              Region
            </UiSelectItem>
            <UiSelectItem value="districtName">
              District
            </UiSelectItem>
            <UiSelectItem value="groupName">
              Patient group
            </UiSelectItem>
            <UiSelectItem value="gender">
              Gender
            </UiSelectItem>
            <UiSelectItem value="specialStatus">
              Special status
            </UiSelectItem>
            <UiSelectItem value="isActive">
              Active status
            </UiSelectItem>
          </UiSelectContent>
        </UiSelect>

        <div class="flex-1" />

        <UiButton
          v-if="canCreate"
          size="sm"
          class="h-9"
          :disabled="!lookupsReady || loadingLookups"
          @click="openCreatePatient"
        >
          <PlusIcon class="size-4" />
          New patient
        </UiButton>

        <UiTooltipProvider>
          <UiTooltip>
            <UiTooltipTrigger as-child>
              <UiButton variant="outline" size="icon" class="size-9" :disabled="loading" @click="refreshGrid">
                <RefreshCwIcon class="size-4" :class="loading ? 'animate-spin' : ''" />
                <span class="sr-only">Refresh patients</span>
              </UiButton>
            </UiTooltipTrigger>
            <UiTooltipContent>Refresh patients</UiTooltipContent>
          </UiTooltip>
        </UiTooltipProvider>

        <UiDropdownMenu>
          <UiDropdownMenuTrigger as-child>
            <UiButton variant="outline" size="icon" class="size-9" title="Choose columns">
              <Columns3Icon class="size-4" />
              <span class="sr-only">Choose columns</span>
            </UiButton>
          </UiDropdownMenuTrigger>
          <UiDropdownMenuContent align="end" class="max-h-[420px] w-56 overflow-y-auto">
            <UiDropdownMenuLabel>Visible columns</UiDropdownMenuLabel>
            <UiDropdownMenuSeparator />
            <UiDropdownMenuCheckboxItem
              v-for="column in table.getAllLeafColumns().filter(item => item.getCanHide())"
              :key="column.id"
              :model-value="column.getIsVisible()"
              @select.prevent
              @update:model-value="value => column.toggleVisibility(Boolean(value))"
            >
              {{ columnLabels[column.id] ?? column.id }}
            </UiDropdownMenuCheckboxItem>
          </UiDropdownMenuContent>
        </UiDropdownMenu>

        <UiDropdownMenu v-if="canExport">
          <UiDropdownMenuTrigger as-child>
            <UiButton variant="outline" size="sm" class="h-9" :disabled="exporting">
              <FileSpreadsheetIcon class="size-4" />
              Export
              <ChevronDownIcon class="size-3.5 text-muted-foreground" />
            </UiButton>
          </UiDropdownMenuTrigger>
          <UiDropdownMenuContent align="end" class="w-60">
            <UiDropdownMenuItem @click="exportPatients(false)">
              <ArrowDownToLineIcon class="size-4" />
              All filtered patients
            </UiDropdownMenuItem>
            <UiDropdownMenuItem :disabled="selectedCount === 0" @click="exportPatients(true)">
              <FileSpreadsheetIcon class="size-4" />
              Selected patients ({{ selectedCount }})
            </UiDropdownMenuItem>
          </UiDropdownMenuContent>
        </UiDropdownMenu>

        <UiTooltipProvider>
          <UiTooltip>
            <UiTooltipTrigger as-child>
              <UiButton variant="ghost" size="icon" class="size-9" @click="resetGridLayout">
                <RotateCcwIcon class="size-4" />
                <span class="sr-only">Reset grid layout</span>
              </UiButton>
            </UiTooltipTrigger>
            <UiTooltipContent>Reset grid layout</UiTooltipContent>
          </UiTooltip>
        </UiTooltipProvider>
      </div>

      <div v-if="filters.length" class="flex min-h-11 flex-wrap items-center gap-2 border-b bg-muted/20 px-3 py-2">
        <span class="text-xs font-medium text-muted-foreground">Active filters</span>
        <UiBadge v-for="(filter, index) in filters" :key="`${filter.field}-${index}`" variant="secondary" class="gap-1 pl-2 pr-1">
          {{ filterLabel(filter) }}
          <button type="button" class="rounded-full p-0.5 hover:bg-background/80" title="Remove filter" @click="removeFilter(index)">
            <XIcon class="size-3" />
            <span class="sr-only">Remove filter</span>
          </button>
        </UiBadge>
      </div>

      <div v-if="errorMessage" class="flex items-center justify-between gap-3 border-b border-destructive/30 bg-destructive/5 px-3 py-2 text-sm text-destructive">
        <span>{{ errorMessage }}</span>
        <UiButton variant="ghost" size="sm" @click="refreshGrid">
          Try again
        </UiButton>
      </div>

      <div class="relative min-h-0 flex-1">
        <div class="patient-grid-scroll h-full overflow-x-scroll overflow-y-auto">
          <UiTable
            container-class="overflow-visible"
            class="patient-grid-table table-fixed border-separate border-spacing-0"
            :style="{ width: `${table.getTotalSize()}px`, minWidth: '100%' }"
          >
            <UiTableHeader class="sticky top-0 z-20 bg-muted/30">
              <UiTableRow v-for="headerGroup in table.getHeaderGroups()" :key="headerGroup.id" class="hover:bg-transparent">
                <UiTableHead
                  v-for="header in headerGroup.headers"
                  :key="header.id"
                  :style="getPinningStyles(header.column, true)"
                  :draggable="header.column.id !== 'select' && header.column.id !== 'actions'"
                  class="group/header relative bg-muted/30"
                  @dragstart="startColumnDrag(header.column.id)"
                  @dragover.prevent
                  @drop="dropColumn(header.column.id)"
                >
                  <FlexRender v-if="!header.isPlaceholder" :render="header.column.columnDef.header" :props="header.getContext()" />
                  <button
                    v-if="header.column.getCanResize()"
                    type="button"
                    class="absolute -right-1 top-1/2 z-40 h-6 w-2 -translate-y-1/2 cursor-col-resize touch-none select-none after:absolute after:inset-y-0 after:left-1/2 after:w-px after:-translate-x-1/2 after:transition-colors"
                    :class="header.column.getIsResizing() ? 'after:bg-primary' : 'after:bg-transparent hover:after:bg-primary/70'"
                    title="Drag to resize column"
                    @dblclick="header.column.resetSize()"
                    @mousedown="header.getResizeHandler()($event)"
                    @touchstart="header.getResizeHandler()($event)"
                  />
                </UiTableHead>
              </UiTableRow>
            </UiTableHeader>

            <UiTableBody>
              <template v-if="table.getRowModel().rows.length">
                <template v-for="row in table.getRowModel().rows" :key="row.id">
                  <UiTableRow v-if="row.getIsGrouped()" class="border-b-0 bg-muted/40 hover:bg-muted/60">
                    <UiTableCell :colspan="table.getVisibleLeafColumns().length" class="h-11 border-b px-3">
                      <button type="button" class="flex w-full items-center gap-2 text-left" @click="row.toggleExpanded()">
                        <ChevronRightIcon class="size-4 transition-transform" :class="row.getIsExpanded() ? 'rotate-90' : ''" />
                        <span class="font-medium">{{ groupDetails(row).label }}</span>
                        <UiBadge variant="outline">
                          {{ groupDetails(row).count.toLocaleString() }}
                        </UiBadge>
                      </button>
                    </UiTableCell>
                  </UiTableRow>

                  <UiTableRow
                    v-else
                    :data-state="row.getIsSelected() ? 'selected' : undefined"
                    class="group/row h-12 border-b-0 data-[state=selected]:bg-muted/70"
                  >
                    <UiTableCell
                      v-for="cell in row.getVisibleCells()"
                      :key="cell.id"
                      :style="getPinningStyles(cell.column)"
                      class="overflow-hidden border-b bg-background px-2 group-hover/row:bg-muted/40 group-data-[state=selected]/row:bg-muted/70"
                    >
                      <FlexRender :render="cell.column.columnDef.cell" :props="cell.getContext()" />
                    </UiTableCell>
                  </UiTableRow>
                </template>
              </template>

              <UiTableRow v-else-if="!loading">
                <UiTableCell :colspan="table.getVisibleLeafColumns().length" class="h-56 text-center">
                  <div class="mx-auto grid max-w-sm justify-items-center gap-2 text-muted-foreground">
                    <SearchIcon class="size-6" />
                    <p class="font-medium text-foreground">
                      No patients found
                    </p>
                    <p class="text-sm">
                      Change the search or filter conditions and try again.
                    </p>
                  </div>
                </UiTableCell>
              </UiTableRow>
            </UiTableBody>
          </UiTable>
        </div>

        <div v-if="loading" class="absolute inset-0 z-40 grid place-items-center bg-background/65 backdrop-blur-[1px]">
          <div class="flex items-center gap-2 rounded-md border bg-background px-4 py-2 text-sm shadow-sm">
            <RefreshCwIcon class="size-4 animate-spin" />
            Loading patients
          </div>
        </div>
      </div>

      <div class="flex flex-wrap items-center gap-3 border-t bg-muted/20 px-3 py-2 text-sm">
        <div class="min-w-36 text-muted-foreground">
          <span class="font-medium text-foreground">{{ totalCount.toLocaleString() }}</span> patients
          <span v-if="selectedCount"> · {{ selectedCount }} selected</span>
        </div>

        <div class="flex-1" />

        <div class="flex items-center gap-2">
          <span class="hidden text-muted-foreground sm:inline">Rows per page</span>
          <UiNativeSelect
            :model-value="String(pagination.pageSize)"
            class="w-[74px]"
            @change="onPageSizeChange"
          >
            <UiNativeSelectOption v-for="size in pageSizes" :key="size" :value="String(size)">
              {{ size }}
            </UiNativeSelectOption>
          </UiNativeSelect>
        </div>

        <span class="w-28 text-center text-muted-foreground">
          Page {{ pagination.pageIndex + 1 }} of {{ pageCount }}
        </span>

        <div class="flex items-center gap-1">
          <UiButton variant="outline" size="icon-sm" :disabled="pagination.pageIndex === 0" title="First page" @click="table.setPageIndex(0)">
            <ChevronFirstIcon class="size-4" />
            <span class="sr-only">First page</span>
          </UiButton>
          <UiButton variant="outline" size="icon-sm" :disabled="!table.getCanPreviousPage()" title="Previous page" @click="table.previousPage()">
            <ChevronLeftIcon class="size-4" />
            <span class="sr-only">Previous page</span>
          </UiButton>
          <UiButton variant="outline" size="icon-sm" :disabled="!table.getCanNextPage()" title="Next page" @click="table.nextPage()">
            <ChevronRightIcon class="size-4" />
            <span class="sr-only">Next page</span>
          </UiButton>
          <UiButton variant="outline" size="icon-sm" :disabled="pagination.pageIndex >= pageCount - 1" title="Last page" @click="table.setPageIndex(pageCount - 1)">
            <ChevronLastIcon class="size-4" />
            <span class="sr-only">Last page</span>
          </UiButton>
        </div>
      </div>
    </div>

    <PatientEditorPopup
      ref="patientEditorRef"
      :groups="groups"
      :regions="regions"
      @saved="onPatientSaved"
    />

    <UiAlertDialog :open="!!deleteTarget" @update:open="open => !open && !deleting && (deleteTarget = null)">
      <UiAlertDialogContent>
        <UiAlertDialogHeader>
          <UiAlertDialogTitle>Delete patient?</UiAlertDialogTitle>
          <UiAlertDialogDescription>
            {{ deleteTarget?.fullName }} will be removed from active patient lists. This operation uses soft delete.
          </UiAlertDialogDescription>
        </UiAlertDialogHeader>
        <UiAlertDialogFooter>
          <UiAlertDialogCancel :disabled="deleting">
            Cancel
          </UiAlertDialogCancel>
          <UiAlertDialogAction
            :disabled="deleting"
            class="bg-destructive text-white hover:bg-destructive/90"
            @click.prevent="deletePatient"
          >
            <Trash2Icon class="size-4" />
            Delete patient
          </UiAlertDialogAction>
        </UiAlertDialogFooter>
      </UiAlertDialogContent>
    </UiAlertDialog>
  </BasicPage>
</template>

<style scoped>
.patient-grid-scroll {
  overscroll-behavior: contain;
  scrollbar-color: color-mix(in srgb, var(--muted-foreground) 45%, transparent) var(--muted);
  scrollbar-gutter: stable;
}

.patient-grid-scroll::-webkit-scrollbar {
  width: 10px;
  height: 12px;
}

.patient-grid-scroll::-webkit-scrollbar-track {
  background: var(--muted);
}

.patient-grid-scroll::-webkit-scrollbar-thumb {
  border: 3px solid transparent;
  border-radius: 999px;
  background: color-mix(in srgb, var(--muted-foreground) 55%, transparent);
  background-clip: content-box;
}

.patient-grid-scroll::-webkit-scrollbar-thumb:hover {
  background: color-mix(in srgb, var(--foreground) 55%, transparent);
  background-clip: content-box;
}

.patient-grid-table :deep([data-slot='table-head'] [data-slot='button']) {
  margin-left: 0;
  max-width: 100%;
  justify-content: flex-start;
  padding-inline: 0.375rem;
}

.patient-grid-table :deep([data-slot='table-head'] [data-slot='button'] > span) {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
