import { describe, expect, it } from 'vitest'

import {
  APP_LOCALE,
  APP_TIME_ZONE,
  formatDate,
  formatDateTime,
  toDateTimeLocal,
} from './dialysis'

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

  it('uses the application-wide Bishkek timezone and Russian locale', () => {
    expect(APP_LOCALE).toBe('ru-RU')
    expect(APP_TIME_ZONE).toBe('Asia/Bishkek')
    expect(toDateTimeLocal('2026-07-16T19:30:00Z')).toBe('2026-07-17T01:30')
    expect(formatDateTime('2026-07-16T19:30:00Z')).toContain('01:30')
  })
})
