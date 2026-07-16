<script setup lang="ts" generic="TData extends object">
import type {
  Column,
  ColumnDef,
  ColumnPinningState,
  Row,
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
  Columns3Icon,
  FileSpreadsheetIcon,
  RefreshCwIcon,
  RotateCcwIcon,
  SearchIcon,
  XIcon,
} from '@lucide/vue'
import { FlexRender } from '@tanstack/vue-table'
import { toast } from 'vue-sonner'

import { SelectColumn } from '@/components/data-table'
import { cn } from '@/lib/utils'

import type {
  ServerDataGridExportConfig,
  ServerDataGridExposed,
  ServerDataGridFilterField,
  ServerDataGridGroupOption,
  ServerDataGridQueryRequest,
  ServerDataGridQueryResult,
} from './server-data-grid.types'

import { createServerDataGridWorkbook } from './server-data-grid-export'
import ServerDataGridFilters from './server-data-grid-filters.vue'
import { formatServerDataGridFilter } from './server-data-grid.utils'
import { useServerDataGrid } from './use-server-data-grid'

const props = withDefaults(defineProps<{
  columnLabels: Record<string, string>
  columns: ColumnDef<TData>[]
  containerClass?: string
  defaultColumnPinning?: ColumnPinningState
  defaultColumnVisibility?: VisibilityState
  defaultPageSize?: number
  defaultSorting?: SortingState
  emptyDescription?: string
  emptyTitle?: string
  enableSelection?: boolean
  exportConfig?: ServerDataGridExportConfig<TData>
  filterDescription?: string
  filterFields?: ServerDataGridFilterField[]
  filterTitle?: string
  formatError?: (error: unknown, fallback: string) => string
  getRowId: (row: TData) => string | number
  groupOptions?: ServerDataGridGroupOption[]
  itemLabel?: string
  load: (request: ServerDataGridQueryRequest) => Promise<ServerDataGridQueryResult<TData>>
  loadErrorMessage?: string
  loadingLabel?: string
  pageSizes?: number[]
  searchPlaceholder?: string
  stateVersion?: number
  storageKey: string
}>(), {
  containerClass: '',
  defaultColumnPinning: () => ({ left: [], right: [] }),
  defaultColumnVisibility: () => ({}),
  defaultPageSize: 25,
  defaultSorting: () => [],
  emptyDescription: 'Change the search or filter conditions and try again.',
  emptyTitle: 'No results found',
  enableSelection: true,
  exportConfig: undefined,
  filterDescription: 'Combine filters to narrow the result set on the server.',
  filterFields: () => [],
  filterTitle: 'Filters',
  formatError: undefined,
  groupOptions: () => [],
  itemLabel: 'items',
  loadErrorMessage: 'Data could not be loaded',
  loadingLabel: 'Loading data',
  pageSizes: () => [10, 25, 50, 100],
  searchPlaceholder: 'Search...',
  stateVersion: 1,
})

const gridColumns = computed<ColumnDef<TData>[]>(() => props.enableSelection
  ? [SelectColumn as ColumnDef<TData>, ...props.columns]
  : props.columns)

const {
  clearSelection,
  columnOrder,
  errorMessage,
  filters,
  groupBy,
  groupSummaries,
  loading,
  pageCount,
  pagination,
  query,
  refresh,
  removeFilter,
  removeSelection,
  reset,
  searchInput,
  selectedIds,
  setFilters,
  setGrouping,
  table,
  totalCount,
} = useServerDataGrid<TData>({
  columns: gridColumns,
  defaultColumnPinning: props.defaultColumnPinning,
  defaultColumnVisibility: props.defaultColumnVisibility,
  defaultPageSize: props.defaultPageSize,
  defaultSorting: props.defaultSorting,
  enableSelection: props.enableSelection,
  formatError: (error, fallback) => props.formatError?.(error, fallback) ?? defaultErrorMessage(error, fallback),
  getRowId: row => String(props.getRowId(row)),
  load: request => props.load(request),
  loadErrorMessage: props.loadErrorMessage,
  pageSizes: props.pageSizes,
  stateVersion: props.stateVersion,
  storageKey: props.storageKey,
})

const exporting = ref(false)
const draggedColumnId = ref<string | null>(null)
const selectedCount = computed(() => selectedIds.value.length)

function defaultErrorMessage(error: unknown, fallback: string) {
  return error instanceof Error ? error.message : fallback
}

function resetGrid() {
  reset()
  toast.success('Grid layout reset')
}

