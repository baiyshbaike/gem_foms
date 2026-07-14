<script setup lang="ts">
import type { DxDataGridTypes } from 'devextreme-vue/data-grid'

import axios from 'axios'
import {
  DxButton,
  DxColumn,
  DxColumnChooser,
  DxColumnFixing,
  DxCustomRule,
  DxDataGrid,
  DxEditing,
  DxEditingTexts,
  DxExport,
  DxFilterPanel,
  DxFilterRow,
  DxForm,
  DxFormItem,
  DxGrouping,
  DxGroupPanel,
  DxHeaderFilter,
  DxLoadPanel,
  DxLookup,
  DxPager,
  DxPaging,
  DxPatternRule,
  DxPopup,
  DxRangeRule,
  DxRequiredRule,
  DxScrolling,
  DxSearchPanel,
  DxSelection,
  DxSorting,
  DxStateStoring,
  DxStringLengthRule,
  DxSummary,
  DxToolbar,
  DxItem as DxToolbarItem,
  DxTotalItem,
} from 'devextreme-vue/data-grid'
import CustomStore from 'devextreme/data/custom_store'
import { toast } from 'vue-sonner'
import { ZodError } from 'zod'

import type {
  Patient,
  PatientGridExportRequest,
  PatientGridRow,
  PatientGroup,
  Region,
} from '@/services/types/dialysis'

import { BasicPage } from '@/components/global-layout'
import { patientApi, regionApi } from '@/services/api/dialysis.api'
import { useAuthStore } from '@/stores/auth'

import type { PatientEditValues } from './patient-grid'

import {
  serializePatientGridLoadOptions,
  toCreatePatientRequest,
  toUpdatePatientRequest,
} from './patient-grid'

type GridInitializedEvent = DxDataGridTypes.InitializedEvent<PatientGridRow, number>
type GridExportingEvent = DxDataGridTypes.ExportingEvent<PatientGridRow, number>
type GridDataErrorEvent = DxDataGridTypes.DataErrorOccurredEvent<PatientGridRow, number>
type GridInitNewRowEvent = DxDataGridTypes.InitNewRowEvent<PatientGridRow, number>
type GridEditorPreparingEvent = DxDataGridTypes.EditorPreparingEvent<PatientGridRow, number>
type GridState = Record<string, unknown>

interface DistrictLookupOptions {
  data?: PatientEditValues
}

interface ValidationRuleOptions {
  data?: PatientEditValues
  value?: unknown
}

interface ApiProblemDetails {
  detail?: string
  errors?: Record<string, string[]>
  title?: string
}

const authStore = useAuthStore()
const gridInstance = shallowRef<GridInitializedEvent['component'] | null>(null)
const regions = ref<Region[]>([])
const groups = ref<PatientGroup[]>([])
const loadingLookups = ref(false)
const exportEndpointActive = ref(false)
const exportSelectedIds = ref<number[]>([])

const canCreate = computed(() => authStore.hasPermission('patient.create'))
const canUpdate = computed(() => authStore.hasPermission('patient.update'))
const canDelete = computed(() => authStore.hasPermission('patient.delete'))
const canExport = computed(() => authStore.hasPermission('patient.export'))
const canManageRows = computed(() => canUpdate.value || canDelete.value)
const lookupsReady = computed(() => regions.value.length > 0 && groups.value.length > 0)
const canAddRow = computed(() => canCreate.value && lookupsReady.value && !loadingLookups.value)
const canEditRow = computed(() => canUpdate.value && lookupsReady.value && !loadingLookups.value)
const gridStorageKey = computed(() => `dialysis:patient-grid:${authStore.user?.id ?? 'anonymous'}`)
const districts = computed(() => regions.value.flatMap(region => region.districts.filter(item => item.isActive)))

const genderOptions = [
  { id: 1, name: 'Male' },
  { id: 2, name: 'Female' },
]

