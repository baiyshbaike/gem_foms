import type {
  ColumnDef,
  ColumnPinningState,
  SortingState,
  VisibilityState,
} from '@tanstack/vue-table'

import { h } from 'vue'

import type {
  ServerDataGridExportColumn,
  ServerDataGridFilterField,
  ServerDataGridGroupOption,
} from '@/components/server-data-grid'
import type {
  Region,
  TenantGridRow,
} from '@/services/types/dialysis'

import { DataTableColumnHeader } from '@/components/data-table'
import { Badge } from '@/components/ui/badge'
import { formatDateTime } from '@/lib/dialysis'

import TenantRowActions from './components/tenant-row-actions.vue'

interface CreateTenantColumnsOptions {
  disableActions: boolean
  onDeactivate: (tenant: TenantGridRow) => void
  onEdit: (tenant: TenantGridRow) => void
}

export const tenantColumnLabels: Record<string, string> = {
  id: 'ID',
  code: 'Code',
  name: 'Name',
  regionId: 'Region ID',
  regionName: 'Region',
  districtId: 'District ID',
  districtName: 'District',
  address: 'Address',
  phone: 'Phone',
  isActive: 'Active',
  createdAt: 'Created',
  disabledAt: 'Disabled',
  actions: 'Actions',
}

export const tenantDefaultColumnVisibility: VisibilityState = {
  address: false,
  disabledAt: false,
  regionId: false,
  id: false,
  phone: false,
}

export const tenantDefaultColumnPinning: ColumnPinningState = {
  left: ['select', 'code'],
  right: ['actions'],
}

export const tenantDefaultSorting: SortingState = [
  { id: 'name', desc: false },
]

export const tenantGroupOptions: ServerDataGridGroupOption[] = [
  { field: 'regionName', label: 'Region' },
  { field: 'districtName', label: 'District' },
  { field: 'isActive', label: 'Active status' },
]

export const tenantExportColumns: ServerDataGridExportColumn<TenantGridRow>[] = [
  { id: 'id', label: 'ID', width: 34, value: tenant => tenant.id },
  { id: 'code', label: 'Code', width: 16, value: tenant => tenant.code },
  { id: 'name', label: 'Name', width: 34, value: tenant => tenant.name },
  { id: 'regionName', label: 'Region', width: 28, value: tenant => tenant.regionName },
  { id: 'districtName', label: 'District', width: 28, value: tenant => tenant.districtName },
  { id: 'address', label: 'Address', width: 42, value: tenant => tenant.address },
  { id: 'phone', label: 'Phone', width: 20, value: tenant => tenant.phone },
  { id: 'isActive', label: 'Active', width: 12, value: tenant => tenant.isActive ? 'Yes' : 'No' },
  { id: 'createdAt', label: 'Created', width: 22, value: tenant => tenant.createdAt },
  { id: 'disabledAt', label: 'Disabled', width: 22, value: tenant => tenant.disabledAt },
]

export function createTenantColumns(options: CreateTenantColumnsOptions): ColumnDef<TenantGridRow>[] {
  return [
    {
      accessorKey: 'id',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'ID', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'font-mono text-xs' }, row.original.id),
      size: 280,
      minSize: 220,
    },
    {
      accessorKey: 'code',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'Code', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'font-mono text-xs font-semibold tracking-normal' }, row.original.code),
      size: 125,
      minSize: 110,
    },
    {
      accessorKey: 'name',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'Name', multiSort: true }),
      cell: ({ row }) => h('div', { class: 'grid min-w-0 py-0.5' }, [
        h('span', { class: 'truncate font-medium text-foreground' }, row.original.name),
        h('span', { class: 'truncate text-xs text-muted-foreground' }, row.original.address || 'No address'),
      ]),
      size: 280,
      minSize: 220,
      maxSize: 440,
    },
    {
      accessorKey: 'regionName',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'Region', multiSort: true }),
      size: 190,
      minSize: 160,
    },
    {
      accessorKey: 'districtName',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'District', multiSort: true }),
      size: 185,
      minSize: 150,
    },
    {
      accessorKey: 'address',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'Address', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'block truncate' }, row.original.address || '-'),
      size: 300,
      minSize: 200,
    },
    {
      accessorKey: 'phone',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'Phone', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'tabular-nums' }, row.original.phone || '-'),
      size: 155,
      minSize: 135,
    },
    {
      accessorKey: 'isActive',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'Active', multiSort: true }),
      cell: ({ row }) => h(Badge, {
        variant: row.original.isActive ? 'outline' : 'secondary',
        class: row.original.isActive ? 'border-emerald-500/40 text-emerald-700 dark:text-emerald-400' : '',
      }, () => row.original.isActive ? 'Active' : 'Inactive'),
      size: 105,
      minSize: 96,
    },
    {
      accessorKey: 'createdAt',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'Created', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'tabular-nums' }, formatDateTime(row.original.createdAt)),
      size: 175,
      minSize: 155,
    },
    {
      accessorKey: 'disabledAt',
      header: ({ column }) => h(DataTableColumnHeader<TenantGridRow>, { column, title: 'Disabled', multiSort: true }),
      cell: ({ row }) => h('span', { class: 'tabular-nums' }, formatDateTime(row.original.disabledAt)),
      size: 175,
      minSize: 155,
    },
    {
      id: 'actions',
      header: () => h('span', { class: 'block text-right' }, 'Actions'),
      cell: ({ row }) => h(TenantRowActions, {
        tenant: row.original,
        disabled: options.disableActions,
        onEdit: options.onEdit,
        onDeactivate: options.onDeactivate,
      }),
      enableHiding: false,
      enablePinning: true,
      enableResizing: false,
      enableSorting: false,
      size: 88,
      minSize: 88,
      maxSize: 88,
    },
  ]
}

export function createTenantFilterFields(
  regions: Region[],
): ServerDataGridFilterField[] {
  const districts = regions.flatMap(region => region.districts.map(district => ({
    label: `${district.name} (${region.name})`,
    value: String(district.id),
  })))

  return [
    { field: 'code', label: 'Code', type: 'text' },
    { field: 'name', label: 'Name', type: 'text' },
    { field: 'address', label: 'Address', type: 'text' },
    { field: 'phone', label: 'Phone', type: 'text' },
    {
      field: 'regionId',
      label: 'Region',
      type: 'select',
      options: regions.map(region => ({ label: region.name, value: String(region.id) })),
    },
    { field: 'districtId', label: 'District', type: 'select', options: districts },
    {
      field: 'isActive',
      label: 'Active status',
      type: 'select',
      options: [
        { label: 'Active', value: 'true' },
        { label: 'Inactive', value: 'false' },
      ],
    },
    { field: 'createdAt', label: 'Created date', type: 'date' },
    { field: 'disabledAt', label: 'Disabled date', type: 'date' },
  ]
}
