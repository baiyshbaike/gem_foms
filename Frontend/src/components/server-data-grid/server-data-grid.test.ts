import { describe, expect, it } from 'vitest'

import type {
  ServerDataGridFilter,
  ServerDataGridFilterField,
} from './server-data-grid.types'

import {
  createServerDataGridQueryRequest,
  formatServerDataGridFilter,
} from './server-data-grid.utils'

describe('server data grid helpers', () => {
  it('normalizes a remote query and enforces page limits', () => {
    expect(createServerDataGridQueryRequest({
      pageIndex: -3,
      pageSize: 500,
      search: '  dialysis  ',
      sorting: [{ id: 'createdAt', desc: true }],
      filters: [{
        field: 'status',
        operator: 'equals',
        value: ' active ',
        valueTo: ' ',
      }],
      groupBy: '',
    })).toEqual({
      page: 1,
      pageSize: 100,
      search: 'dialysis',
      sorting: [{ field: 'createdAt', descending: true }],
      filters: [{
        field: 'status',
        operator: 'equals',
        value: 'active',
        valueTo: null,
      }],
      groupBy: null,
    })
  })

  it('uses configured option labels in active filter text', () => {
    const fields: ServerDataGridFilterField[] = [{
      field: 'status',
      label: 'Status',
      type: 'select',
      options: [{ label: 'In progress', value: 'active' }],
    }]
    const filter: ServerDataGridFilter = {
      field: 'status',
      operator: 'equals',
      value: 'active',
      valueTo: null,
    }

    expect(formatServerDataGridFilter(filter, fields)).toBe('Status is In progress')
  })

  it('formats ranges and value-free operators', () => {
    const fields: ServerDataGridFilterField[] = [{
      field: 'createdAt',
      label: 'Created',
      type: 'date',
    }]

    expect(formatServerDataGridFilter({
      field: 'createdAt',
      operator: 'between',
      value: '2026-01-01',
      valueTo: '2026-01-31',
    }, fields)).toBe('Created between 2026-01-01 - 2026-01-31')

    expect(formatServerDataGridFilter({
      field: 'createdAt',
      operator: 'isEmpty',
      value: null,
      valueTo: null,
    }, fields)).toBe('Created is empty')
  })
})
