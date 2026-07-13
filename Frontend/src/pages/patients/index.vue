<script setup lang="ts">
import {
  ArrowDownIcon,
  ArrowUpDownIcon,
  ArrowUpIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ChevronsLeftIcon,
  ChevronsRightIcon,
  EditIcon,
  FilterXIcon,
  PlusIcon,
  RefreshCwIcon,
  SaveIcon,
  SearchIcon,
  Trash2Icon,
  XIcon,
} from '@lucide/vue'
import { toast } from 'vue-sonner'

import type { CreatePatientRequest, Patient, PatientGender, UpdatePatientRequest } from '@/services/types/dialysis'

import { BasicPage } from '@/components/global-layout'
import {
  formatDate,
  genderOptions,
  patientGroupOptions,
} from '@/lib/dialysis'
import { patientApi } from '@/services/api/dialysis.api'

type SortDirection = 'asc' | 'desc'
type PatientSortKey
  = 'id'
    | 'inn'
    | 'fullName'
    | 'birthDate'
    | 'gender'
    | 'phone'
    | 'regionId'
    | 'districtId'
    | 'groupName'
    | 'createdAt'
    | 'isActive'
    | 'specialStatus'

const loading = ref(false)
const saving = ref(false)
const deletingId = ref<number | null>(null)
const search = ref('')
const patients = ref<Patient[]>([])
const editingId = ref<number | null>(null)
const isFormOpen = ref(false)

const page = ref(1)
const pageSize = ref(10)
const sortKey = ref<PatientSortKey>('createdAt')
const sortDirection = ref<SortDirection>('desc')

const pageSizeOptions = [10, 25, 50, 100]

const sortOptions: { value: PatientSortKey, label: string }[] = [
  { value: 'createdAt', label: 'Registration date' },
  { value: 'id', label: 'ID' },
  { value: 'inn', label: 'INN' },
  { value: 'fullName', label: 'Full name' },
  { value: 'birthDate', label: 'Birth date' },
  { value: 'gender', label: 'Gender' },
  { value: 'phone', label: 'Phone' },
  { value: 'regionId', label: 'Region' },
  { value: 'districtId', label: 'District' },
  { value: 'groupName', label: 'Group' },
  { value: 'isActive', label: 'Status' },
  { value: 'specialStatus', label: 'Special status' },
]

const filters = reactive({
  inn: '',
  firstName: '',
  lastName: '',
  middleName: '',
  phone: '',
  regionId: '',
  districtId: '',
  groupId: '',
  gender: '',
  status: '',
  specialStatus: '',
  birthDateFrom: '',
  birthDateTo: '',
  createdAtFrom: '',
  createdAtTo: '',
})

const form = reactive({
  inn: '',
  firstName: '',
  lastName: '',
  middleName: '',
  birthDate: '',
  gender: '1',
  address: '',
  address2: '',
  phone: '',
  districtId: '',
  regionId: '',
  groupId: '1',
  specialStatus: false,
  isActive: true,
})

const formTitle = computed(() => editingId.value ? `Edit patient #${editingId.value}` : 'Create patient')

const activeFilterCount = computed(() => {
  const advancedFilters = Object.values(filters).filter(value => String(value).trim()).length
  return advancedFilters + (search.value.trim() ? 1 : 0)
})