function onPageSizeChange(event: Event) {
  table.setPageSize(Number((event.target as HTMLSelectElement).value))
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

function getPinningStyles(column: Column<TData>, header = false): CSSProperties {
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
    isolation: pinned ? 'isolate' : undefined,
    position: pinned ? 'sticky' : 'relative',
    width: `${column.getSize()}px`,
    minWidth: `${column.getSize()}px`,
    maxWidth: `${column.getSize()}px`,
    zIndex: pinned ? (header ? 30 : 10) : (header ? 20 : 0),
  }
}

function groupDetails(row: Row<TData>) {
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

async function exportRows(selectedOnly: boolean) {
  const config = props.exportConfig
  if (!config) {
    return
  }
  if (selectedOnly && selectedIds.value.length === 0) {
    toast.error(`Select at least one ${singularize(props.itemLabel)} to export`)
    return
  }

  exporting.value = true
  try {
    const result = await config.load(query.value, selectedOnly ? selectedIds.value : [])
    await createServerDataGridWorkbook({
      columns: config.columns,
      fileName: typeof config.fileName === 'function' ? config.fileName() : config.fileName,
      groupBy: groupBy.value,
      rows: result.items,
      sheetName: config.sheetName,
      summaries: result.groups,
      visibleColumnIds: table.getVisibleLeafColumns().map(column => column.id),
    })

    if (result.totalCount > result.items.length) {
      toast.warning(`Export was limited to ${result.items.length.toLocaleString()} ${props.itemLabel}`)
    }
    else {
      toast.success(`Exported ${result.items.length.toLocaleString()} ${props.itemLabel}`)
    }
  }
  catch (error) {
    toast.error(props.formatError?.(error, 'Export could not be created') ?? defaultErrorMessage(error, 'Export could not be created'))
  }
  finally {
    exporting.value = false
  }
}

function singularize(label: string) {
  return label.endsWith('s') ? label.slice(0, -1) : label
}

defineExpose<ServerDataGridExposed>({
  clearSelection,
  refresh,
  removeSelection,
})
</script>

<template>
  <div :class="cn('flex h-[calc(100dvh-154px)] min-h-[560px] flex-col overflow-hidden rounded-md border bg-background', containerClass)">
    <div class="flex flex-wrap items-center gap-2 border-b p-3">
      <UiInputGroup class="w-full sm:w-[280px]">
        <UiInputGroupAddon align="inline-start">
          <SearchIcon class="size-4 text-muted-foreground" />
        </UiInputGroupAddon>
        <UiInputGroupInput v-model="searchInput" :placeholder="searchPlaceholder" />
        <UiInputGroupAddon v-if="searchInput" align="inline-end">
          <UiInputGroupButton type="button" size="icon-xs" title="Clear search" @click="searchInput = ''">
            <XIcon class="size-3.5" />
            <span class="sr-only">Clear search</span>
          </UiInputGroupButton>
        </UiInputGroupAddon>
      </UiInputGroup>

      <ServerDataGridFilters
        v-if="filterFields.length"
        :model-value="filters"
        :fields="filterFields"
        :default-field="filterFields[0]?.field"
        :title="filterTitle"
        :description="filterDescription"
        @update:model-value="setFilters"
      />

      <UiSelect v-if="groupOptions.length" :model-value="groupBy ?? 'none'" @update:model-value="setGrouping">
        <UiSelectTrigger class="h-9 w-[170px]">
          <UiSelectValue placeholder="Group by" />
        </UiSelectTrigger>
        <UiSelectContent>
          <UiSelectItem value="none">
            No grouping
          </UiSelectItem>
          <UiSelectItem v-for="option in groupOptions" :key="option.field" :value="option.field">
            {{ option.label }}
          </UiSelectItem>
        </UiSelectContent>
      </UiSelect>

      <div class="flex-1" />
      <slot name="toolbar-actions" />

      <UiTooltipProvider>
        <UiTooltip>
          <UiTooltipTrigger as-child>
            <UiButton variant="outline" size="icon" class="size-9" :disabled="loading" @click="refresh">
              <RefreshCwIcon class="size-4" :class="loading ? 'animate-spin' : ''" />
              <span class="sr-only">Refresh data</span>
            </UiButton>
          </UiTooltipTrigger>
          <UiTooltipContent>Refresh</UiTooltipContent>
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

      <UiDropdownMenu v-if="exportConfig">
        <UiDropdownMenuTrigger as-child>
          <UiButton variant="outline" size="sm" class="h-9" :disabled="exporting">
            <FileSpreadsheetIcon class="size-4" />
            Export
            <ChevronDownIcon class="size-3.5 text-muted-foreground" />
          </UiButton>
        </UiDropdownMenuTrigger>
        <UiDropdownMenuContent align="end" class="w-60">
          <UiDropdownMenuItem @click="exportRows(false)">
            <ArrowDownToLineIcon class="size-4" />
            All filtered {{ itemLabel }}
          </UiDropdownMenuItem>
          <UiDropdownMenuItem
            v-if="enableSelection && exportConfig.allowSelected !== false"
            :disabled="selectedCount === 0"
            @click="exportRows(true)"
          >
            <FileSpreadsheetIcon class="size-4" />
            Selected {{ itemLabel }} ({{ selectedCount }})
          </UiDropdownMenuItem>
        </UiDropdownMenuContent>
      </UiDropdownMenu>

      <UiTooltipProvider>
        <UiTooltip>
          <UiTooltipTrigger as-child>
            <UiButton variant="ghost" size="icon" class="size-9" @click="resetGrid">
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
        {{ formatServerDataGridFilter(filter, filterFields) }}
        <button type="button" class="rounded-full p-0.5 hover:bg-background/80" title="Remove filter" @click="removeFilter(index)">
          <XIcon class="size-3" />
          <span class="sr-only">Remove filter</span>
        </button>
      </UiBadge>
    </div>

    <div v-if="errorMessage" class="flex items-center justify-between gap-3 border-b border-destructive/30 bg-destructive/5 px-3 py-2 text-sm text-destructive">
      <span>{{ errorMessage }}</span>
      <UiButton variant="ghost" size="sm" @click="refresh">
        Try again
      </UiButton>
    </div>

    <div class="relative min-h-0 flex-1">
      <div class="server-data-grid-scroll h-full overflow-x-scroll overflow-y-auto">
        <UiTable
          container-class="overflow-visible"
          class="server-data-grid-table table-fixed border-separate border-spacing-0"
          :style="{ width: `${table.getTotalSize()}px`, minWidth: '100%' }"
        >
          <UiTableHeader class="sticky top-0 z-20 bg-muted/30">
            <UiTableRow v-for="headerGroup in table.getHeaderGroups()" :key="headerGroup.id" class="hover:bg-transparent">
              <UiTableHead
                v-for="header in headerGroup.headers"
                :key="header.id"
                :style="getPinningStyles(header.column, true)"
                :draggable="header.column.id !== 'select' && header.column.id !== 'actions'"
                class="relative"
                :class="[
                  header.column.getIsPinned() ? 'bg-muted' : 'bg-muted/30',
                  header.column.getIsPinned() === 'right' ? 'border-l' : '',
                ]"
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
                    class="overflow-hidden border-b px-2"
                    :class="[
                      cell.column.getIsPinned()
                        ? 'bg-background group-hover/row:bg-muted group-data-[state=selected]/row:bg-muted'
                        : 'bg-background group-hover/row:bg-muted/40 group-data-[state=selected]/row:bg-muted/70',
                      cell.column.getIsPinned() === 'right' ? 'border-l' : '',
                    ]"
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
                    {{ emptyTitle }}
                  </p>
                  <p class="text-sm">
                    {{ emptyDescription }}
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
          {{ loadingLabel }}
        </div>
      </div>
    </div>

    <div class="flex flex-wrap items-center gap-3 border-t bg-muted/20 px-3 py-2 text-sm">
      <div class="min-w-36 text-muted-foreground">
        <span class="font-medium text-foreground">{{ totalCount.toLocaleString() }}</span> {{ itemLabel }}
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
</template>

<style scoped>
.server-data-grid-scroll {
  overscroll-behavior: contain;
  scrollbar-color: color-mix(in srgb, var(--muted-foreground) 45%, transparent) var(--muted);
  scrollbar-gutter: stable;
}

.server-data-grid-scroll::-webkit-scrollbar {
  width: 10px;
  height: 12px;
}

.server-data-grid-scroll::-webkit-scrollbar-track {
  background: var(--muted);
}

.server-data-grid-scroll::-webkit-scrollbar-thumb {
  border: 3px solid transparent;
  border-radius: 999px;
  background: color-mix(in srgb, var(--muted-foreground) 55%, transparent);
  background-clip: content-box;
}

.server-data-grid-scroll::-webkit-scrollbar-thumb:hover {
  background: color-mix(in srgb, var(--foreground) 55%, transparent);
  background-clip: content-box;
}

.server-data-grid-table :deep([data-slot='table-head'] [data-slot='button']) {
  margin-left: 0;
  max-width: 100%;
  justify-content: flex-start;
  padding-inline: 0.375rem;
}

.server-data-grid-table :deep([data-slot='table-head'] [data-slot='button'] > span) {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
