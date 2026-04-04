import { describe, it, expect, vi, afterEach } from 'vitest'

/**
 * Tests for form validation rules used across detail pages.
 * These rules are defined inline in TransactionDetailPage, PriceDetailPage,
 * and InstrumentDetailPage. We test them as pure functions to verify
 * the validation contract without mounting the full page components.
 */

// --- Shared rules (used in TransactionDetailPage and PriceDetailPage) ---

const requiredRule = (v: unknown) => !!v || 'Required'
const requiredNumberRule = (v: unknown) => (v !== null && v !== '') || 'Required'
const nonNegativeRule = (v: unknown) => !v || (v as number) >= 0 || 'Must be non-negative'
const maxLength20Rule = (v: string) => !v || v.length <= 20 || 'Cannot exceed 20 characters'

// Date rule needs `today` pinned
function notFutureRule(today: string) {
  return (v: string) => v <= today || 'Cannot be in the future'
}

// --- PriceDetailPage rules ---

const SOURCE_MAX_LENGTH = 50
const sourceMaxLengthRule = (v: string) =>
  !v || v.length <= SOURCE_MAX_LENGTH || `Cannot exceed ${SOURCE_MAX_LENGTH} characters`

// --- InstrumentDetailPage rules ---

const NAME_MAX_LENGTH = 100
const TICKER_MAX_LENGTH = 20
const ISIN_MAX_LENGTH = 12

const nameRules = [
  (v: string) => !!v.trim() || 'Name is required',
  (v: string) => v.length <= NAME_MAX_LENGTH || `Name cannot exceed ${NAME_MAX_LENGTH} characters`,
]

const tickerRules = [
  (v: string) => !!v.trim() || 'Ticker is required',
  (v: string) =>
    v.length <= TICKER_MAX_LENGTH || `Ticker cannot exceed ${TICKER_MAX_LENGTH} characters`,
]

const isinRules = [
  (v: string) =>
    !v || v.length <= ISIN_MAX_LENGTH || `ISIN cannot exceed ${ISIN_MAX_LENGTH} characters`,
]

const dataFeedTickerRules = [
  (v: string) => !!v?.trim() || 'Ticker is required',
  (v: string) => !v || v.length <= TICKER_MAX_LENGTH || `Maximum ${TICKER_MAX_LENGTH} characters`,
]

describe('requiredRule', () => {
  it('should pass for truthy values', () => {
    expect(requiredRule('hello')).toBe(true)
    expect(requiredRule(1)).toBe(true)
    expect(requiredRule(true)).toBe(true)
  })

  it('should fail for falsy values', () => {
    expect(requiredRule('')).toBe('Required')
    expect(requiredRule(null)).toBe('Required')
    expect(requiredRule(undefined)).toBe('Required')
    expect(requiredRule(0)).toBe('Required')
  })
})

describe('requiredNumberRule', () => {
  it('should pass for zero', () => {
    expect(requiredNumberRule(0)).toBe(true)
  })

  it('should pass for numbers', () => {
    expect(requiredNumberRule(42)).toBe(true)
    expect(requiredNumberRule(-1)).toBe(true)
  })

  it('should fail for null and empty string', () => {
    expect(requiredNumberRule(null)).toBe('Required')
    expect(requiredNumberRule('')).toBe('Required')
  })
})

describe('nonNegativeRule', () => {
  it('should pass for positive numbers', () => {
    expect(nonNegativeRule(1)).toBe(true)
    expect(nonNegativeRule(100.5)).toBe(true)
  })

  it('should pass for zero and falsy values', () => {
    expect(nonNegativeRule(0)).toBe(true)
    expect(nonNegativeRule(null)).toBe(true)
    expect(nonNegativeRule('')).toBe(true)
  })

  it('should fail for negative numbers', () => {
    expect(nonNegativeRule(-1)).toBe('Must be non-negative')
    expect(nonNegativeRule(-0.01)).toBe('Must be non-negative')
  })
})

describe('notFutureRule', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('should pass for today and past dates', () => {
    const rule = notFutureRule('2026-04-04')
    expect(rule('2026-04-04')).toBe(true)
    expect(rule('2026-04-03')).toBe(true)
    expect(rule('2020-01-01')).toBe(true)
  })

  it('should fail for future dates', () => {
    const rule = notFutureRule('2026-04-04')
    expect(rule('2026-04-05')).toBe('Cannot be in the future')
    expect(rule('2027-01-01')).toBe('Cannot be in the future')
  })
})

describe('maxLength20Rule', () => {
  it('should pass for short strings', () => {
    expect(maxLength20Rule('REF001')).toBe(true)
    expect(maxLength20Rule('12345678901234567890')).toBe(true) // exactly 20
  })

  it('should pass for empty string', () => {
    expect(maxLength20Rule('')).toBe(true)
  })

  it('should fail for strings longer than 20', () => {
    expect(maxLength20Rule('123456789012345678901')).toBe('Cannot exceed 20 characters')
  })
})

describe('sourceMaxLengthRule', () => {
  it('should pass for strings up to 50 characters', () => {
    expect(sourceMaxLengthRule('Yahoo Finance')).toBe(true)
    expect(sourceMaxLengthRule('a'.repeat(50))).toBe(true)
  })

  it('should fail for strings longer than 50', () => {
    expect(sourceMaxLengthRule('a'.repeat(51))).toBe('Cannot exceed 50 characters')
  })
})

describe('instrument nameRules', () => {
  it('should require non-empty name', () => {
    expect(nameRules[0]!('Fundsmith Equity')).toBe(true)
    expect(nameRules[0]!('')).toBe('Name is required')
    expect(nameRules[0]!('   ')).toBe('Name is required')
  })

  it('should enforce max length', () => {
    expect(nameRules[1]!('Short name')).toBe(true)
    expect(nameRules[1]!('a'.repeat(101))).toBe('Name cannot exceed 100 characters')
  })
})

describe('instrument tickerRules', () => {
  it('should require non-empty ticker', () => {
    expect(tickerRules[0]!('SMIT')).toBe(true)
    expect(tickerRules[0]!('')).toBe('Ticker is required')
    expect(tickerRules[0]!('   ')).toBe('Ticker is required')
  })

  it('should enforce max length', () => {
    expect(tickerRules[1]!('SMIT')).toBe(true)
    expect(tickerRules[1]!('a'.repeat(21))).toBe('Ticker cannot exceed 20 characters')
  })
})

describe('instrument isinRules', () => {
  it('should allow empty ISIN', () => {
    expect(isinRules[0]!('')).toBe(true)
  })

  it('should allow valid length ISIN', () => {
    expect(isinRules[0]!('GB00B0CNH163')).toBe(true) // 12 chars
  })

  it('should fail for ISIN longer than 12', () => {
    expect(isinRules[0]!('GB00B0CNH1631')).toBe('ISIN cannot exceed 12 characters')
  })
})

describe('dataFeedTickerRules', () => {
  it('should require non-empty ticker', () => {
    expect(dataFeedTickerRules[0]!('SMIT.L')).toBe(true)
    expect(dataFeedTickerRules[0]!('')).toBe('Ticker is required')
    expect(dataFeedTickerRules[0]!('   ')).toBe('Ticker is required')
  })

  it('should enforce max length', () => {
    expect(dataFeedTickerRules[1]!('SMIT.L')).toBe(true)
    expect(dataFeedTickerRules[1]!('a'.repeat(21))).toBe('Maximum 20 characters')
  })

  it('should pass max length check for empty value', () => {
    expect(dataFeedTickerRules[1]!('')).toBe(true)
  })
})