const filteredPatients = computed(() => {
  const quickSearch = normalize(search.value)

  return patients.value.filter((patient) => {
    if (quickSearch && !getSearchText(patient).includes(quickSearch)) {
      return false
    }

    if (filters.inn && !normalize(patient.inn).includes(normalize(filters.inn))) {
      return false
    }

    if (filters.firstName && !normalize(patient.firstName).includes(normalize(filters.firstName))) {
      return false
    }

    if (filters.lastName && !normalize(patient.lastName).includes(normalize(filters.lastName))) {
      return false
    }

    if (filters.middleName && !normalize(patient.middleName).includes(normalize(filters.middleName))) {
      return false
    }

    if (filters.phone && !normalize(patient.phone).includes(normalize(filters.phone))) {
      return false
    }

    if (filters.regionId && patient.regionId !== Number(filters.regionId)) {
      return false
    }

    if (filters.districtId && patient.districtId !== Number(filters.districtId)) {
      return false
    }

    if (filters.groupId && patient.groupId !== Number(filters.groupId)) {
      return false
    }

    if (filters.gender && patient.gender !== Number(filters.gender)) {
      return false
    }

    if (filters.status === 'active' && !patient.isActive) {
      return false
    }

    if (filters.status === 'inactive' && patient.isActive) {
      return false
    }

    if (filters.specialStatus === 'yes' && !patient.specialStatus) {
      return false
    }

    if (filters.specialStatus === 'no' && patient.specialStatus) {
      return false
    }

    if (!isDateInRange(patient.birthDate, filters.birthDateFrom, filters.birthDateTo)) {
      return false
    }

    if (!isDateInRange(patient.createdAt, filters.createdAtFrom, filters.createdAtTo)) {
      return false
    }

    return true
  })
})

const sortedPatients = computed(() => {
  const direction = sortDirection.value === 'asc' ? 1 : -1

  return [...filteredPatients.value].sort((left, right) => {
    const leftValue = getSortValue(left, sortKey.value)
    const rightValue = getSortValue(right, sortKey.value)

    if (typeof leftValue === 'number' && typeof rightValue === 'number') {
      return (leftValue - rightValue) * direction
    }

    return String(leftValue).localeCompare(String(rightValue), undefined, { sensitivity: 'base' }) * direction
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(sortedPatients.value.length / pageSize.value)))

const pagedPatients = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return sortedPatients.value.slice(start, start + pageSize.value)
})

const rowStart = computed(() => sortedPatients.value.length === 0 ? 0 : (page.value - 1) * pageSize.value + 1)
const rowEnd = computed(() => Math.min(page.value * pageSize.value, sortedPatients.value.length))

watch(
  () => [
    search.value,
    filters.inn,
    filters.firstName,
    filters.lastName,
    filters.middleName,
    filters.phone,
    filters.regionId,
    filters.districtId,
    filters.groupId,
    filters.gender,
    filters.status,
    filters.specialStatus,
    filters.birthDateFrom,
    filters.birthDateTo,
    filters.createdAtFrom,
    filters.createdAtTo,
    pageSize.value,
    sortKey.value,
    sortDirection.value,
  ],
  () => {
    page.value = 1
  },
)

watch(totalPages, (value) => {
  if (page.value > value) {
    page.value = value
  }
})

function openCreateModal() {
  resetForm()
  isFormOpen.value = true
}

function resetForm() {
  editingId.value = null
  Object.assign(form, {
    inn: '',
    firstName: '',
    lastName: '',
    middleName: '',
    birthDate: '',
    gender: '1',
    address: '',
    address2: '',
    phone: '',
    districtId: '',
    regionId: '',
    groupId: '1',
    specialStatus: false,
    isActive: true,
  })
}

function editPatient(patient: Patient) {
  editingId.value = patient.id
  Object.assign(form, {
    inn: patient.inn,
    firstName: patient.firstName,
    lastName: patient.lastName,
    middleName: patient.middleName,
    birthDate: patient.birthDate.slice(0, 10),
    gender: String(patient.gender),
    address: patient.address,
    address2: patient.address2,
    phone: patient.phone,
    districtId: String(patient.districtId),
    regionId: String(patient.regionId),
    groupId: String(patient.groupId),
    specialStatus: patient.specialStatus,
    isActive: patient.isActive,
  })
  isFormOpen.value = true
}

function basePayload(): CreatePatientRequest {
  return {
    inn: form.inn.trim(),
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    middleName: form.middleName.trim(),
    birthDate: form.birthDate,
    gender: Number(form.gender) as PatientGender,
    address: form.address.trim(),
    address2: form.address2.trim(),
    phone: form.phone.trim(),
    districtId: Number(form.districtId),
    regionId: Number(form.regionId),
    specialStatus: form.specialStatus,
  }
}