const innPattern = /^\d{14}$/
const formColumnsByScreen = { lg: 2, md: 2, sm: 1, xs: 1 }
const innEditorOptions = {
  inputAttr: { inputmode: 'numeric' },
  maxLength: 14,
}
const nameEditorOptions = { maxLength: 100 }
const phoneEditorOptions = { maxLength: 50 }
const birthDateEditorOptions = {
  displayFormat: 'dd.MM.yyyy',
  max: new Date(),
  useMaskBehavior: true,
}
const addressEditorOptions = {
  height: 72,
  maxLength: 500,
}
const refreshButtonOptions = {
  hint: 'Refresh patients',
  icon: 'refresh',
  onClick: refreshGrid,
  text: 'Refresh',
}
const resetButtonOptions = {
  hint: 'Reset grid layout',
  icon: 'revert',
  onClick: resetGridLayout,
  text: 'Reset layout',
}

const patientStore = new CustomStore<PatientGridRow, number>({
  key: 'id',
  loadMode: 'processed',
  async load(loadOptions) {
    const request = serializePatientGridLoadOptions(loadOptions)

    if (exportEndpointActive.value) {
      const exportRequest: PatientGridExportRequest = {
        ...request,
        selectedIds: exportSelectedIds.value,
      }
      return patientApi.gridExport(exportRequest)
    }

    return patientApi.gridQuery(request)
  },
  async byKey(key) {
    const patient = await patientApi.get(Number(key))
    return toGridRow(patient)
  },
  async insert(values) {
    try {
      const created = await patientApi.create(toCreatePatientRequest(values))
      return toGridRow(created)
    }
    catch (error) {
      throw toPatientStoreError(error, 'Patient could not be created')
    }
  },
  async update(key, values) {
    try {
      const current = await patientApi.get(Number(key))
      const updated = await patientApi.update(
        Number(key),
        toUpdatePatientRequest({ ...current, ...values }),
      )
      return toGridRow(updated)
    }
    catch (error) {
      throw toPatientStoreError(error, 'Patient could not be updated')
    }
  },
  async remove(key) {
    try {
      await patientApi.delete(Number(key))
    }
    catch (error) {
      throw toPatientStoreError(error, 'Patient could not be deleted')
    }
  },
})

function toGridRow(patient: Patient): PatientGridRow {
  const region = regions.value.find(item => item.id === patient.regionId)
  const district = region?.districts.find(item => item.id === patient.districtId)

  return {
    ...patient,
    districtName: district?.name ?? String(patient.districtId),
    fullName: `${patient.lastName} ${patient.firstName} ${patient.middleName}`.trim(),
    regionName: region?.name ?? String(patient.regionId),
  }
}

function districtLookupDataSource(options?: DistrictLookupOptions) {
  const regionId = Number(options?.data?.regionId)

  return {
    filter: ['regionId', '=', Number.isInteger(regionId) && regionId > 0 ? regionId : -1],
    store: districts.value,
  }
}

function setRegionValue(newData: PatientEditValues, value: number | null) {
  const regionId = Number(value)
  newData.regionId = Number.isInteger(regionId) && regionId > 0 ? regionId : null
  newData.districtId = null
}

function validateDistrictRegion(options: ValidationRuleOptions): boolean {
  const districtId = Number(options.value)
  if (!Number.isInteger(districtId) || districtId <= 0) {
    return true
  }

  const regionId = Number(options.data?.regionId)
  return districts.value.some(district => district.id === districtId && district.regionId === regionId)
}

function onInitNewRow(event: GridInitNewRowEvent) {
  const newGroupId = groups.value.find(group => group.code.toLowerCase() === 'new')?.id
    ?? groups.value[0]?.id
    ?? 1

  Object.assign(event.data, {
    districtId: null,
    gender: 1,
    groupId: newGroupId,
    isActive: true,
    regionId: null,
    specialStatus: false,
  })
}

function onEditorPreparing(event: GridEditorPreparingEvent) {
  if (event.parentType !== 'dataRow') {
    return
  }

  if (event.row?.isNewRow && (event.dataField === 'groupId' || event.dataField === 'isActive')) {
    event.editorOptions.disabled = true
  }

  if (event.dataField === 'districtId') {
    event.editorOptions.disabled = !event.row?.data.regionId
  }
}

