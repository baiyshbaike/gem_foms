<script setup lang="ts">
import { PlusIcon, Trash2Icon } from '@lucide/vue'
import axios from 'axios'
import { toast } from 'vue-sonner'

import type {
  ServerDataGridExportConfig,
  ServerDataGridExposed,
  ServerDataGridQueryRequest,
  ServerDataGridQueryResult,
} from '@/components/server-data-grid'
import type {
  PatientGridExportRequest,
  PatientGridRow,
  PatientGroup,
  Region,
} from '@/services/types/dialysis'

import { BasicPage } from '@/components/global-layout'
import { ServerDataGrid } from '@/components/server-data-grid'
import { patientApi, regionApi } from '@/services/api/dialysis.api'
import { useAuthStore } from '@/stores/auth'

import PatientEditorPopup from './patient-editor-popup.vue'
import {
  createPatientColumns,
  createPatientFilterFields,
  patientColumnLabels,
  patientDefaultColumnPinning,
  patientDefaultColumnVisibility,
  patientDefaultSorting,
  patientExportColumns,
  patientGroupOptions,
} from './patient-grid-config'

interface PatientEditorComponent {
  openCreate: () => void
  openEdit: (patient: PatientGridRow) => void
}

interface ApiProblemDetails {
  detail?: string
  errors?: Record<string, string[]>
  title?: string
}

const authStore = useAuthStore()
const gridRef = ref<ServerDataGridExposed | null>(null)
const patientEditorRef = ref<PatientEditorComponent | null>(null)
const regions = ref<Region[]>([])
const groups = ref<PatientGroup[]>([])
const loadingLookups = ref(false)
const deleting = ref(false)
const deleteTarget = ref<PatientGridRow | null>(null)

const canCreate = computed(() => authStore.hasPermission('patient.create'))
const canUpdate = computed(() => authStore.hasPermission('patient.update'))
const canDelete = computed(() => authStore.hasPermission('patient.delete'))
const canExport = computed(() => authStore.hasPermission('patient.export'))
const lookupsReady = computed(() => regions.value.length > 0 && groups.value.length > 0)
const gridStorageKey = computed(() => `dialysis:patient-grid:${authStore.user?.id ?? 'anonymous'}`)

const columns = computed(() => createPatientColumns({
  canDelete: canDelete.value,
  canUpdate: canUpdate.value,
  disableActions: !lookupsReady.value || loadingLookups.value,
  onDelete: requestDeletePatient,
  onEdit: openEditPatient,
}))

const filterFields = computed(() => createPatientFilterFields(regions.value, groups.value))

const exportConfig = computed<ServerDataGridExportConfig<PatientGridRow> | undefined>(() => canExport.value
  ? {
      columns: patientExportColumns,
      fileName: () => `patients-${new Date().toISOString().slice(0, 10)}.xlsx`,
      load: loadPatientExport,
      sheetName: 'Patients',
    }
  : undefined)

onMounted(loadLookups)

function loadPatients(
  request: ServerDataGridQueryRequest,
): Promise<ServerDataGridQueryResult<PatientGridRow>> {
  return patientApi.gridQuery(request)
}

function loadPatientExport(
  request: ServerDataGridQueryRequest,
  selectedRowIds: string[],
): Promise<ServerDataGridQueryResult<PatientGridRow>> {
  const payload: PatientGridExportRequest = {
    ...request,
    selectedIds: selectedRowIds.map(Number),
  }
  return patientApi.gridExport(payload)
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
    toast.error(formatPatientError(error, 'Patient lookups could not be loaded'))
  }
  finally {
    loadingLookups.value = false
  }
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
    gridRef.value?.removeSelection(patient.id)
    deleteTarget.value = null
    toast.success('Patient deleted')
    gridRef.value?.refresh()
  }
  catch (error) {
    toast.error(formatPatientError(error, 'Patient could not be deleted'))
  }
  finally {
    deleting.value = false
  }
}

function onPatientSaved(action: 'created' | 'updated') {
  toast.success(action === 'created' ? 'Patient created' : 'Patient updated')
  gridRef.value?.refresh()
}

function formatPatientError(error: unknown, fallback: string): string {
  if (axios.isAxiosError<ApiProblemDetails>(error)) {
    const problem = error.response?.data
    const validationMessage = problem?.errors
      ? Object.values(problem.errors).flat()[0]
      : undefined
    return validationMessage ?? problem?.detail ?? problem?.title ?? fallback
  }
  return error instanceof Error ? error.message : fallback
}
</script>

<template>
  <BasicPage title="Patients" description="Global patient registry" sticky>
    <ServerDataGrid
      ref="gridRef"
      :columns="columns"
      :column-labels="patientColumnLabels"
      :default-column-pinning="patientDefaultColumnPinning"
      :default-column-visibility="patientDefaultColumnVisibility"
      :default-sorting="patientDefaultSorting"
      :export-config="exportConfig"
      :filter-fields="filterFields"
      :format-error="formatPatientError"
      :get-row-id="patient => patient.id"
      :group-options="patientGroupOptions"
      :load="loadPatients"
      :storage-key="gridStorageKey"
      :state-version="2"
      empty-title="No patients found"
      filter-description="Combine filters to narrow the patient registry on the server."
      filter-title="Patient filters"
      item-label="patients"
      load-error-message="Patients could not be loaded"
      loading-label="Loading patients"
      search-placeholder="Search patients..."
    >
      <template #toolbar-actions>
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
      </template>
    </ServerDataGrid>

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