async function loadPatients() {
  loading.value = true
  try {
    patients.value = await patientApi.list()
  }
  catch {
    toast.error('Patients could not be loaded')
  }
  finally {
    loading.value = false
  }
}

async function savePatient() {
  saving.value = true
  try {
    if (editingId.value) {
      const payload: UpdatePatientRequest = {
        ...basePayload(),
        groupId: Number(form.groupId),
        isActive: form.isActive,
      }
      await patientApi.update(editingId.value, payload)
      toast.success('Patient updated')
    }
    else {
      await patientApi.create(basePayload())
      toast.success('Patient created')
    }

    isFormOpen.value = false
    resetForm()
    await loadPatients()
  }
  catch {
    toast.error('Patient could not be saved')
  }
  finally {
    saving.value = false
  }
}

async function deletePatient(patient: Patient) {
  // eslint-disable-next-line no-alert
  if (!window.confirm(`Delete patient ${patient.lastName} ${patient.firstName}?`)) {
    return
  }

  deletingId.value = patient.id
  try {
    await patientApi.delete(patient.id)
    toast.success('Patient deleted')
    await loadPatients()
  }
  catch {
    toast.error('Patient could not be deleted')
  }
  finally {
    deletingId.value = null
  }
}

function clearFilters() {
  search.value = ''
  Object.assign(filters, {
    inn: '',
    firstName: '',
    lastName: '',
    middleName: '',
    phone: '',
    regionId: '',
    districtId: '',
    groupId: '',
    gender: '',
    status: '',
    specialStatus: '',
    birthDateFrom: '',
    birthDateTo: '',
    createdAtFrom: '',
    createdAtTo: '',
  })
}

function toggleSort(key: PatientSortKey) {
  if (sortKey.value === key) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
    return
  }

  sortKey.value = key
  sortDirection.value = key === 'birthDate' || key === 'createdAt' || key === 'id' ? 'desc' : 'asc'
}

function goToPage(nextPage: number) {
  page.value = Math.min(Math.max(nextPage, 1), totalPages.value)
}

function normalize(value: unknown) {
  return String(value ?? '').trim().toLowerCase()
}

function getFullName(patient: Patient) {
  return `${patient.lastName} ${patient.firstName} ${patient.middleName}`.trim()
}

function getGenderLabel(value: PatientGender) {
  return genderOptions.find(gender => gender.value === value)?.label ?? '-'
}

function getSearchText(patient: Patient) {
  return normalize([
    patient.id,
    patient.inn,
    getFullName(patient),
    patient.phone,
    patient.address,
    patient.address2,
    patient.regionId,
    patient.districtId,
    patient.groupName,
    getGenderLabel(patient.gender),
    patient.isActive ? 'active' : 'inactive',
    patient.specialStatus ? 'special' : '',
    formatDate(patient.birthDate),
    formatDate(patient.createdAt),
  ].join(' '))
}

function getDateTime(value: string | null | undefined) {
  if (!value) {
    return null
  }

  const parsed = new Date(value)
  if (!Number.isNaN(parsed.getTime())) {
    return parsed.getTime()
  }

  const dateOnly = new Date(`${value.slice(0, 10)}T00:00:00`)
  return Number.isNaN(dateOnly.getTime()) ? null : dateOnly.getTime()
}

function isDateInRange(value: string | null | undefined, from: string, to: string) {
  if (!from && !to) {
    return true
  }

  const current = getDateTime(value)
  if (current === null) {
    return false
  }

  const fromTime = from ? getDateTime(from) : null
  const toTime = to ? getDateTime(`${to}T23:59:59`) : null

  if (fromTime !== null && current < fromTime) {
    return false
  }

  if (toTime !== null && current > toTime) {
    return false
  }

  return true
}