function onGridInitialized(event: GridInitializedEvent) {
  gridInstance.value = event.component ?? null
}

function onGridDataError(event: GridDataErrorEvent) {
  toast.error(event.error?.message || 'Patient operation failed')
}

function onRowInserted() {
  toast.success('Patient created')
}

function onRowUpdated() {
  toast.success('Patient updated')
}

function onRowRemoved() {
  toast.success('Patient deleted')
}

async function refreshGrid() {
  await gridInstance.value?.refresh()
}

function resetGridLayout() {
  localStorage.removeItem(gridStorageKey.value)
  gridInstance.value?.state(null)
  toast.success('Grid layout reset')
}

function loadGridState(): GridState | null {
  const stored = localStorage.getItem(gridStorageKey.value)
  if (!stored) {
    return null
  }

  try {
    return JSON.parse(stored) as GridState
  }
  catch {
    localStorage.removeItem(gridStorageKey.value)
    return null
  }
}

function saveGridState(state: GridState) {
  const persisted = { ...state }
  delete persisted.selectedRowKeys
  delete persisted.selectionFilter
  delete persisted.focusedRowKey
  localStorage.setItem(gridStorageKey.value, JSON.stringify(persisted))
}

function patientPopupWidth() {
  return Math.min(window.innerWidth - 24, 780)
}

function patientPopupMaxHeight() {
  return Math.max(200, window.innerHeight - 24)
}

function toPatientStoreError(error: unknown, fallback: string): Error {
  if (error instanceof ZodError) {
    return new Error(error.issues[0]?.message ?? fallback)
  }

  if (axios.isAxiosError<ApiProblemDetails>(error)) {
    const problem = error.response?.data
    const validationMessage = problem?.errors
      ? Object.values(problem.errors).flat()[0]
      : undefined

    return new Error(validationMessage ?? problem?.detail ?? problem?.title ?? fallback)
  }

  return error instanceof Error ? error : new Error(fallback)
}

async function onExporting(event: GridExportingEvent) {
  event.cancel = true

  if (!canExport.value) {
    toast.error('You do not have permission to export patients')
    return
  }

  const selectedKeys = await Promise.resolve(event.component.getSelectedRowKeys())
  exportSelectedIds.value = event.selectedRowsOnly
    ? selectedKeys.map(Number)
    : []

  if (event.selectedRowsOnly && exportSelectedIds.value.length === 0) {
    toast.error('Select at least one patient to export')
    return
  }

  exportEndpointActive.value = true

  try {
    const [excelExporter, excelJs, fileSaver] = await Promise.all([
      import('devextreme/excel_exporter'),
      import('devextreme-exceljs-fork'),
      import('file-saver'),
    ])
    const { exportDataGrid } = excelExporter
    const { Workbook } = excelJs
    const { saveAs } = fileSaver
    const workbook = new Workbook()
    const worksheet = workbook.addWorksheet('Patients')

    await exportDataGrid({
      autoFilterEnabled: true,
      component: event.component,
      selectedRowsOnly: event.selectedRowsOnly,
      worksheet,
    })

    const buffer = await workbook.xlsx.writeBuffer()
    saveAs(
      new Blob([buffer as BlobPart], { type: 'application/octet-stream' }),
      `patients-${new Date().toISOString().slice(0, 10)}.xlsx`,
    )
    toast.success('Patient export created')
  }
  catch (error) {
    toast.error(toPatientStoreError(error, 'Patient export could not be created').message)
  }
  finally {
    exportEndpointActive.value = false
    exportSelectedIds.value = []
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
    toast.error(toPatientStoreError(error, 'Patient lookups could not be loaded').message)
  }
  finally {
    loadingLookups.value = false
  }
}

onMounted(loadLookups)
</script>

