import { describe, expect, it } from 'vitest'

import { formatDate } from './dialysis'

describe('dialysis date formatting', () => {
  it('formats date-only values as DD.MM.YYYY without timezone conversion', () => {
    expect(formatDate('2000-01-02')).toBe('02.01.2000')
    expect(formatDate('2000-01-02T23:30:00Z')).toBe('02.01.2000')
  })

  it('handles missing and unrecognized values safely', () => {
    expect(formatDate(null)).toBe('-')
    expect(formatDate(undefined)).toBe('-')
    expect(formatDate('not-a-date')).toBe('not-a-date')
  })
})