function getSortValue(patient: Patient, key: PatientSortKey) {
  switch (key) {
    case 'id':
      return patient.id
    case 'inn':
      return patient.inn
    case 'fullName':
      return getFullName(patient)
    case 'birthDate':
      return getDateTime(patient.birthDate) ?? 0
    case 'gender':
      return getGenderLabel(patient.gender)
    case 'phone':
      return patient.phone
    case 'regionId':
      return patient.regionId
    case 'districtId':
      return patient.districtId
    case 'groupName':
      return patient.groupName
    case 'createdAt':
      return getDateTime(patient.createdAt) ?? 0
    case 'isActive':
      return patient.isActive ? 1 : 0
    case 'specialStatus':
      return patient.specialStatus ? 1 : 0
  }
}

onMounted(loadPatients)
</script>

<template>
  <BasicPage
    title="Patients"
    description="Global patient registry"
    sticky
  >
    <template #actions>
      <UiButton variant="outline" @click="loadPatients">
        <RefreshCwIcon class="mr-2 size-4" />
        Refresh
      </UiButton>
      <UiButton @click="openCreateModal">
        <PlusIcon class="mr-2 size-4" />
        New
      </UiButton>
    </template>

    <div>
      <UiCard>
        <UiCardHeader>
          <div class="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
            <div>
              <UiCardTitle>Patient list</UiCardTitle>
              <UiCardDescription>
                Search, filter, sort, and page through the global patient registry.
              </UiCardDescription>
            </div>
            <div class="flex flex-wrap gap-2 text-sm text-muted-foreground">
              <UiBadge variant="secondary">
                {{ patients.length }} total
              </UiBadge>
              <UiBadge variant="secondary">
                {{ sortedPatients.length }} filtered
              </UiBadge>
              <UiBadge v-if="activeFilterCount" variant="default">
                {{ activeFilterCount }} active filters
              </UiBadge>
            </div>
          </div>
        </UiCardHeader>
        <UiCardContent class="space-y-4">
          <div class="grid gap-3 xl:grid-cols-[minmax(280px,1fr)_170px_130px_auto]">
            <div class="relative">
              <SearchIcon class="absolute left-3 top-2.5 size-4 text-muted-foreground" />
              <UiInput
                v-model="search"
                class="pl-9"
                placeholder="Search ID, INN, name, phone, address, group"
              />
            </div>
            <select
              v-model="sortKey"
              class="h-9 rounded-md border bg-background px-3 text-sm"
            >
              <option
                v-for="option in sortOptions"
                :key="option.value"
                :value="option.value"
              >
                Sort: {{ option.label }}
              </option>
            </select>
            <select
              v-model="sortDirection"
              class="h-9 rounded-md border bg-background px-3 text-sm"
            >
              <option value="asc">
                Asc
              </option>
              <option value="desc">
                Desc
              </option>
            </select>
            <UiButton
              variant="outline"
              :disabled="activeFilterCount === 0"
              @click="clearFilters"
            >
              <FilterXIcon class="mr-2 size-4" />
              Clear
            </UiButton>
          </div>

          <div class="grid gap-3 rounded-md border bg-muted/20 p-3 sm:grid-cols-2 lg:grid-cols-4 2xl:grid-cols-7">
            <div class="grid gap-1">
              <UiLabel for="filterInn">
                INN
              </UiLabel>
              <UiInput id="filterInn" v-model="filters.inn" placeholder="INN" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterLastName">
                Last name
              </UiLabel>
              <UiInput id="filterLastName" v-model="filters.lastName" placeholder="Last name" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterFirstName">
                First name
              </UiLabel>
              <UiInput id="filterFirstName" v-model="filters.firstName" placeholder="First name" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterMiddleName">
                Middle name
              </UiLabel>
              <UiInput id="filterMiddleName" v-model="filters.middleName" placeholder="Middle name" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterPhone">
                Phone
              </UiLabel>
              <UiInput id="filterPhone" v-model="filters.phone" placeholder="Phone" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterRegion">
                Region
              </UiLabel>
              <UiInput id="filterRegion" v-model="filters.regionId" type="number" min="1" placeholder="ID" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterDistrict">
                District
              </UiLabel>
              <UiInput id="filterDistrict" v-model="filters.districtId" type="number" min="1" placeholder="ID" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterGroup">
                Group
              </UiLabel>
              <select
                id="filterGroup"
                v-model="filters.groupId"
                class="h-9 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">
                  All groups
                </option>
                <option
                  v-for="group in patientGroupOptions"
                  :key="group.value"
                  :value="group.value"
                >
                  {{ group.label }}
                </option>
              </select>
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterGender">
                Gender
              </UiLabel>
              <select
                id="filterGender"
                v-model="filters.gender"
                class="h-9 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">
                  All genders
                </option>
                <option
                  v-for="gender in genderOptions"
                  :key="gender.value"
                  :value="gender.value"
                >
                  {{ gender.label }}
                </option>
              </select>
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterStatus">
                Status
              </UiLabel>
              <select
                id="filterStatus"
                v-model="filters.status"
                class="h-9 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">
                  All statuses
                </option>
                <option value="active">
                  Active
                </option>
                <option value="inactive">
                  Inactive
                </option>
              </select>
            </div>
            <div class="grid gap-1">
              <UiLabel for="filterSpecialStatus">
                Special
              </UiLabel>
              <select
                id="filterSpecialStatus"
                v-model="filters.specialStatus"
                class="h-9 rounded-md border bg-background px-3 text-sm"
              >
                <option value="">
                  All
                </option>
                <option value="yes">
                  Special only
                </option>
                <option value="no">
                  Not special
                </option>
              </select>
            </div>
            <div class="grid gap-1">
              <UiLabel for="birthDateFrom">
                Birth from
              </UiLabel>
              <UiInput id="birthDateFrom" v-model="filters.birthDateFrom" type="date" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="birthDateTo">
                Birth to
              </UiLabel>
              <UiInput id="birthDateTo" v-model="filters.birthDateTo" type="date" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="createdAtFrom">
                Registered from
              </UiLabel>
              <UiInput id="createdAtFrom" v-model="filters.createdAtFrom" type="date" />
            </div>
            <div class="grid gap-1">
              <UiLabel for="createdAtTo">
                Registered to
              </UiLabel>
              <UiInput id="createdAtTo" v-model="filters.createdAtTo" type="date" />
            </div>
          </div>

          <div class="overflow-x-auto rounded-md border">
            <table class="w-full min-w-[1280px] text-sm">
              <thead class="bg-muted/60 text-left">
                <tr>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('id')">
                      ID
                      <ArrowUpIcon v-if="sortKey === 'id' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'id'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('inn')">
                      INN
                      <ArrowUpIcon v-if="sortKey === 'inn' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'inn'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('fullName')">
                      Full name
                      <ArrowUpIcon v-if="sortKey === 'fullName' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'fullName'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('gender')">
                      Gender
                      <ArrowUpIcon v-if="sortKey === 'gender' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'gender'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('birthDate')">
                      Birth date
                      <ArrowUpIcon v-if="sortKey === 'birthDate' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'birthDate'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('phone')">
                      Contact
                      <ArrowUpIcon v-if="sortKey === 'phone' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'phone'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('regionId')">
                      Region
                      <ArrowUpIcon v-if="sortKey === 'regionId' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'regionId'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('groupName')">
                      Group
                      <ArrowUpIcon v-if="sortKey === 'groupName' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'groupName'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('createdAt')">
                      Registered
                      <ArrowUpIcon v-if="sortKey === 'createdAt' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'createdAt'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 font-medium">
                    <button class="flex items-center gap-1" type="button" @click="toggleSort('isActive')">
                      Status
                      <ArrowUpIcon v-if="sortKey === 'isActive' && sortDirection === 'asc'" class="size-3" />
                      <ArrowDownIcon v-else-if="sortKey === 'isActive'" class="size-3" />
                      <ArrowUpDownIcon v-else class="size-3 opacity-50" />
                    </button>
                  </th>
                  <th class="px-3 py-2 text-right font-medium">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="loading">
                  <td colspan="11" class="px-3 py-8 text-center text-muted-foreground">
                    Loading patients...
                  </td>
                </tr>
                <tr v-else-if="sortedPatients.length === 0">
                  <td colspan="11" class="px-3 py-8 text-center text-muted-foreground">
                    No patients found.
                  </td>
                </tr>
                <template v-else>
                  <tr
                    v-for="patient in pagedPatients"
                    :key="patient.id"
                    class="border-t"
                  >
                    <td class="px-3 py-2 font-mono text-xs text-muted-foreground">
                      #{{ patient.id }}
                    </td>
                    <td class="px-3 py-2 font-mono">
                      {{ patient.inn }}
                    </td>
                    <td class="px-3 py-2">
                      <div class="font-medium">
                        {{ getFullName(patient) }}
                      </div>
                      <div class="max-w-[260px] truncate text-xs text-muted-foreground">
                        {{ patient.address }}
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      {{ getGenderLabel(patient.gender) }}
                    </td>
                    <td class="px-3 py-2">
                      {{ formatDate(patient.birthDate) }}
                    </td>
                    <td class="px-3 py-2">
                      <div>{{ patient.phone }}</div>
                      <div class="max-w-[220px] truncate text-xs text-muted-foreground">
                        {{ patient.address2 }}
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      <div>Region {{ patient.regionId }}</div>
                      <div class="text-xs text-muted-foreground">
                        District {{ patient.districtId }}
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      {{ patient.groupName }}
                    </td>
                    <td class="px-3 py-2">
                      {{ formatDate(patient.createdAt) }}
                    </td>
                    <td class="px-3 py-2">
                      <div class="flex flex-wrap gap-2">
                        <UiBadge :variant="patient.isActive ? 'default' : 'secondary'">
                          {{ patient.isActive ? 'Active' : 'Inactive' }}
                        </UiBadge>
                        <UiBadge v-if="patient.specialStatus" variant="destructive">
                          Special
                        </UiBadge>
                      </div>
                    </td>
                    <td class="px-3 py-2">
                      <div class="flex justify-end gap-2">
                        <UiButton size="sm" variant="outline" @click="editPatient(patient)">
                          <EditIcon class="size-4" />
                        </UiButton>
                        <UiButton
                          size="sm"
                          variant="destructive"
                          :disabled="deletingId === patient.id"
                          @click="deletePatient(patient)"
                        >
                          <Trash2Icon class="size-4" />
                        </UiButton>
                      </div>
                    </td>
                  </tr>
                </template>
              </tbody>
            </table>
          </div>

          <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
            <div class="text-sm text-muted-foreground">
              Showing {{ rowStart }}-{{ rowEnd }} of {{ sortedPatients.length }} filtered patients
              <span v-if="sortedPatients.length !== patients.length">
                from {{ patients.length }} total
              </span>
            </div>
            <div class="flex flex-wrap items-center gap-2">
              <select
                v-model.number="pageSize"
                class="h-9 rounded-md border bg-background px-3 text-sm"
              >
                <option
                  v-for="size in pageSizeOptions"
                  :key="size"
                  :value="size"
                >
                  {{ size }} / page
                </option>
              </select>
              <UiButton size="sm" variant="outline" :disabled="page === 1" @click="goToPage(1)">
                <ChevronsLeftIcon class="size-4" />
              </UiButton>
              <UiButton size="sm" variant="outline" :disabled="page === 1" @click="goToPage(page - 1)">
                <ChevronLeftIcon class="size-4" />
              </UiButton>
              <div class="min-w-24 text-center text-sm">
                Page {{ page }} / {{ totalPages }}
              </div>
              <UiButton size="sm" variant="outline" :disabled="page === totalPages" @click="goToPage(page + 1)">
                <ChevronRightIcon class="size-4" />
              </UiButton>
              <UiButton size="sm" variant="outline" :disabled="page === totalPages" @click="goToPage(totalPages)">
                <ChevronsRightIcon class="size-4" />
              </UiButton>
            </div>
          </div>
        </UiCardContent>
      </UiCard>
    </div>

    <UiDialog v-model:open="isFormOpen">
      <UiDialogContent class="max-h-[90vh] overflow-y-auto sm:max-w-3xl">
        <UiDialogHeader>
          <UiDialogTitle>{{ formTitle }}</UiDialogTitle>
          <UiDialogDescription>
            Identity and contact fields are required.
          </UiDialogDescription>
        </UiDialogHeader>

        <form class="space-y-4" @submit.prevent="savePatient">
          <div class="grid gap-2">
            <UiLabel for="inn">
              INN
            </UiLabel>
            <UiInput id="inn" v-model="form.inn" maxlength="14" required />
          </div>
          <div class="grid gap-3 md:grid-cols-2">
            <div class="grid gap-2">
              <UiLabel for="lastName">
                Last name
              </UiLabel>
              <UiInput id="lastName" v-model="form.lastName" required />
            </div>
            <div class="grid gap-2">
              <UiLabel for="firstName">
                First name
              </UiLabel>
              <UiInput id="firstName" v-model="form.firstName" required />
            </div>
          </div>
          <div class="grid gap-2">
            <UiLabel for="middleName">
              Middle name
            </UiLabel>
            <UiInput id="middleName" v-model="form.middleName" required />
          </div>
          <div class="grid gap-3 md:grid-cols-2">
            <div class="grid gap-2">
              <UiLabel for="birthDate">
                Birth date
              </UiLabel>
              <UiInput id="birthDate" v-model="form.birthDate" type="date" required />
            </div>
            <div class="grid gap-2">
              <UiLabel for="gender">
                Gender
              </UiLabel>
              <select id="gender" v-model="form.gender" class="h-9 rounded-md border bg-background px-3 text-sm">
                <option
                  v-for="gender in genderOptions"
                  :key="gender.value"
                  :value="gender.value"
                >
                  {{ gender.label }}
                </option>
              </select>
            </div>
          </div>
          <div class="grid gap-2">
            <UiLabel for="phone">
              Phone
            </UiLabel>
            <UiInput id="phone" v-model="form.phone" required />
          </div>
          <div class="grid gap-2">
            <UiLabel for="address">
              Address
            </UiLabel>
            <UiInput id="address" v-model="form.address" required />
          </div>
          <div class="grid gap-2">
            <UiLabel for="address2">
              Address 2
            </UiLabel>
            <UiInput id="address2" v-model="form.address2" required />
          </div>
          <div class="grid gap-3 md:grid-cols-2">
            <div class="grid gap-2">
              <UiLabel for="regionId">
                Region ID
              </UiLabel>
              <UiInput id="regionId" v-model="form.regionId" type="number" min="1" required />
            </div>
            <div class="grid gap-2">
              <UiLabel for="districtId">
                District ID
              </UiLabel>
              <UiInput id="districtId" v-model="form.districtId" type="number" min="1" required />
            </div>
          </div>
          <div v-if="editingId" class="grid gap-2">
            <UiLabel for="groupId">
              Group
            </UiLabel>
            <select id="groupId" v-model="form.groupId" class="h-9 rounded-md border bg-background px-3 text-sm">
              <option
                v-for="group in patientGroupOptions"
                :key="group.value"
                :value="group.value"
              >
                {{ group.label }}
              </option>
            </select>
          </div>
          <div class="flex flex-wrap gap-4 rounded-md border p-3">
            <label class="flex items-center gap-2 text-sm">
              <input v-model="form.specialStatus" type="checkbox" class="size-4">
              Special status
            </label>
            <label v-if="editingId" class="flex items-center gap-2 text-sm">
              <input v-model="form.isActive" type="checkbox" class="size-4">
              Active
            </label>
          </div>
          <UiDialogFooter>
            <UiButton type="button" variant="outline" @click="resetForm">
              <XIcon class="mr-2 size-4" />
              Clear
            </UiButton>
            <UiButton type="submit" :disabled="saving">
              <SaveIcon class="mr-2 size-4" />
              Save
            </UiButton>
          </UiDialogFooter>
        </form>
      </UiDialogContent>
    </UiDialog>
  </BasicPage>
</template>
