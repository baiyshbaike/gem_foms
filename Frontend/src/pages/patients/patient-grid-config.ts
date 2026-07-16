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
  PatientGridRow,
  PatientGroup,
  Region,
} from '@/services/types/dialysis'

import { DataTableColumnHeader } from '@/components/data-table'
import { Badge } from '@/components/ui/badge'
import { formatDate, formatDateTime } from '@/lib/dialysis'

import PatientRowActions from './patient-row-actions.vue'

interface CreatePatientColumnsOptions {
  canDelete: boolean
  canUpdate: boolean
  disableActions: boolean
  onDelete: (patient: PatientGridRow) => void
  onEdit: (patient: PatientGridRow) => void
}

export const patientColumnLabels: Record<string, string> = {
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

export const patientDefaultColumnVisibility: VisibilityState = {
  address: false,
  address2: false,
  firstName: false,
  id: false,
  lastName: false,
  middleName: false,
  updatedAt: false,
}

export const patientDefaultColumnPinning: ColumnPinningState = {
  left: ['select', 'inn'],
  right: ['actions'],
}

export const patientDefaultSorting: SortingState = [
  { id: 'createdAt', desc: true },
]

export const patientGroupOptions: ServerDataGridGroupOption[] = [
  { field: 'regionName', label: 'Region' },
  { field: 'districtName', label: 'District' },
  { field: 'groupName', label: 'Patient group' },
  { field: 'gender', label: 'Gender' },
  { field: 'specialStatus', label: 'Special status' },
  { field: 'isActive', label: 'Active status' },
]

export const patientExportColumns: ServerDataGridExportColumn<PatientGridRow>[] = [
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

export function createPatientColumns(options: CreatePatientColumnsOptions): ColumnDef<PatientGridRow>[] {
  const columns: ColumnDef<PatientGridRow>[] = [
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

  if (options.canUpdate || options.canDelete) {
    columns.push({
      id: 'actions',
      header: () => h('span', { class: 'block text-right' }, 'Actions'),
      cell: ({ row }) => h(PatientRowActions, {
        patient: row.original,
        canUpdate: options.canUpdate,
        canDelete: options.canDelete,
        disabled: options.disableActions,
        onEdit: options.onEdit,
        onDelete: options.onDelete,
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

  return columns
}

export function createPatientFilterFields(
  regions: Region[],
  groups: PatientGroup[],
): ServerDataGridFilterField[] {
  const districts = regions.flatMap(region => region.districts
    .filter(district => district.isActive)
    .map(district => ({
      label: `${district.name} (${region.name})`,
      value: String(district.id),
    })))

  return [
    { field: 'fullName', label: 'Full name', type: 'text' },
    { field: 'inn', label: 'INN', type: 'text' },
    { field: 'phone', label: 'Phone', type: 'text' },
    { field: 'birthDate', label: 'Birth date', type: 'date' },
    { field: 'createdAt', label: 'Registered date', type: 'date' },
    {
      field: 'regionId',
      label: 'Region',
      type: 'select',
      options: regions.map(region => ({ label: region.name, value: String(region.id) })),
    },
    {
      field: 'districtId',
      label: 'District',
      type: 'select',
      options: districts,
    },
    {
      field: 'groupId',
      label: 'Group',
      type: 'select',
      options: groups.map(group => ({ label: group.name, value: String(group.id) })),
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
  ]
}