<template>
  <BasicPage
    title="Patients"
    description="Global patient registry"
    sticky
  >
    <div class="patient-grid-shell">
      <DxDataGrid
        :data-source="patientStore"
        :show-borders="true"
        :show-row-lines="true"
        :row-alternation-enabled="true"
        :hover-state-enabled="true"
        :focused-row-enabled="true"
        :repaint-changes-only="true"
        :allow-column-reordering="true"
        :allow-column-resizing="true"
        :column-auto-width="false"
        :column-width="140"
        :column-min-width="90"
        :word-wrap-enabled="false"
        :sync-lookup-filter-values="false"
        :remote-operations="{
          paging: true,
          filtering: true,
          sorting: true,
          grouping: true,
          groupPaging: true,
          summary: true,
        }"
        column-resizing-mode="widget"
        height="100%"
        no-data-text="No patients match the current filters"
        @initialized="onGridInitialized"
        @init-new-row="onInitNewRow"
        @editor-preparing="onEditorPreparing"
        @data-error-occurred="onGridDataError"
        @row-inserted="onRowInserted"
        @row-updated="onRowUpdated"
        @row-removed="onRowRemoved"
        @exporting="onExporting"
      >
        <DxEditing
          mode="popup"
          :allow-adding="canAddRow"
          :allow-updating="canEditRow"
          :allow-deleting="canDelete"
          :confirm-delete="true"
          :use-icons="true"
        >
          <DxEditingTexts
            add-row="New patient"
            cancel-row-changes="Cancel"
            confirm-delete-message="Delete this patient?"
            confirm-delete-title="Delete patient"
            delete-row="Delete"
            edit-row="Edit"
            save-row-changes="Save"
          />
          <DxPopup
            title="Patient details"
            :show-title="true"
            :width="patientPopupWidth"
            height="auto"
            :max-height="patientPopupMaxHeight"
          />
          <DxForm
            :col-count="2"
            :col-count-by-screen="formColumnsByScreen"
            label-location="top"
            :show-colon-after-label="false"
            :show-validation-summary="true"
          />
        </DxEditing>

        <DxToolbar>
          <DxToolbarItem name="groupPanel" location="before" locate-in-menu="auto" />
          <DxToolbarItem
            name="addRowButton"
            location="after"
            locate-in-menu="auto"
            show-text="always"
            :visible="canCreate"
          />
          <DxToolbarItem name="columnChooserButton" location="after" locate-in-menu="auto" />
          <DxToolbarItem
            name="exportButton"
            location="after"
            locate-in-menu="auto"
            :visible="canExport"
          />
          <DxToolbarItem
            location="after"
            locate-in-menu="auto"
            show-text="inMenu"
            widget="dxButton"
            :options="resetButtonOptions"
          />
          <DxToolbarItem
            location="after"
            locate-in-menu="auto"
            show-text="inMenu"
            widget="dxButton"
            :options="refreshButtonOptions"
          />
          <DxToolbarItem name="searchPanel" location="after" />
        </DxToolbar>

        <DxLoadPanel :enabled="true" />
        <DxScrolling mode="standard" row-rendering-mode="virtual" />
        <DxPaging :page-size="25" />
        <DxPager
          :visible="true"
          :allowed-page-sizes="[10, 25, 50, 100]"
          :show-page-size-selector="true"
          :show-info="true"
          :show-navigation-buttons="true"
          display-mode="full"
        />
        <DxSorting mode="multiple" />
        <DxSelection mode="multiple" show-check-boxes-mode="always" />
        <DxSearchPanel
          :visible="true"
          :width="280"
          placeholder="Search patients..."
        />
        <DxFilterRow :visible="true" apply-filter="auto" />
        <DxHeaderFilter :visible="true" :allow-search="true" />
        <DxFilterPanel :visible="true" />
        <DxGroupPanel
          :visible="true"
          empty-panel-text="Drag a column here to group"
        />
        <DxGrouping :auto-expand-all="false" />
        <DxColumnChooser :enabled="true" mode="select" />
        <DxColumnFixing :enabled="true" />
        <DxStateStoring
          :enabled="true"
          type="custom"
          :custom-load="loadGridState"
          :custom-save="saveGridState"
        />
        <DxExport
          :enabled="canExport"
          :allow-export-selected-data="true"
          :formats="['xlsx']"
        />

        <DxColumn
          data-field="id"
          caption="ID"
          data-type="number"
          :width="90"
          :allow-editing="false"
          fixed-position="left"
          :fixed="true"
        >
          <DxFormItem :visible="false" />
        </DxColumn>
        <DxColumn
          data-field="inn"
          caption="INN"
          :width="155"
          fixed-position="left"
          :fixed="true"
        >
          <DxFormItem :col-span="2" :editor-options="innEditorOptions" />
          <DxRequiredRule message="INN is required" />
          <DxPatternRule :pattern="innPattern" message="INN must contain exactly 14 digits" />
          <DxStringLengthRule :min="14" :max="14" message="INN must contain exactly 14 digits" />
        </DxColumn>
        <DxColumn data-field="lastName" caption="Last name" :visible="false">
          <DxFormItem :editor-options="nameEditorOptions" />
          <DxRequiredRule message="Last name is required" />
          <DxStringLengthRule :max="100" message="Last name must be at most 100 characters" />
        </DxColumn>
        <DxColumn data-field="firstName" caption="First name" :visible="false">
          <DxFormItem :editor-options="nameEditorOptions" />
          <DxRequiredRule message="First name is required" />
          <DxStringLengthRule :max="100" message="First name must be at most 100 characters" />
        </DxColumn>
        <DxColumn data-field="middleName" caption="Middle name" :visible="false">
          <DxFormItem :editor-options="nameEditorOptions" />
          <DxRequiredRule message="Middle name is required" />
          <DxStringLengthRule :max="100" message="Middle name must be at most 100 characters" />
        </DxColumn>
        <DxColumn
          data-field="fullName"
          caption="Full name"
          :width="280"
          :allow-editing="false"
          cell-template="fullNameCell"
        >
          <DxFormItem :visible="false" />
        </DxColumn>
        <DxColumn data-field="gender" caption="Gender" :width="110">
          <DxLookup :data-source="genderOptions" value-expr="id" display-expr="name" />
          <DxFormItem />
          <DxRequiredRule message="Gender is required" />
          <DxRangeRule :min="1" :max="2" message="Select a valid gender" />
        </DxColumn>
        <DxColumn
          data-field="birthDate"
          caption="Birth date"
          data-type="date"
          format="dd.MM.yyyy"
          :width="125"
        >
          <DxFormItem :editor-options="birthDateEditorOptions" />
          <DxRequiredRule message="Birth date is required" />
        </DxColumn>
        <DxColumn data-field="phone" caption="Phone" :width="160">
          <DxFormItem :editor-options="phoneEditorOptions" />
          <DxRequiredRule message="Phone is required" />
          <DxStringLengthRule :max="50" message="Phone must be at most 50 characters" />
        </DxColumn>
        <DxColumn
          data-field="regionId"
          caption="Region"
          data-type="number"
          :visible="false"
          :set-cell-value="setRegionValue"
        >
          <DxLookup :data-source="regions" value-expr="id" display-expr="name" />
          <DxFormItem />
          <DxRequiredRule message="Region is required" />
        </DxColumn>
        <DxColumn
          data-field="districtId"
          caption="District"
          data-type="number"
          :visible="false"
        >
          <DxLookup :data-source="districtLookupDataSource" value-expr="id" display-expr="name" />
          <DxFormItem />
          <DxRequiredRule message="District is required" />
          <DxCustomRule
            :reevaluate="true"
            :validation-callback="validateDistrictRegion"
            message="District must belong to the selected region"
          />
        </DxColumn>
        <DxColumn
          data-field="regionName"
          caption="Region"
          :width="180"
          :allow-editing="false"
        >
          <DxFormItem :visible="false" />
        </DxColumn>
        <DxColumn
          data-field="districtName"
          caption="District"
          :width="180"
          :allow-editing="false"
        >
          <DxFormItem :visible="false" />
        </DxColumn>
        <DxColumn
          data-field="groupId"
          caption="Group"
          data-type="number"
          :visible="false"
        >
          <DxLookup :data-source="groups" value-expr="id" display-expr="name" />
          <DxFormItem />
          <DxRequiredRule message="Group is required" />
        </DxColumn>
        <DxColumn
          data-field="groupName"
          caption="Group"
          :width="135"
          :allow-editing="false"
        >
          <DxFormItem :visible="false" />
        </DxColumn>
        <DxColumn
          data-field="specialStatus"
          caption="Special"
          data-type="boolean"
          :width="105"
          :show-editor-always="false"
          true-text="Yes"
          false-text="No"
        >
          <DxFormItem />
        </DxColumn>
        <DxColumn
          data-field="isActive"
          caption="Active"
          data-type="boolean"
          :width="105"
          :show-editor-always="false"
          true-text="Active"
          false-text="Inactive"
        >
          <DxFormItem />
        </DxColumn>
        <DxColumn
          data-field="createdAt"
          caption="Registered"
          data-type="datetime"
          format="dd.MM.yyyy HH:mm"
          sort-order="desc"
          :allow-editing="false"
          :width="165"
        >
          <DxFormItem :visible="false" />
        </DxColumn>
        <DxColumn
          data-field="updatedAt"
          caption="Updated"
          data-type="datetime"
          format="dd.MM.yyyy HH:mm"
          :visible="false"
          :allow-editing="false"
          :width="165"
        >
          <DxFormItem :visible="false" />
        </DxColumn>
        <DxColumn data-field="address" caption="Address" :visible="false" :width="280">
          <DxFormItem editor-type="dxTextArea" :editor-options="addressEditorOptions" />
          <DxRequiredRule message="Address is required" />
          <DxStringLengthRule :max="500" message="Address must be at most 500 characters" />
        </DxColumn>
        <DxColumn data-field="address2" caption="Address 2" :visible="false" :width="280">
          <DxFormItem editor-type="dxTextArea" :editor-options="addressEditorOptions" />
          <DxRequiredRule message="Address 2 is required" />
          <DxStringLengthRule :max="500" message="Address 2 must be at most 500 characters" />
        </DxColumn>
        <DxColumn
          v-if="canManageRows"
          type="buttons"
          caption="Actions"
          :width="96"
          :fixed="true"
          fixed-position="right"
          :allow-exporting="false"
          :show-in-column-chooser="false"
        >
          <DxButton v-if="canUpdate" name="edit" />
          <DxButton v-if="canDelete" name="delete" />
        </DxColumn>

        <DxSummary>
          <DxTotalItem
            column="id"
            summary-type="count"
            display-format="{0} patients"
          />
        </DxSummary>

        <template #fullNameCell="{ data }">
          <div class="patient-name-cell">
            <span class="patient-name">{{ data.data.fullName }}</span>
            <span class="patient-address">{{ data.data.address }}</span>
          </div>
        </template>
      </DxDataGrid>
    </div>
  </BasicPage>
