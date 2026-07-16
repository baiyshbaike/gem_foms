import type {
  ColumnOrderState,
  ColumnPinningState,
  ColumnSizingState,
  GroupingState,
  SortingState,
  VisibilityState,
} from '@tanstack/vue-table'

export type ServerDataGridFilterOperator
  = | 'contains'
    | 'notContains'
    | 'startsWith'
    | 'endsWith'
    | 'equals'
    | 'notEquals'
    | 'greaterThan'
    | 'greaterThanOrEqual'
    | 'lessThan'
    | 'lessThanOrEqual'
    | 'between'
    | 'isEmpty'
    | 'isNotEmpty'

export interface ServerDataGridSort {
  field: string
  descending: boolean
}

export interface ServerDataGridFilter {
  field: string
  operator: ServerDataGridFilterOperator
  value: string | null
  valueTo: string | null
}

export interface ServerDataGridQueryRequest {
  page: number
  pageSize: number
  search: string | null
  sorting: ServerDataGridSort[]
  filters: ServerDataGridFilter[]
  groupBy: string | null
}

export interface ServerDataGridGroupSummary {
  key: string
  label: string
  count: number
}

export interface ServerDataGridQueryResult<TData> {
  items: TData[]
  totalCount: number
  groups: ServerDataGridGroupSummary[]
}

export interface ServerDataGridQueryState {
  pageIndex: number
  pageSize: number
  search: string
  sorting: Array<{ id: string, desc: boolean }>
  filters: ServerDataGridFilter[]
  groupBy: string | null
}

export type ServerDataGridFilterValueType = 'text' | 'date' | 'number' | 'select'

export interface ServerDataGridOption {
  label: string
  value: string
}

export interface ServerDataGridFilterField {
  field: string
  label: string
  type: ServerDataGridFilterValueType
  options?: ServerDataGridOption[]
}

export interface ServerDataGridGroupOption {
  field: string
  label: string
}

export interface ServerDataGridExportColumn<TData> {
  id: string
  label: string
  width: number
  value: (row: TData) => string | number | boolean | null
}

export interface ServerDataGridExportConfig<TData> {
  columns: ServerDataGridExportColumn<TData>[]
  fileName: string | (() => string)
  load: (
    request: ServerDataGridQueryRequest,
    selectedRowIds: string[],
  ) => Promise<ServerDataGridQueryResult<TData>>
  sheetName: string
  allowSelected?: boolean
}

export interface ServerDataGridPersistedState {
  version: number
  columnOrder?: ColumnOrderState
  columnPinning?: ColumnPinningState
  columnSizing?: ColumnSizingState
  columnVisibility?: VisibilityState
  filters?: ServerDataGridFilter[]
  grouping?: GroupingState
  pageSize?: number
  search?: string
  sorting?: SortingState
}

export interface ServerDataGridExposed {
  clearSelection: () => void
  refresh: () => void
  removeSelection: (id: string | number) => void
}
