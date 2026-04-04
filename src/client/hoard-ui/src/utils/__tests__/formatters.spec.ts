import { describe, it, expect, vi, afterEach } from 'vitest'
import {
  formatCurrency,
  formatPercentage,
  formatUpdatedTime,
  formatDate,
  formatDateTime,
  formatUnits,
  getTrendClass,
} from '../formatters'

describe('formatCurrency', () => {
  it('should format positive values as GBP', () => {
    expect(formatCurrency(1234.56)).toBe('£1,234.56')
  })

  it('should format negative values as absolute', () => {
    expect(formatCurrency(-1234.56)).toBe('£1,234.56')
  })

  it('should format zero', () => {
    expect(formatCurrency(0)).toBe('£0.00')
  })

  it('should return empty string for null', () => {
    expect(formatCurrency(null)).toBe('')
  })

  it('should return empty string for undefined', () => {
    expect(formatCurrency(undefined)).toBe('')
  })

  it('should return empty string for NaN', () => {
    expect(formatCurrency(NaN)).toBe('')
  })
})

describe('formatPercentage', () => {
  it('should format with two decimal places', () => {
    expect(formatPercentage(12.3456)).toBe('12.35%')
  })

  it('should format negative values as absolute', () => {
    expect(formatPercentage(-5.1)).toBe('5.10%')
  })

  it('should format zero', () => {
    expect(formatPercentage(0)).toBe('0.00%')
  })

  it('should return empty string for null', () => {
    expect(formatPercentage(null)).toBe('')
  })

  it('should return empty string for undefined', () => {
    expect(formatPercentage(undefined)).toBe('')
  })

  it('should return empty string for NaN', () => {
    expect(formatPercentage(NaN)).toBe('')
  })
})

describe('formatUpdatedTime', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('should return time for dates within last 24 hours', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-04-04T15:00:00Z'))

    const recent = new Date('2026-04-04T14:30:00Z')
    expect(formatUpdatedTime(recent)).toBe('14:30')
  })

  it('should return date for dates older than 24 hours', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-04-04T15:00:00Z'))

    const old = new Date('2026-01-23T10:00:00Z')
    expect(formatUpdatedTime(old)).toBe('23 Jan 2026')
  })

  it('should accept ISO string input', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-04-04T15:00:00Z'))

    expect(formatUpdatedTime('2026-01-23T10:00:00Z')).toBe('23 Jan 2026')
  })

  it('should return empty string for null', () => {
    expect(formatUpdatedTime(null)).toBe('')
  })

  it('should return empty string for undefined', () => {
    expect(formatUpdatedTime(undefined)).toBe('')
  })

  it('should return empty string for invalid date string', () => {
    expect(formatUpdatedTime('not-a-date')).toBe('')
  })
})

describe('formatDate', () => {
  it('should format a Date object', () => {
    expect(formatDate(new Date('2026-01-15T00:00:00Z'))).toBe('15 Jan 2026')
  })

  it('should format an ISO string', () => {
    expect(formatDate('2026-01-15')).toBe('15 Jan 2026')
  })

  it('should return empty string for null', () => {
    expect(formatDate(null)).toBe('')
  })

  it('should return empty string for invalid date', () => {
    expect(formatDate('garbage')).toBe('')
  })
})

describe('formatDateTime', () => {
  it('should format date and time', () => {
    expect(formatDateTime(new Date('2026-01-15T14:30:00Z'))).toBe('15 Jan 2026 14:30')
  })

  it('should accept ISO string', () => {
    expect(formatDateTime('2026-01-15T14:30:00Z')).toBe('15 Jan 2026 14:30')
  })

  it('should return empty string for null', () => {
    expect(formatDateTime(null)).toBe('')
  })

  it('should return empty string for invalid date', () => {
    expect(formatDateTime('garbage')).toBe('')
  })
})

describe('formatUnits', () => {
  it('should format whole numbers', () => {
    expect(formatUnits(1234)).toBe('1,234')
  })

  it('should format decimals up to 6 places', () => {
    expect(formatUnits(1.123456)).toBe('1.123456')
  })

  it('should strip trailing zeros', () => {
    expect(formatUnits(1.1)).toBe('1.1')
  })

  it('should use absolute value', () => {
    expect(formatUnits(-500)).toBe('500')
  })

  it('should return em dash for null', () => {
    expect(formatUnits(null)).toBe('—')
  })

  it('should return em dash for undefined', () => {
    expect(formatUnits(undefined)).toBe('—')
  })
})

describe('getTrendClass', () => {
  it('should return text-positive for positive values', () => {
    expect(getTrendClass(1)).toBe('text-positive')
  })

  it('should return text-negative for negative values', () => {
    expect(getTrendClass(-1)).toBe('text-negative')
  })

  it('should return undefined for zero', () => {
    expect(getTrendClass(0)).toBeUndefined()
  })

  it('should return undefined for null', () => {
    expect(getTrendClass(null)).toBeUndefined()
  })

  it('should return undefined for undefined', () => {
    expect(getTrendClass(undefined)).toBeUndefined()
  })
})