</template>

<style scoped>
.patient-grid-shell {
  height: calc(100dvh - 154px);
  min-height: 560px;
  width: 100%;
  overflow: hidden;
  border: 1px solid var(--border);
  border-radius: 6px;
}

.patient-name-cell {
  display: flex;
  min-width: 0;
  flex-direction: column;
  padding-block: 4px;
}

.patient-name,
.patient-address {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.patient-name {
  color: var(--foreground);
  font-weight: 500;
}

.patient-address {
  color: var(--muted-foreground);
  font-size: 12px;
}

:deep(.dx-datagrid) {
  color: var(--foreground);
  font-family: inherit;
}

:deep(.dx-datagrid-headers),
:deep(.dx-datagrid-total-footer),
:deep(.dx-datagrid-pager) {
  background: color-mix(in oklab, var(--muted) 42%, transparent);
}

:deep(.dx-datagrid-header-panel .dx-toolbar) {
  padding: 6px 10px;
}

:deep(.dx-datagrid-headers .dx-datagrid-table .dx-row > td) {
  color: var(--foreground);
  font-weight: 600;
}

:deep(.dx-datagrid-rowsview .dx-row-focused.dx-data-row > td) {
  background: var(--accent);
  color: var(--accent-foreground);
}

@media (max-width: 768px) {
  .patient-grid-shell {
    height: calc(100dvh - 178px);
    min-height: 500px;
  }
}
</style>
