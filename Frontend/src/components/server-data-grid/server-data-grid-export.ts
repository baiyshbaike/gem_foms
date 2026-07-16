import type {
  ServerDataGridExportColumn,
  ServerDataGridGroupSummary,
} from './server-data-grid.types'

interface CreateWorkbookOptions<TData> {
  columns: ServerDataGridExportColumn<TData>[]
  fileName: string
  groupBy: string | null
  rows: TData[]
  sheetName: string
  summaries: ServerDataGridGroupSummary[]
  visibleColumnIds: string[]
}

export async function createServerDataGridWorkbook<TData extends object>(
  options: CreateWorkbookOptions<TData>,
) {
  const { Workbook } = await import('exceljs')
  const workbook = new Workbook()
  const worksheet = workbook.addWorksheet(options.sheetName, {
    views: [{ state: 'frozen', ySplit: 1 }],
  })
  const visibleColumnIds = new Set(options.visibleColumnIds)
  const visibleColumns = options.columns.filter(column => visibleColumnIds.has(column.id))
  const exportColumns = visibleColumns.length > 0 ? visibleColumns : options.columns.slice(0, 2)

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
  for (const item of options.rows) {
    if (options.groupBy) {
      const rawGroupValue = (item as Record<string, unknown>)[options.groupBy]
      const groupKey = typeof rawGroupValue === 'boolean'
        ? String(rawGroupValue).toLowerCase()
        : String(rawGroupValue)

      if (groupKey !== previousGroupKey) {
        const summary = options.summaries.find(candidate => candidate.key === groupKey)
        const groupRow = worksheet.addRow([`${summary?.label ?? rawGroupValue} (${summary?.count ?? 0})`])
        if (exportColumns.length > 1) {
          worksheet.mergeCells(groupRow.number, 1, groupRow.number, exportColumns.length)
        }
        groupRow.font = { bold: true }
        groupRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE2E8F0' } }
        previousGroupKey = groupKey
      }
    }

    worksheet.addRow(exportColumns.map(column => column.value(item)))
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
    options.fileName,
  )
}

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = filename
  anchor.click()
  URL.revokeObjectURL(url)
}
